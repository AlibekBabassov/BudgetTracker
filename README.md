# Budget Tracker

A web app for personal budgeting, written in **C# / Blazor WebAssembly** with a functional-style core.
Record income and expenses, see where the money goes each month, set category limits and get warned before you overspend.
No account, no server: all data stays in your browser.

Team: Babassov Alibek, Zhumagaliev Aktilek  https://alibekbabassov.github.io/BudgetTracker/reports

## Features

Transactions - Add, edit and delete income and expenses (category, amount, date, note). Filter by type, category, month and text. 
Reports - Monthly summary, donut charts by category, a 6-month income vs expenses chart. 
Limits - A monthly limit per expense category with a warning at 80% and an alert when exceeded. 
Look & feel - Light and dark theme, currency symbol, responsive layout, keyboard and screen-reader friendly. 

## Technology

- **.NET 10**, **C# 14**, **Blazor WebAssembly** (hosted as static files, no backend)
- **xUnit** and **bUnit** for tests, **Coverlet** for coverage
- **GitHub Actions** for tests and deployment to **GitHub Pages**
- No UI or chart libraries: CSS and SVG are hand-written

## Run it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). Works the same on macOS, Windows and Linux.

```bash
dotnet run --project src/BudgetTracker.Web
```

Open the address printed in the terminal (by default <http://localhost:5180>).

The project targets .NET 10. To use .NET 8 instead, change `net10.0` to `net8.0` in the three `.csproj` files, `10.0.*` to `8.0.*` in `BudgetTracker.Web.csproj`, and `10.0.x` to `8.0.x` in `.github/workflows/deploy.yml`.

### Tests

bash
dotnet test                                                  
dotnet test --collect:"XPlat Code Coverage"    

### Publish to GitHub Pages

1. Push the repository to GitHub (default branch `main`).
2. In **Settings → Pages**, set **Source** to **GitHub Actions**.
3. Every push to `main` runs the tests, publishes the app and deploys it. The workflow in `.github/workflows/deploy.yml` already fixes the base path for `https://<user>.github.io/<repo>/`.

## Architecture in one picture
- **Domain** has no dependency on Blazor or the browser. Every function is pure: same input, same output, nothing is changed in place. This is where the functional-programming ideas live (see below), and it is why the logic is easy to test.
- **Web** is a thin layer: `AppState` keeps one immutable snapshot of the data and announces changes; components render it.
- More detail, diagrams and design decisions: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

### Functional programming in this project

| Idea | Where |
|---|---|
| Immutable data | `Transaction`, `BudgetLimit`, `AppData` are `record`s. Edits use `with` and create new values. |
| Pure functions | `TransactionValidator`, `ReportCalculator`, `BudgetCheck`, `TransactionFilter` |
| Pipelines | Reports are `Where → GroupBy → Select → OrderBy` over immutable lists |
| Errors as values | `Result<T>` (`Ok` / `Fail`) instead of exceptions for expected problems such as invalid input |
| Pattern matching | `switch` on `Result<T>` cases and `is` patterns |
| Side effects at the edges | Only `AppState`, `LocalStorageService` and `BrowserService` touch the outside world |

### State management

`AppState` is registered as a scoped service (one per browser tab). Components inherit `AppStateComponent`, which subscribes to `AppState.OnChange` and unsubscribes in `Dispose`. User settings (currency) are shared down the tree with a `CascadingValue`. Components such as `TransactionForm` and `DataTable<TItem>` are fully reusable: they receive data through `[Parameter]`s and report back through `EventCallback`s.

### Error handling

- Form input is validated by `TransactionValidator` and shown as a list of messages.
- A damaged save file does not crash the app: it starts empty, keeps a backup copy under `budget-tracker.data.v1.corrupt` and shows a warning.
- If the browser refuses to store data (full or disabled storage), the app keeps working in memory and tells the user that changes will be lost.

## Project structure
BudgetTracker/
├─ src/
│  ├─ BudgetTracker.Domain/        pure logic: Models, Logic, Common (Result)
│  └─ BudgetTracker.Web/           Blazor WASM: Pages, Components, Layout, Services, wwwroot
├─ tests/BudgetTracker.Tests/      xUnit (Domain, AppState) and bUnit (components)
├─ docs/                           architecture, user guide, QA plan
└─ .github/workflows/deploy.yml    test + deploy to GitHub Pages

## Documentation

- [Architecture and diagrams](docs/ARCHITECTURE.md)
- [User guide](docs/USER_GUIDE.md)
- [Manual QA plan and device matrix](docs/QA.md)
- [Defense cheat sheet (Russian)](docs/DEFENSE_RU.md)

## Known limitations

- Data lives in one browser on one device (there is no sync between devices).
- One currency at a time (the symbol is only a label, there is no conversion).
