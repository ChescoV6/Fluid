# Code Signing Guide

Unsigned executables trigger Windows SmartScreen warnings ("Windows protected your PC") for every new user. Signing removes that friction. This guide covers the two realistic options for a community project.

---

## Option A: Self-signed certificate (free, but must be installed manually)

Good for testing and for users who trust you directly. Self-signed certs do **not** remove SmartScreen — users must install your certificate first.

### 1. Create the certificate (run once, keep the .pfx safe)

```powershell
$cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=Fluid" -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(3)
$pwd = ConvertTo-SecureString -String "YourPasswordHere" -Force -AsPlainText
Export-PfxCertificate -Cert "Cert:\CurrentUser\My\$($cert.Thumbprint)" -FilePath "FluidSigning.pfx" -Password $pwd
```

### 2. Export the public part for users

```powershell
Export-Certificate -Cert "Cert:\CurrentUser\My\$($cert.Thumbprint)" -FilePath "FluidSigning.cer"
```

Users run `FluidSigning.cer` → *Install Certificate* → **Local Machine** → **Trusted People** (once). After that your signed builds open without warnings.

### 3. Sign a build

Requires the Windows SDK (`signtool.exe`):

```powershell
signtool sign /f FluidSigning.pfx /p "YourPasswordHere" /tr http://timestamp.digicert.com /td sha256 /fd sha256 Fluid.exe
```

The timestamp server (`/tr`) is important — the signature stays valid even after the certificate expires.

---

## Option B: Trusted CA certificate (removes SmartScreen reputation over time)

For a proper public release you want a certificate from a CA trusted by Windows:

| Type | Approx. cost | Notes |
|------|--------------|-------|
| OV (Organization Validation) | ~$100–300/yr | Standard choice; SmartScreen reputation builds with downloads |
| EV (Extended Validation) | ~$250–500/yr | Immediate SmartScreen reputation; requires hardware token (or cloud HSM) |

Common providers: Certum (cheap open-source developer tier), SSL.com, Sectigo, DigiCert.

Sign the same way as Option A step 3, pointing `/f` at the CA-provided certificate.

---

## CI signing (this repository)

`.github/workflows/release.yml` already contains an optional signing step. To enable it:

1. Base64-encode your `.pfx`:
   ```powershell
   [Convert]::ToBase64String([IO.File]::ReadAllBytes("FluidSigning.pfx")) | Set-Content cert.b64
   ```
2. In your GitHub repo: **Settings → Secrets and variables → Actions**, add:
   - `SIGNING_CERT` = contents of `cert.b64`
   - `SIGNING_CERT_PASSWORD` = the certificate password
3. Push a tag (`v1.0.0`) — the release workflow signs the published `Fluid.exe` automatically.

> Note: the CI step calls `signtool.exe`, which is preinstalled on GitHub's Windows runners as part of the Windows SDK. If the step can't find it, add a step to locate it under `C:\Program Files (x86)\Windows Kits\10\bin\`.
