use tauri::Url;

// Capabilities restrict IPC; this guard separately blocks leaving the local UI.
pub fn is_allowed(url: &Url, development: bool) -> bool {
    if !url.username().is_empty() || url.password().is_some() {
        return false;
    }
    match (url.scheme(), url.host_str(), url.port_or_known_default()) {
        ("http", Some("tauri.localhost"), Some(80)) => true,
        ("tauri", Some("localhost"), None) => true,
        ("http", Some("127.0.0.1"), Some(5173)) => development,
        _ => false,
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn permits_only_app_origin_in_release() {
        for address in [
            "http://tauri.localhost/",
            "tauri://localhost/index.html#content",
        ] {
            assert!(is_allowed(&Url::parse(address).unwrap(), false));
        }
        let dev = Url::parse("http://127.0.0.1:5173/#content").unwrap();
        assert!(is_allowed(&dev, true));
        assert!(!is_allowed(&dev, false));
    }

    #[test]
    fn rejects_remote_files_credentials_and_lookalike_origins() {
        for address in [
            "https://example.com/",
            "http://tauri.localhost.example.com/",
            "http://tauri.localhost:8080/",
            "http://user:password@tauri.localhost/",
            "http://localhost:5173/",
            "http://127.0.0.1:5174/",
            "https://127.0.0.1:5173/",
            "file:///C:/Windows/win.ini",
            "data:text/html,test",
            "javascript:alert(1)",
            "about:blank",
        ] {
            let url = Url::parse(address).unwrap();
            assert!(!is_allowed(&url, false), "{address}");
            assert!(!is_allowed(&url, true), "{address}");
        }
    }
}
