# Manual QA plan

Automated tests cover the logic and components (`dotnet test`). This plan covers what only a human on a real device can check.
Fill the tables in before the defense: the results are evidence for the *Quality Assurance* and *Cross-Platform* criteria.

Legend: ✅ passed · ❌ failed (write the bug below) · — not tested

## 1. Functional checklist

| # | Scenario | Expected | Result |
|---|---|---|---|
| F1 | Add an expense (Food, 12.50, today) | Appears in history, Dashboard totals update | |
| F2 | Add an income | Shown in green with a + sign | |
| F3 | Enter amount `12,5` | Accepted as 12.50 | |
| F4 | Enter amount `abc`, `0`, `-5`, `10.005` | Clear error message, nothing saved | |
| F5 | Leave date empty or type a far-future date | Error message, nothing saved | |
| F6 | Edit a transaction | Form fills in, saving replaces the entry, Cancel works | |
| F7 | Delete a transaction | Confirmation dialog first; Cancel keeps it | |
| F8 | Filters: type, category, text, "month only" | List and counter update immediately | |
| F9 | Change month with ‹ › on Dashboard, Reports, Limits | All widgets follow the same month | |
| F10 | Set a limit, then spend 80% of it | Orange warning on Dashboard | |
| F11 | Spend past the limit | Red alert, bar capped at 100% | |
| F12 | Set the same category limit twice | Only the newest limit remains | |
| F13 | Reload the page | All data is still there | |
| F14 | Switch theme and currency, reload | Settings are kept, no light flash in dark mode | |
| F15 | Delete all data | Confirmation first; settings are kept | |
| F16 | Damage the data: in DevTools set `budget-tracker.data.v1` to `{oops` and reload | App starts, warning shown, backup key exists | |
| F17 | Private/incognito window with storage blocked (or Safari "Block all cookies") | App works, red storage message appears | |

## 2. Browser and device matrix

Run F1, F6, F10 and F13 on each line.

| Device | OS | Browser | Version | Result | Notes |
|---|---|---|---|---|---|
| MacBook | macOS | Safari | | | |
| MacBook | macOS | Chrome | | | |
| MacBook | macOS | Firefox | | | |
| PC / laptop | Windows | Edge | | | |
| PC / laptop | Windows | Chrome | | | |
| Linux machine or VM | Linux | Firefox | | | |
| Phone | iOS | Safari | | | |
| Phone | Android | Chrome | | | |
| Tablet | iPadOS / Android | Safari / Chrome | | | |

## 3. Responsive checks (DevTools device toolbar)

| Width | What to check | Result |
|---|---|---|
| 360 px | Bottom navigation fits 5 items; tables are card-style; forms are one column; no horizontal scroll | |
| 768 px | Bottom navigation; two-column filters; charts readable | |
| 1280 px | Sidebar navigation; two-column dashboard | |
| Landscape phone | Content not hidden behind bottom bar | |
| Zoom 200% | Layout still usable | |

## 4. Accessibility spot checks

| Check | Result |
|---|---|
| Whole flow (add, edit, delete, filter) with keyboard only | |
| Visible focus ring on every control | |
| Screen reader (VoiceOver: Cmd+F5) reads form labels, alerts and chart descriptions | |
| Lighthouse (Chrome DevTools) accessibility score | |
| Lighthouse performance score | |

## 5. Bugs found and fixed

| # | Found in | Description | Fix | Status |
|---|---|---|---|---|
| | | | | |
