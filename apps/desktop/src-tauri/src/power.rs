use crate::transport::Revocation;
use std::sync::Arc;
use tauri::{Emitter, Manager};
use windows_sys::Win32::Foundation::{HWND, LPARAM, LRESULT, WPARAM};
use windows_sys::Win32::UI::Shell::{DefSubclassProc, RemoveWindowSubclass, SetWindowSubclass};
use windows_sys::Win32::UI::WindowsAndMessaging::{WM_NCDESTROY, WM_POWERBROADCAST};

fn invalidates_session(message: u32, event: usize) -> bool {
    // Suspend, critical resume, resume from suspend, and automatic resume.
    message == WM_POWERBROADCAST && matches!(event, 4 | 6 | 7 | 18)
}

struct PowerGuard {
    session: Arc<Revocation>,
    app: tauri::AppHandle,
}

unsafe extern "system" fn on_message(
    hwnd: HWND,
    message: u32,
    wparam: WPARAM,
    lparam: LPARAM,
    id: usize,
    data: usize,
) -> LRESULT {
    // SAFETY: the Box is owned by this subclass until WM_NCDESTROY.
    let guard = unsafe { &*(data as *const PowerGuard) };
    if invalidates_session(message, wparam) {
        guard.session.revoke();
        let _ = guard.app.emit("backend-unavailable", ());
    }
    if message == WM_NCDESTROY {
        unsafe {
            RemoveWindowSubclass(hwnd, Some(on_message), id);
        }
        unsafe {
            drop(Box::from_raw(data as *mut PowerGuard));
        }
    }
    unsafe { DefSubclassProc(hwnd, message, wparam, lparam) }
}

pub fn install(
    window: &tauri::WebviewWindow,
    session: Arc<Revocation>,
) -> Result<(), &'static str> {
    let hwnd = window.hwnd().map_err(|_| "POWER_GUARD_FAILED")?.0;
    let data = Box::into_raw(Box::new(PowerGuard {
        session,
        app: window.app_handle().clone(),
    }));
    // Must be installed on the owning UI thread (Tauri setup).
    if unsafe { SetWindowSubclass(hwnd as HWND, Some(on_message), 26, data as usize) } == 0 {
        unsafe {
            drop(Box::from_raw(data));
        }
        return Err("POWER_GUARD_FAILED");
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    #[test]
    fn only_suspend_and_resume_invalidate_the_session() {
        for event in [4, 6, 7, 18] {
            assert!(super::invalidates_session(0x218, event));
        }
        for event in [0, 10, 0x8013] {
            assert!(!super::invalidates_session(0x218, event));
        }
        assert!(!super::invalidates_session(0x123, 4));
    }
}
