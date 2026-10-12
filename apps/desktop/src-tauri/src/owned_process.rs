//! Windows creates the child atomically inside a private kill-on-close job.
//! Only the two pipe ends, null stderr, and profile lease are inherited.
use std::fs::File;
use std::os::windows::ffi::OsStrExt;
use std::os::windows::io::{AsRawHandle, FromRawHandle, OwnedHandle};
use std::path::Path;
use std::sync::Arc;
use windows_sys::Win32::Foundation::*;
use windows_sys::Win32::Security::SECURITY_ATTRIBUTES;
use windows_sys::Win32::System::JobObjects::*;
use windows_sys::Win32::System::Pipes::CreatePipe;
use windows_sys::Win32::System::Threading::*;

// Inheritable duplicates exist only while creating a child. Serialize our
// process creation so a concurrent launcher cannot inherit another profile lease.
pub static SPAWN_GATE: std::sync::Mutex<()> = std::sync::Mutex::new(());

pub struct Job(OwnedHandle);

impl Job {
    fn create() -> Result<Self, &'static str> {
        // SAFETY: null security/name creates an unnamed, non-inheritable private job.
        let handle = unsafe { CreateJobObjectW(std::ptr::null(), std::ptr::null()) };
        let job = Self(own(handle)?);
        let mut limits: JOBOBJECT_EXTENDED_LIMIT_INFORMATION = unsafe { std::mem::zeroed() };
        limits.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
        let ok = unsafe {
            SetInformationJobObject(
                job.handle(),
                JobObjectExtendedLimitInformation,
                (&limits as *const JOBOBJECT_EXTENDED_LIMIT_INFORMATION).cast(),
                std::mem::size_of_val(&limits) as u32,
            )
        };
        if ok == 0 {
            return Err("PROCESS_OWNERSHIP_FAILED");
        }
        Ok(job)
    }

    fn handle(&self) -> HANDLE {
        self.0.as_raw_handle()
    }

    pub fn terminate(&self) {
        // Only this job's descendants can be affected. No process names/PID lookup.
        unsafe {
            TerminateJobObject(self.handle(), 1);
        }
    }
}

pub struct OwnedProcess {
    process: OwnedHandle,
    pub input: Option<File>,
    pub output: Option<File>,
    pub job: Arc<Job>,
    pid: u32,
}

fn own(handle: HANDLE) -> Result<OwnedHandle, &'static str> {
    if handle.is_null() || handle == INVALID_HANDLE_VALUE {
        return Err("PROCESS_OWNERSHIP_FAILED");
    }
    // SAFETY: the successful Win32 call transferred a newly owned handle.
    Ok(unsafe { OwnedHandle::from_raw_handle(handle) })
}

fn pipe(parent_reads: bool) -> Result<(File, File), &'static str> {
    let mut read = std::ptr::null_mut();
    let mut write = std::ptr::null_mut();
    let security = SECURITY_ATTRIBUTES {
        nLength: std::mem::size_of::<SECURITY_ATTRIBUTES>() as u32,
        lpSecurityDescriptor: std::ptr::null_mut(),
        bInheritHandle: 1,
    };
    if unsafe { CreatePipe(&mut read, &mut write, &security, 0) } == 0 {
        return Err("PROCESS_OWNERSHIP_FAILED");
    }
    let read = own(read)?;
    let write = own(write)?;
    let parent = if parent_reads {
        read.as_raw_handle()
    } else {
        write.as_raw_handle()
    };
    if unsafe { SetHandleInformation(parent, HANDLE_FLAG_INHERIT, 0) } == 0 {
        return Err("PROCESS_OWNERSHIP_FAILED");
    }
    Ok((File::from(read), File::from(write)))
}

struct Attributes(Vec<usize>);
impl Attributes {
    fn new() -> Result<Self, &'static str> {
        let mut size = 0;
        unsafe {
            InitializeProcThreadAttributeList(std::ptr::null_mut(), 2, 0, &mut size);
        }
        let mut value = Self(vec![0; size.div_ceil(std::mem::size_of::<usize>())]);
        if unsafe { InitializeProcThreadAttributeList(value.pointer(), 2, 0, &mut size) } == 0 {
            // An uninitialized list must not go through DeleteProcThreadAttributeList.
            value.0.clear();
            return Err("PROCESS_OWNERSHIP_FAILED");
        }
        Ok(value)
    }
    fn pointer(&mut self) -> LPPROC_THREAD_ATTRIBUTE_LIST {
        self.0.as_mut_ptr().cast()
    }
    fn set(&mut self, kind: u32, handles: &mut [HANDLE]) -> Result<(), &'static str> {
        if unsafe {
            UpdateProcThreadAttribute(
                self.pointer(),
                0,
                kind as usize,
                handles.as_mut_ptr().cast(),
                std::mem::size_of_val(handles),
                std::ptr::null_mut(),
                std::ptr::null(),
            )
        } == 0
        {
            return Err("PROCESS_OWNERSHIP_FAILED");
        }
        Ok(())
    }
}
impl Drop for Attributes {
    fn drop(&mut self) {
        if !self.0.is_empty() {
            unsafe {
                DeleteProcThreadAttributeList(self.pointer());
            }
        }
    }
}

impl OwnedProcess {
    pub fn spawn(executable: &Path, profile: &File) -> Result<Self, &'static str> {
        let _spawn_guard = SPAWN_GATE.lock().map_err(|_| "PROCESS_OWNERSHIP_FAILED")?;
        use std::os::windows::fs::OpenOptionsExt;
        let job = Arc::new(Job::create()?);
        let (input, parent_input) = pipe(false)?;
        let (parent_output, output) = pipe(true)?;
        let null = std::fs::OpenOptions::new()
            .write(true)
            .attributes(0)
            .open("NUL")
            .map_err(|_| "PROCESS_OWNERSHIP_FAILED")?;
        let duplicate = |file: &File| -> Result<OwnedHandle, &'static str> {
            let mut handle = std::ptr::null_mut();
            let current = unsafe { GetCurrentProcess() };
            if unsafe {
                DuplicateHandle(
                    current,
                    file.as_raw_handle(),
                    current,
                    &mut handle,
                    0,
                    1,
                    DUPLICATE_SAME_ACCESS,
                )
            } == 0
            {
                return Err("PROCESS_OWNERSHIP_FAILED");
            }
            own(handle)
        };
        // Keeping the profile lease open in the child prevents a new pair until it exits.
        let profile_lease = duplicate(profile)?;
        let error = duplicate(&null)?;
        let mut handles = [
            input.as_raw_handle(),
            output.as_raw_handle(),
            error.as_raw_handle(),
            profile_lease.as_raw_handle(),
        ];
        let mut jobs = [job.handle()];
        let mut attributes = Attributes::new()?;
        attributes.set(PROC_THREAD_ATTRIBUTE_HANDLE_LIST, &mut handles)?;
        attributes.set(PROC_THREAD_ATTRIBUTE_JOB_LIST, &mut jobs)?;
        let mut startup: STARTUPINFOEXW = unsafe { std::mem::zeroed() };
        startup.StartupInfo.cb = std::mem::size_of::<STARTUPINFOEXW>() as u32;
        startup.StartupInfo.dwFlags = STARTF_USESTDHANDLES;
        startup.StartupInfo.hStdInput = handles[0];
        startup.StartupInfo.hStdOutput = handles[1];
        startup.StartupInfo.hStdError = handles[2];
        startup.lpAttributeList = attributes.pointer();
        let path: Vec<u16> = executable
            .as_os_str()
            .encode_wide()
            .chain(Some(0))
            .collect();
        let mut info: PROCESS_INFORMATION = unsafe { std::mem::zeroed() };
        // Explicit application path, no argv/env credential, no shell or fallback.
        if unsafe {
            CreateProcessW(
                path.as_ptr(),
                std::ptr::null_mut(),
                std::ptr::null(),
                std::ptr::null(),
                1,
                CREATE_NO_WINDOW | EXTENDED_STARTUPINFO_PRESENT,
                std::ptr::null(),
                std::ptr::null(),
                &startup.StartupInfo,
                &mut info,
            )
        } == 0
        {
            return Err("PROCESS_OWNERSHIP_FAILED");
        }
        let process = own(info.hProcess)?;
        let _thread = own(info.hThread)?;
        Ok(Self {
            process,
            input: Some(parent_input),
            output: Some(parent_output),
            job,
            pid: info.dwProcessId,
        })
    }
    pub fn id(&self) -> u32 {
        self.pid
    }
    pub fn exited(&self) -> bool {
        unsafe { WaitForSingleObject(self.process.as_raw_handle(), 0) == WAIT_OBJECT_0 }
    }
    pub fn monitor_exit(
        &self,
        on_exit: impl FnOnce() + Send + 'static,
    ) -> Result<(), &'static str> {
        let handle = self
            .process
            .try_clone()
            .map_err(|_| "PROCESS_OWNERSHIP_FAILED")?;
        std::thread::spawn(move || {
            unsafe {
                WaitForSingleObject(handle.as_raw_handle(), INFINITE);
            }
            on_exit();
        });
        Ok(())
    }
    pub fn shutdown(&mut self) {
        self.input.take();
        self.job.terminate();
        unsafe {
            WaitForSingleObject(self.process.as_raw_handle(), 5000);
        }
    }
}
impl Drop for OwnedProcess {
    fn drop(&mut self) {
        self.shutdown();
    }
}
