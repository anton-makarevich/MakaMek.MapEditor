# Data safety declaration (Google Play)

Canonical record of the app's data handling, mirrored from `PRIVACY.md`.
Use this when completing the Play Console **Data safety** questionnaire.

## Data collected

| Category | Type | Collected? | Purpose | Processing |
|---|---|---|---|---|
| Network connections | IP address (transient) | Yes, **only** during the one-time terrain-data download | App functionality — downloading terrain/biome data on first launch from `api.github.com` | Ephemeral — IP is seen by GitHub's servers only; not retained, logged, or stored by the app or the developer |
| Everything else | Personal, device, identifiers, financial, health, messages, photos, audio, files, location, app activity, purchases | No | — | — |

Notes:

- The only network call is the first-launch download of terrain/biome data from
  `https://api.github.com/repos/anton-makarevich/MakaMek/contents/data/hexes/biomes`.
  It is an HTTPS request to a third party (GitHub); no analytics, advertising,
  or tracking SDKs are present.
- The IP transmission is ephemeral: it is inherent to the HTTPS request and is
  not retained, logged, or stored by the app or the developer.
- Android `INTERNET` permission exists solely for this HTTPS download;
  `usesCleartextTraffic=false` is set, so cleartext traffic is not permitted.
- After the one-time download the app works fully offline and makes no network
  calls.

## Data sharing / sale / deletion

- **Shared**: No data is shared with third parties for their own purposes. The
  only data transmitted off-device is the IP address necessarily visible to
  GitHub's servers during the one-time download; that processing is ephemeral
  and GitHub receives no other data.
- **Sold**: No data is sold.
- **Deletion**: No account exists. The Developer holds no user data, so there
  is nothing for the user to delete from the Developer. All app data (saved
  maps chosen via the OS file picker, cached terrain data) resides on the
  user's device and can be deleted by the user directly.

## Play Console questionnaire answers

- **Does your app collect or share any of the required user data types?** Yes —
  only the IP address transmitted as part of the HTTPS terrain-data download
  (Network connections). The processing is ephemeral and the data is not
  retained. No other data types are collected or shared.
- **Data collected:** Network connections — the IP address transmitted as part
  of the HTTPS terrain-data download (app functionality; ephemeral, not
  retained). All other data types: not collected.
- **Data shared:** No data is shared with third parties beyond the IP address
  necessarily visible to GitHub during the HTTPS download (ephemeral, as
  disclosed above); nothing is shared for advertising, analytics, or any other
  purpose.
- **Data encrypted in transit:** Yes (HTTPS).
- **Data deletion mechanism required?** No — no account and no Developer-held
  data; all app data resides on the user's device and can be deleted by the
  user.
