# Store assets

This folder collects the Google Play listing assets and listing text. 

## Graphics

Generate and drop the results in this folder.

| Asset | Required size | File |
|---|---|---|
| App icon | 512x512 px | `icon.png` |
| Feature graphic | 1024x500 px | `feature-graphic.png` |
| Phone screenshots | 1080x1920 px, 2-8 images, PNG/JPEG | `phone-screenshots/` |
| Tablet screenshots | 2000x1200 px, optional | `tablet-screenshots/` |

Use the app logo (`src/MakaMek.MapEditor/Assets/logo.png`) and real in-app
screenshots as the source material. Screenshots should not be more than
stretched/resized; keep at least 4 phone screenshots for the "Promotional
content" and "Devices" phone tabs.

## Listing text

| File | Purpose |
|---|---|
| `short-description.txt` | Short description (80 characters max) |
| `full-description.txt` | Full description |
| `whatsnew-<locale>.txt` | Release notes per locale, mirrored to `distribution/whatsnew/` for the CI deploy |
