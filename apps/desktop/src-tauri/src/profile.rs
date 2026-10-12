use std::fs::{File, OpenOptions};
use std::path::{Path, PathBuf};

pub struct Profile {
    pub lock: File,
    pub bootstrap: PathBuf,
}

impl Profile {
    pub fn acquire(directory: &Path) -> Result<Self, &'static str> {
        std::fs::create_dir_all(directory).map_err(|_| "PROFILE_UNAVAILABLE")?;
        let mut options = OpenOptions::new();
        options.read(true).write(true).create(true).truncate(false);
        #[cfg(windows)]
        {
            use std::os::windows::fs::OpenOptionsExt;
            // The open handle is the lock; never delete a stale marker or trust a PID.
            options.share_mode(0);
        }
        let lock = options
            .open(directory.join("instance.lock"))
            .map_err(|_| "PROFILE_UNAVAILABLE")?;
        #[cfg(not(windows))]
        lock.try_lock().map_err(|_| "PROFILE_UNAVAILABLE")?;
        Ok(Self {
            lock,
            bootstrap: directory.join("bootstrap.json"),
        })
    }
}
