"""Install pinned Linux x64 SDKs outside the checkout, with vendor checksums."""
import hashlib
import json
from pathlib import Path
import subprocess
import tarfile
import tomllib
import urllib.request

ROOT = Path('/workspace/.personal-ai-agent-setup')
REPO = Path(__file__).resolve().parents[1]


def metadata(url):
    with urllib.request.urlopen(url) as response:
        return response.read()


def download(url, destination, expected, algorithm):
    if not destination.exists():
        urllib.request.urlretrieve(url, destination)
    with destination.open('rb') as stream:
        checksum = hashlib.file_digest(stream, algorithm).hexdigest()
    if checksum.lower() != expected.lower():
        raise RuntimeError(f'Checksum mismatch: {destination.name}; artifact was not used')


def install_dotnet():
    version = json.loads((REPO / 'global.json').read_text())['sdk']['version']
    executable = ROOT / 'dotnet/dotnet'
    if executable.exists() and subprocess.check_output([str(executable), '--version'], text=True).strip() == version:
        return
    data = json.loads(metadata('https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json'))
    sdks = [sdk for release in data['releases'] for sdk in release.get('sdks', [release['sdk']])]
    sdk = next(item for item in sdks if item['version'] == version)
    artifact = next(item for item in sdk['files'] if item['rid'] == 'linux-x64' and item['name'].endswith('.tar.gz'))
    archive = ROOT / f'dotnet-sdk-{version}.tar.gz'
    download(artifact['url'], archive, artifact['hash'], 'sha512')
    executable.parent.mkdir(exist_ok=True)
    with tarfile.open(archive) as source:
        source.extractall(executable.parent, filter='data')


def install_rust():
    version = tomllib.loads((REPO / 'rust-toolchain.toml').read_text())['toolchain']['channel']
    executable = ROOT / 'rust/bin/rustc'
    if executable.exists() and subprocess.check_output([str(executable), '--version'], text=True).split()[1] == version:
        return
    data = tomllib.loads(metadata(f'https://static.rust-lang.org/dist/channel-rust-{version}.toml').decode())
    artifact = data['pkg']['rust']['target']['x86_64-unknown-linux-gnu']
    archive = ROOT / f'rust-{version}.tar.xz'
    download(artifact['xz_url'], archive, artifact['xz_hash'], 'sha256')
    destination = ROOT / 'rust-installer'
    destination.mkdir(exist_ok=True)
    with tarfile.open(archive) as source:
        source.extractall(destination, filter='data')
    installer = destination / f'rust-{version}-x86_64-unknown-linux-gnu/install.sh'
    subprocess.run(['bash', str(installer), '--prefix=' + str(ROOT / 'rust'),
                    '--components=rustc,cargo,rust-std-x86_64-unknown-linux-gnu,rustfmt-preview,clippy-preview',
                    '--disable-ldconfig'], check=True)


if __name__ == '__main__':
    ROOT.mkdir(exist_ok=True)
    install_dotnet()
    install_rust()
    print('Pinned SDKs available; no repository dependencies changed.')
