#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

fn main() {
    // No shell/fs plugins, backend launch or renderer commands in STORY-024.
    tauri::Builder::default()
        .run(tauri::generate_context!())
        .expect("DESKTOP_START_FAILED");
}
