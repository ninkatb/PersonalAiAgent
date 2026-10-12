#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod navigation;
mod owned_process;
mod power;
mod profile;
mod transport;
use std::sync::Mutex;
use tauri::{Emitter, Manager};

struct BackendState(Mutex<Option<transport::BackendSession>>);
struct SessionGuard(std::sync::Arc<transport::Revocation>);

fn health_arguments_allowed(body: &tauri::ipc::InvokeBody) -> bool {
    matches!(body, tauri::ipc::InvokeBody::Json(value) if value.as_object().is_some_and(|object| object.is_empty()))
}

#[tauri::command]
async fn backend_health(
    app: tauri::AppHandle,
    window: tauri::WebviewWindow,
    request: tauri::ipc::Request<'_>,
) -> Result<transport::HealthResult, &'static str> {
    if window.label() != "main"
        || !health_arguments_allowed(request.body())
        || !navigation::is_allowed(
            &window.url().map_err(|_| "BACKEND_UNAVAILABLE")?,
            cfg!(debug_assertions),
        )
    {
        return Err("BACKEND_UNAVAILABLE");
    }
    tauri::async_runtime::spawn_blocking(move || {
        let state = app.state::<BackendState>();
        let guard = state.0.lock().map_err(|_| "BACKEND_UNAVAILABLE")?;
        guard.as_ref().ok_or("BACKEND_UNAVAILABLE")?.health()
    })
    .await
    .map_err(|_| "BACKEND_UNAVAILABLE")?
}

fn main() {
    let app = tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![backend_health])
        .setup(|app| {
            let sidecar = std::env::current_exe()?
                .parent()
                .ok_or("SIDECAR_PATH_INVALID")?
                .join("sidecar")
                .join("PersonalAI.Api.exe");
            let profile = app
                .path()
                .app_local_data_dir()?
                .join("profiles")
                .join("default");
            let profile = std::sync::Arc::new(profile::Profile::acquire(&profile)?);
            app.manage(profile.clone());
            let session = transport::BackendSession::start_with_profile(&sidecar, profile).ok();
            let revocation = session.as_ref().map(|session| session.revocation.clone());
            if let Some(session) = &session {
                let handle = app.handle().clone();
                session.monitor_exit(move || {
                    let _ = handle.emit("backend-unavailable", ());
                })?;
            }
            app.manage(BackendState(Mutex::new(session)));
            let config = app
                .config()
                .app
                .windows
                .iter()
                .find(|window| window.label == "main")
                .ok_or("DESKTOP_WINDOW_NOT_CONFIGURED")?;
            let window = tauri::WebviewWindowBuilder::from_config(app, config)?
                .on_navigation(|url| navigation::is_allowed(url, cfg!(debug_assertions)))
                .on_document_title_changed(|window, title| {
                    // The local document title already comes from the active catalog.
                    if window.set_title(&title).is_err() {
                        eprintln!("DESKTOP_TITLE_UPDATE_FAILED");
                    }
                })
                .build()?;
            if let Some(revocation) = revocation {
                if let Err(error) = power::install(&window, revocation.clone()) {
                    revocation.revoke();
                    return Err(error.into());
                }
                app.manage(SessionGuard(revocation));
            }
            Ok(())
        })
        .build(tauri::generate_context!())
        .expect("DESKTOP_START_FAILED");
    app.run(|app, event| {
        if matches!(
            event,
            tauri::RunEvent::ExitRequested { .. } | tauri::RunEvent::Exit
        ) && let Some(guard) = app.try_state::<SessionGuard>()
        {
            guard.0.revoke();
        }
        if matches!(event, tauri::RunEvent::Exit)
            && let Ok(mut state) = app.state::<BackendState>().0.lock()
        {
            state.take();
        }
    });
}

#[cfg(test)]
mod ipc_tests {
    #[test]
    fn health_rejects_arbitrary_transport_arguments_and_raw_payloads() {
        use tauri::ipc::InvokeBody;
        assert!(super::health_arguments_allowed(&InvokeBody::Json(
            serde_json::json!({})
        )));
        for value in [
            serde_json::json!({"url":"http://evil.example"}),
            serde_json::json!({"method":"POST"}),
            serde_json::json!({"headers":{}}),
            serde_json::json!({"path":"/debug"}),
            serde_json::json!({"command":"SYNTHETIC"}),
            serde_json::json!(null),
            serde_json::json!([]),
        ] {
            assert!(!super::health_arguments_allowed(&InvokeBody::Json(value)));
        }
        assert!(!super::health_arguments_allowed(&InvokeBody::Raw(vec![])));
    }
}
