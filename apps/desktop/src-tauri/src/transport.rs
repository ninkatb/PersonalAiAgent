use crate::owned_process::OwnedProcess;
use crate::profile::Profile;
use serde::{Deserialize, Serialize};
use std::fs::File;
use std::io::{Read, Write};
use std::path::Path;
use std::sync::{
    Arc,
    atomic::{AtomicBool, Ordering},
    mpsc,
};
use std::time::Duration;

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct Ready {
    protocol: u8,
    pid: u32,
    port: u16,
}

#[derive(Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct HealthResult {
    status: String,
}

// Credentials and process handles never cross the renderer boundary or Debug.
pub struct BackendSession {
    child: OwnedProcess,
    input: Option<File>,
    _profile: Arc<Profile>,
    pub revocation: Arc<Revocation>,
    token: String,
    origin: String,
    port: u16,
    client: reqwest::blocking::Client,
}

pub struct Revocation {
    active: AtomicBool,
    job: Arc<crate::owned_process::Job>,
}

impl Revocation {
    pub fn revoke(&self) {
        self.active.store(false, Ordering::SeqCst);
        self.job.terminate();
    }
    fn is_active(&self) -> bool {
        self.active.load(Ordering::SeqCst)
    }
}

fn random_hex(size: usize) -> Result<String, &'static str> {
    let mut bytes = vec![0u8; size];
    getrandom::fill(&mut bytes).map_err(|_| "SESSION_RANDOM_FAILED")?;
    Ok(bytes.iter().map(|byte| format!("{byte:02x}")).collect())
}

pub fn read_frame(mut reader: impl Read) -> Result<Vec<u8>, &'static str> {
    let mut frame = Vec::new();
    let mut byte = [0u8];
    for _ in 0..1024 {
        reader
            .read_exact(&mut byte)
            .map_err(|_| "STARTUP_INCOMPLETE")?;
        if byte[0] == b'\n' {
            return Ok(frame);
        }
        frame.push(byte[0]);
    }
    Err("STARTUP_TOO_LARGE")
}

fn validate_ready(frame: &[u8], pid: u32) -> Result<u16, &'static str> {
    let ready: Ready = serde_json::from_slice(frame).map_err(|_| "STARTUP_INVALID")?;
    if ready.protocol != 1 || ready.pid != pid || ready.port == 0 {
        return Err("STARTUP_INVALID");
    }
    Ok(ready.port)
}

impl BackendSession {
    #[cfg(test)]
    pub fn start(executable: &Path, directory: &Path) -> Result<Self, &'static str> {
        Self::start_with_profile(executable, Arc::new(Profile::acquire(directory)?))
    }

    pub fn start_with_profile(
        executable: &Path,
        profile: Arc<Profile>,
    ) -> Result<Self, &'static str> {
        // Reserved for STORY-028: no bootstrap file is read/written in this story.
        let _bootstrap_path = &profile.bootstrap;
        let token = random_hex(32)?;
        let session_id = random_hex(16)?;
        let origin = format!("http://session-{session_id}.localhost");
        let client = reqwest::blocking::Client::builder()
            .no_proxy()
            .redirect(reqwest::redirect::Policy::none())
            .timeout(Duration::from_secs(3))
            .build()
            .map_err(|_| "BACKEND_UNAVAILABLE")?;
        let child = OwnedProcess::spawn(executable, &profile.lock)?;
        let revocation = Arc::new(Revocation {
            active: AtomicBool::new(true),
            job: child.job.clone(),
        });
        // Own the process immediately so every failure path runs Drop/cleanup.
        let mut session = Self {
            child,
            input: None,
            _profile: profile,
            revocation,
            token,
            origin,
            port: 0,
            client,
        };
        session.input = session.child.input.take();
        let message = serde_json::json!({ "protocol": 1, "token": session.token,
            "sessionId": session_id, "origin": session.origin });
        let input = session.input.as_mut().ok_or("STARTUP_INCOMPLETE")?;
        serde_json::to_writer(&mut *input, &message).map_err(|_| "STARTUP_FAILED")?;
        input.write_all(b"\n").map_err(|_| "STARTUP_FAILED")?;
        input.flush().map_err(|_| "STARTUP_FAILED")?;
        let output = session.child.output.take().ok_or("STARTUP_INCOMPLETE")?;
        let (sender, receiver) = mpsc::sync_channel(1);
        std::thread::spawn(move || {
            let _ = sender.send(read_frame(output));
        });
        let frame = receiver
            .recv_timeout(Duration::from_secs(12))
            .map_err(|_| "STARTUP_TIMEOUT")??;
        session.port = validate_ready(&frame, session.child.id())?;
        session.health()?; // Validate possession of the session before exposing readiness.
        Ok(session)
    }

    pub fn health(&self) -> Result<HealthResult, &'static str> {
        if !self.revocation.is_active() || self.child.exited() {
            return Err("BACKEND_UNAVAILABLE");
        }
        let response = self
            .client
            .get(format!("http://127.0.0.1:{}/health", self.port))
            .header("Origin", &self.origin)
            .bearer_auth(&self.token)
            .send()
            .map_err(|_| "BACKEND_UNAVAILABLE")?;
        if response.status() != reqwest::StatusCode::OK {
            return Err("BACKEND_UNAVAILABLE");
        }
        let mut body = Vec::new();
        response
            .take(1025)
            .read_to_end(&mut body)
            .map_err(|_| "BACKEND_UNAVAILABLE")?;
        if body.len() > 1024 {
            return Err("BACKEND_UNAVAILABLE");
        }
        let health: HealthResult =
            serde_json::from_slice(&body).map_err(|_| "BACKEND_UNAVAILABLE")?;
        if health.status != "ok" {
            return Err("BACKEND_UNAVAILABLE");
        }
        // A result from a revoked session must never cross the IPC boundary.
        if !self.revocation.is_active() {
            return Err("BACKEND_UNAVAILABLE");
        }
        Ok(health)
    }

    pub fn monitor_exit(
        &self,
        on_exit: impl FnOnce() + Send + 'static,
    ) -> Result<(), &'static str> {
        let revocation = self.revocation.clone();
        self.child.monitor_exit(move || {
            revocation.revoke();
            on_exit();
        })
    }

    pub fn shutdown(&mut self) {
        self.revocation.revoke();
        self.input.take();
        // Bound cleanup time: close the private parent channel, then reap the child.
        self.child.shutdown();
    }
}

impl Drop for BackendSession {
    fn drop(&mut self) {
        self.shutdown();
    }
}

#[cfg(test)]
#[path = "lifecycle_tests.rs"]
mod lifecycle_tests;

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn readiness_rejects_forged_pid_version_port_and_extra_fields() {
        for frame in [
            r#"{"protocol":2,"pid":42,"port":1234}"#,
            r#"{"protocol":1,"pid":43,"port":1234}"#,
            r#"{"protocol":1,"pid":42,"port":0}"#,
            r#"{"protocol":1,"pid":42,"port":65536}"#,
            r#"{"protocol":1,"pid":42,"port":1234,"token":"SYNTHETIC"}"#,
        ] {
            assert!(validate_ready(frame.as_bytes(), 42).is_err());
        }
        assert_eq!(
            validate_ready(br#"{"protocol":1,"pid":42,"port":1234}"#, 42),
            Ok(1234)
        );
    }

    #[test]
    fn startup_is_bounded_and_requires_a_complete_frame() {
        assert!(read_frame(&b"{}"[..]).is_err());
        assert!(read_frame(&[b'a'; 1024][..]).is_err());
        assert_eq!(read_frame(&b"{}\n"[..]), Ok(b"{}".to_vec()));
    }

    #[test]
    fn generates_fresh_crypto_random_session_credentials() {
        let first = random_hex(32).unwrap();
        assert_eq!(first.len(), 64);
        assert!(first.bytes().all(|byte| byte.is_ascii_hexdigit()));
        assert_ne!(first, random_hex(32).unwrap());
    }

    #[test]
    fn missing_sidecar_fails_without_an_alternate_launcher() {
        let profile = test_profile();
        assert!(
            BackendSession::start(Path::new("SYNTHETIC_MISSING_SIDECAR.exe"), &profile).is_err()
        );
        assert!(Profile::acquire(&profile).is_ok());
    }

    #[test]
    fn real_sidecar_health_and_cleanup() {
        let folder = std::env::current_exe()
            .unwrap()
            .parent()
            .unwrap()
            .parent()
            .unwrap()
            .to_owned();
        let path = folder.join("sidecar").join(if cfg!(windows) {
            "PersonalAI.Api.exe"
        } else {
            "PersonalAI.Api"
        });
        let mut session = BackendSession::start(&path, &test_profile())
            .expect("Publish sidecar before running desktop tests");
        assert_eq!(session.health().unwrap().status, "ok");
        let port = session.port;
        session.shutdown();
        assert!(session.child.exited());
        assert!(std::net::TcpStream::connect((std::net::Ipv4Addr::LOCALHOST, port)).is_err());
    }

    pub(super) fn test_profile() -> std::path::PathBuf {
        std::env::temp_dir().join(format!("personal-ai-lifecycle-{}", random_hex(16).unwrap()))
    }
}
