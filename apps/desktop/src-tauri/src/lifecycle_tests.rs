use super::*;
use std::process::{Command, Stdio};
use std::sync::Barrier;
use std::time::Instant;

fn executable() -> std::path::PathBuf {
    std::env::current_exe()
        .unwrap()
        .parent()
        .unwrap()
        .parent()
        .unwrap()
        .join("sidecar/PersonalAI.Api.exe")
}
fn directory() -> std::path::PathBuf {
    super::tests::test_profile()
}
fn wait_until(mut predicate: impl FnMut() -> bool) {
    let deadline = Instant::now() + Duration::from_secs(8);
    while !predicate() {
        assert!(
            Instant::now() < deadline,
            "Lifecycle operation did not complete"
        );
        std::thread::sleep(Duration::from_millis(20));
    }
}

#[test]
fn failed_startup_keeps_the_desktop_profile_lease_until_owner_exits() {
    let directory = directory();
    let profile = Arc::new(Profile::acquire(&directory).unwrap());
    assert!(
        BackendSession::start_with_profile(Path::new("SYNTHETIC_MISSING.exe"), profile.clone())
            .is_err()
    );
    assert!(Profile::acquire(&directory).is_err());
    drop(profile);
    assert!(Profile::acquire(&directory).is_ok());
}

#[test]
fn concurrent_same_profile_starts_have_exactly_one_owner() {
    let directory = directory();
    std::fs::create_dir_all(&directory).unwrap();
    std::fs::write(directory.join("user-data.txt"), "SYNTHETIC_KEEP").unwrap();
    let barrier = Arc::new(Barrier::new(3));
    let workers: Vec<_> = (0..2)
        .map(|_| {
            let barrier = barrier.clone();
            let directory = directory.clone();
            std::thread::spawn(move || {
                barrier.wait();
                BackendSession::start(&executable(), &directory)
            })
        })
        .collect();
    barrier.wait();
    let results: Vec<_> = workers
        .into_iter()
        .map(|worker| worker.join().unwrap())
        .collect();
    assert_eq!(results.iter().filter(|result| result.is_ok()).count(), 1);
    assert_eq!(
        results
            .iter()
            .filter(|result| matches!(result, Err("PROFILE_UNAVAILABLE")))
            .count(),
        1
    );
    assert!(Profile::acquire(&directory).is_err());
    drop(results);
    wait_until(|| Profile::acquire(&directory).is_ok());
    assert_eq!(
        std::fs::read_to_string(directory.join("user-data.txt")).unwrap(),
        "SYNTHETIC_KEEP"
    );
    assert!(directory.join("instance.lock").exists());
}

#[test]
fn restart_rejects_old_credentials_and_closes_old_channel() {
    let directory = directory();
    let mut first = BackendSession::start(&executable(), &directory).unwrap();
    let old_token = first.token.clone();
    let old_origin = first.origin.clone();
    let old_port = first.port;
    first.shutdown();
    assert!(first.input.is_none());
    assert!(first.health().is_err());
    drop(first);
    let second = BackendSession::start(&executable(), &directory).unwrap();
    assert!(old_token != second.token); // Never print even test transport credentials.
    assert!(old_origin != second.origin);
    let endpoint = format!("http://127.0.0.1:{}/health", second.port);
    let response = second
        .client
        .get(&endpoint)
        .header("Origin", &second.origin)
        .bearer_auth(old_token)
        .send()
        .unwrap();
    assert_eq!(response.status(), reqwest::StatusCode::UNAUTHORIZED);
    let response = second
        .client
        .get(&endpoint)
        .header("Origin", old_origin)
        .bearer_auth(&second.token)
        .send()
        .unwrap();
    assert_eq!(response.status(), reqwest::StatusCode::FORBIDDEN);
    if old_port != second.port {
        assert!(std::net::TcpStream::connect((std::net::Ipv4Addr::LOCALHOST, old_port)).is_err());
    }
    assert!(second.health().is_ok());
}

#[test]
fn profiles_have_distinct_locks_paths_channels_and_credentials() {
    let first_directory = directory();
    let first = BackendSession::start(&executable(), &first_directory).unwrap();
    let second = BackendSession::start(&executable(), &directory()).unwrap();
    assert_ne!(first.port, second.port);
    assert_ne!(first._profile.bootstrap, second._profile.bootstrap);
    assert!(!first._profile.bootstrap.exists());
    for (target, source) in [(&first, &second), (&second, &first)] {
        let response = target
            .client
            .get(format!("http://127.0.0.1:{}/health", target.port))
            .header("Origin", &target.origin)
            .bearer_auth(&source.token)
            .send()
            .unwrap();
        assert_eq!(response.status(), reqwest::StatusCode::UNAUTHORIZED);
    }
    first.revocation.revoke();
    assert!(first.health().is_err());
    assert!(second.health().is_ok());
}

#[test]
fn channel_eof_and_unexpected_input_end_only_the_owned_session() {
    for unexpected_input in [false, true] {
        let mut session = BackendSession::start(&executable(), &directory()).unwrap();
        if unexpected_input {
            session.input.as_mut().unwrap().write_all(b"x").unwrap();
        } else {
            session.input.take();
        }
        wait_until(|| session.child.exited());
        assert!(session.health().is_err());
    }
}

#[test]
fn revocation_is_terminal_and_interrupts_health_without_restarting() {
    let session = Arc::new(BackendSession::start(&executable(), &directory()).unwrap());
    let worker_session = session.clone();
    let worker = std::thread::spawn(move || {
        for _ in 0..100 {
            if worker_session.health().is_err() {
                return;
            }
        }
        panic!("Revocation did not interrupt operations");
    });
    session.revocation.revoke();
    session.revocation.revoke();
    worker.join().unwrap();
    wait_until(|| session.child.exited());
    assert!(session.health().is_err());
}

// This harness is compiled only by cargo test. It is absent from the desktop package.
#[test]
fn lifecycle_child_harness() {
    let Ok(profile) = std::env::var("PERSONAL_AI_TEST_PROFILE") else {
        return;
    };
    let profile = std::path::PathBuf::from(profile);
    let metadata = profile.join("harness-ready.json");
    if std::env::var("PERSONAL_AI_TEST_MODE").as_deref() == Ok("startup") {
        let lease = Profile::acquire(&profile).unwrap();
        let child = OwnedProcess::spawn(&executable(), &lease.lock).unwrap();
        std::fs::write(
            metadata,
            serde_json::json!({"pid":child.id(), "port":0}).to_string(),
        )
        .unwrap();
        let _ = std::io::stdin().read(&mut [0u8]);
        drop(child);
    } else {
        let session = BackendSession::start(&executable(), &profile).unwrap();
        std::fs::write(
            metadata,
            serde_json::json!({"pid":session.child.id(), "port":session.port}).to_string(),
        )
        .unwrap();
        let _ = std::io::stdin().read(&mut [0u8]);
        drop(session);
    }
}

#[test]
fn abrupt_parent_death_cleans_ready_and_prehandshake_child_and_keeps_unrelated_process() {
    use std::os::windows::io::{AsRawHandle, FromRawHandle, OwnedHandle};
    use windows_sys::Win32::Foundation::WAIT_OBJECT_0;
    use windows_sys::Win32::System::Threading::{
        OpenProcess, PROCESS_SYNCHRONIZE, WaitForSingleObject,
    };
    let gate = crate::owned_process::SPAWN_GATE.lock().unwrap();
    let mut unrelated = Command::new(std::env::current_exe().unwrap())
        .args([
            "--exact",
            "transport::lifecycle_tests::lifecycle_child_harness",
            "--nocapture",
        ])
        .env("PERSONAL_AI_TEST_PROFILE", directory())
        .stdin(Stdio::piped())
        .stdout(Stdio::null())
        .stderr(Stdio::null())
        .spawn()
        .unwrap();
    drop(gate);
    for mode in ["ready", "startup"] {
        let profile = directory();
        std::fs::create_dir_all(&profile).unwrap();
        let gate = crate::owned_process::SPAWN_GATE.lock().unwrap();
        let mut parent = Command::new(std::env::current_exe().unwrap())
            .args([
                "--exact",
                "transport::lifecycle_tests::lifecycle_child_harness",
                "--nocapture",
            ])
            .env("PERSONAL_AI_TEST_PROFILE", &profile)
            .env("PERSONAL_AI_TEST_MODE", mode)
            .stdin(Stdio::piped())
            .stdout(Stdio::null())
            .stderr(Stdio::null())
            .spawn()
            .unwrap();
        drop(gate);
        let metadata = profile.join("harness-ready.json");
        wait_until(|| {
            std::fs::read(&metadata)
                .ok()
                .is_some_and(|bytes| serde_json::from_slice::<serde_json::Value>(&bytes).is_ok())
        });
        let data: serde_json::Value =
            serde_json::from_slice(&std::fs::read(metadata).unwrap()).unwrap();
        let pid = data["pid"].as_u64().unwrap() as u32;
        let handle = unsafe { OpenProcess(PROCESS_SYNCHRONIZE, 0, pid) };
        assert!(!handle.is_null());
        let handle = unsafe { OwnedHandle::from_raw_handle(handle) };
        println!(
            "lifecycle {mode}: parent PID {}, child PID {pid}",
            parent.id()
        );
        parent.kill().unwrap();
        parent.wait().unwrap();
        assert_eq!(
            unsafe { WaitForSingleObject(handle.as_raw_handle(), 8000) },
            WAIT_OBJECT_0
        );
        wait_until(|| Profile::acquire(&profile).is_ok());
        assert!(profile.exists());
        assert!(unrelated.try_wait().unwrap().is_none());
        let session = BackendSession::start(&executable(), &profile).unwrap();
        assert!(session.health().is_ok());
    }
    unrelated.stdin.take();
    unrelated.wait().unwrap();
}
