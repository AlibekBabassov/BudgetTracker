# Architecture

Budget Tracker has three layers. Dependencies point one way only: **UI → Services → Domain**.
Domain knows nothing about Blazor or the browser.

```mermaid
flowchart TB
    subgraph UI["UI layer (Razor)"]
        Pages["Pages: Dashboard, Transactions, Reports, Limits, Settings"]
        Comp["Components: TransactionForm, DataTable&lt;T&gt;, CategoryChart, TrendChart, LimitBar, MoneyText, AlertBanner, ConfirmDialog"]
    end
    subgraph SVC["Services layer"]
        State["AppState (state container, OnChange)"]
        Storage["IStorage ← LocalStorageService"]
        Browser["IBrowser ← BrowserService (theme, scroll)"]
    end
    subgraph DOM["Domain layer (pure C#)"]
        Models["Models: records"]
        Logic["Validator, ReportCalculator, BudgetCheck, TransactionFilter, DataSerializer"]
        Res["Result&lt;T&gt;"]
    end
    Pages --> Comp
    Pages --> State
    Comp --> State
    State --> Logic
    State --> Storage
    Pages --> Browser
    Logic --> Models
    Logic --> Res
```

## Domain model

```mermaid
classDiagram
    class Transaction {
        <<record>>
        Guid Id
        TransactionType Type
        string Category
        decimal Amount
        DateOnly Date
        string Note
    }
    class TransactionDraft {
        <<record>>
        Guid? Id
        TransactionType Type
        string? Category
        decimal Amount
        DateOnly Date
        string? Note
    }
    class BudgetLimit {
        <<record>>
        string Category
        decimal MonthlyLimit
    }
    class AppSettings {
        <<record>>
        string Theme
        string Currency
    }
    class AppData {
        <<record>>
        IReadOnlyList~Transaction~ Transactions
        IReadOnlyList~BudgetLimit~ Limits
        AppSettings Settings
    }
    class Result~T~ {
        <<abstract record>>
        Ok(T Value)
        Fail(Errors)
        Map()
        Match()
    }
    AppData o-- Transaction
    AppData o-- BudgetLimit
    AppData *-- AppSettings
    TransactionDraft ..> Transaction : TransactionValidator.Validate()
```

## Flow: adding a transaction

```mermaid
sequenceDiagram
    actor User
    participant Form as TransactionForm
    participant State as AppState
    participant Val as TransactionValidator (pure)
    participant Store as IStorage (LocalStorage)
    participant UI as Subscribed components

    User->>Form: fills the form, presses "Add"
    Form->>Form: parse amount and date (InputParser)
    Form->>State: Save(TransactionDraft)
    State->>Val: Validate(draft, today)
    alt valid
        Val-->>State: Ok(Transaction)
        State->>State: _data = _data with { Transactions = ... }
        State->>Store: SetAsync(key, JSON)
        State-->>UI: OnChange
        UI->>UI: StateHasChanged (re-render)
        State-->>Form: Ok
        Form->>Form: reset fields
    else invalid
        Val-->>State: Fail(errors)
        State-->>Form: Fail(errors)
        Form->>User: shows the list of errors
    end
```

## Key design decisions

| Decision | Reason |
|---|---|
| One immutable `AppData` snapshot, replaced on every change | No hidden mutation; easy to reason about, serialize and test |
| `Result<T>` instead of exceptions for invalid input | Failure is an expected outcome, so it is part of the return type and cannot be forgotten |
| Domain as a separate project | Enforces "no UI in logic"; tests run without a browser; storage or UI can change independently |
| `IStorage` and `IBrowser` interfaces | Tests use `FakeStorage`; LocalStorage could later be replaced by an API without touching components |
| Source-generated JSON (`AppJsonContext`) | No runtime reflection, safe with Blazor trimming, faster start-up |
| `AppStateComponent` base class | One place for subscribe/unsubscribe, so no component can leak an event handler |
| `CascadingValue<AppSettings>` | Currency and theme reach every `MoneyText` without parameter passing |
| Templated `DataTable<TItem>` | The table skeleton (empty state, paging) is written once; callers decide the columns |
| Plain CSS and SVG, no libraries | Nothing extra to download, full control of responsive behaviour |

## Mapping to the grading criteria

| Criterion | Where to look |
|---|---|
| Code Structure and Design | Three projects; `Domain/Logic/*`; records and pure functions |
| Component Reusability | `TransactionForm` (parameters + callbacks), `DataTable<TItem>`, `MoneyText`, `LimitBar`, `ConfirmDialog` |
| Cross-Platform | Runs in any modern browser; QA matrix in [QA.md](QA.md) |
| State Management | `AppState`, `AppStateComponent`, `AppSettingsProvider` (cascading value) |
| UI / UX | `wwwroot/css/app.css`, empty states, confirmation dialogs, light/dark theme |
| Responsiveness | `@media` breakpoints at 900 px and 700 px; bottom navigation and card-style tables on phones |
| Core Functionality | Transactions, reports, limits, settings |
| Advanced Features | Category limits with 80% warning, 6-month trend chart, validation with `Result<T>` |
| Error Handling | `Result<T>`, corrupted-data backup, storage failure banner |
| Efficiency / Resources | `@key`, paged `DataTable`, no chart library, source-generated JSON, trimmed WASM build |
| Scalability | Storage behind `IStorage`; Domain independent of UI |
| Testing | `tests/BudgetTracker.Tests`: xUnit for Domain and `AppState`, bUnit for components |
| Quality Assurance | [QA.md](QA.md) checklist and device matrix |
| Documentation | README, this file, USER_GUIDE, QA |
| Innovation / Creative use | SVG charts built by hand, templated components, cascading settings |
| Professionalism | CI pipeline, consistent structure, accessible markup |
