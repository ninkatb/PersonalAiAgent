fn main() {
    tauri_build::try_build(
        tauri_build::Attributes::new()
            .app_manifest(tauri_build::AppManifest::new().commands(&["backend_health"])),
    )
    .expect("DESKTOP_BUILD_FAILED");
}
