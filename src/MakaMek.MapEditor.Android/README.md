# MakaMek.MapEditor.Android

The Android head of MakaMek.MapEditor. The production build produces a signed
`.apk` (GitHub Release / F-Droid) and a signed `.aab` (Google Play), then
deploys the `.aab` to the Play Console `internal` track.

## Required repository secrets

| Secret | Description |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | The Android release keystore (`.jks`/`.keystore`) encoded as base64: `base64 -w0 android-release.keystore` |
| `ANDROID_KEYSTORE_ALIAS` | Alias of the signing key inside the keystore |
| `ANDROID_KEYSTORE_STORE_PASS` | Keystore (store) password |
| `ANDROID_KEYSTORE_KEY_PASS` | Key password. For PKCS12 keystores (the `keytool` default) this must equal the store password |
| `PLAY_STORE_SERVICE_ACCOUNT_JSON` | JSON key of the Play Console service account (see below) |

The four keystore secrets must be configured together. When they are absent
(main-branch pushes, manual runs, forks) the build falls back to auto-signing
with the debug keystore, so plain builds keep working without secrets.

## Play Console prerequisites

1. Create the app entry in the [Play Console](https://play.google.com/console) with package name `nl.sanetby.makamek.mapeditor` (matches the project's `ApplicationId`).
2. Set up Play App Signing, then upload the release keystore (`ANDROID_KEYSTORE_BASE64`) as the **upload key** used to sign builds.
3. Enable the Google Play Developer API for the app's Google Cloud project.
4. Create a service account (Google Cloud → IAM & Admin → Service accounts), add a JSON key, and save the downloaded JSON file contents as the `PLAY_STORE_SERVICE_ACCOUNT_JSON` secret.
5. Grant the service account access to the app in Play Console → Users and permissions (invite the service account email as an "App manager" or with at least the **Create releases** permission for the app).
6. Release notes are read from `distribution/whatsnew/whatsnew-<BCP47-locale>` (e.g. `whatsnew-en-US`).

Deployment runs only for tag pushes and is gated by the `play-store-production`
[GitHub environment](https://docs.github.com/en/actions/deployment/using-environments-for-deployment),
where required reviewers can be enabled. The first upload goes to the `internal`
track; promote releases manually in the Play Console until production is ready.
