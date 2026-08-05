# Data safety declaration (Google Play)

Canonical record of the app's data handling, mirrored from `PRIVACY.md`.
Use this when completing the Play Console **Data safety** questionnaire.

## Data collected

| Category | Type | Collected? | Purpose | Processing |
|---|---|---|---|---|
| Network connections | IP address (transient) | Yes, **only** during the one-time terrain-data download | App functionality — downloading terrain/biome data on first launch from `api.github.com` | IP is seen by GitHub's servers only; not collected, logged, or stored by the developer |
| Everything else | Personal, device, identifiers, financial, health, messages, photos, audio, files, location, app activity, purchases | No | — | — |

Notes:

- The only network call is the first-launch download of terrain/biome data from
  `https://api.github.com/repos/anton-makarevich/MakaMek/contents/data/hexes/biomes`.
  It is an HTTPS request to a third party (GitHub); no analytics, advertising,
  or tracking SDKs are present.
- Android `INTERNET` permission exists solely for this HTTPS download;
  `usesCleartextTraffic=false` is set, so cleartext traffic is not permitted.
- After the one-time download the app works fully offline and makes no network
  calls.

## Data sharing / sale / deletion

- **Shared**: No data is shared with third parties (other than the IP address
  necessarily visible to GitHub when the download is made).
- **Sold**: No data is sold.
- **Deletion**: No account exists; the developer collects and stores no user
  data, so there is nothing to delete. All app data (saved maps chosen via the
  OS file picker, cached terrain data) resides on the user's device.

## Play Console questionnaire answers

- **Does your app collect or share any of the required user data types?** No.
- **Data collected:** Network connections — only the IP address transmitted as
  part of the HTTPS terrain-data download (app functionality). All other data
  types: not collected.
- **Data shared:** No.
- **Data encrypted in transit:** Yes (HTTPS).
- **Data deletion mechanism required?** No — no account, no stored data.
