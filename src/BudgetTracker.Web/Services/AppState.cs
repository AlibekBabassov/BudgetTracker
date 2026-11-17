using BudgetTracker.Domain.Common;
using BudgetTracker.Domain.Logic;
using BudgetTracker.Domain.Models;

namespace BudgetTracker.Web.Services;

public sealed class AppState {
    public const string StorageKey = "budget-tracker.data.v1";
    public const string CorruptBackupKey = "budget-tracker.data.v1.corrupt";

    private readonly IStorage _storage;
    private readonly Func<DateOnly> _today;
    private AppData _data = AppData.Empty;
    private DateOnly _selectedMonth;

    public AppState(IStorage storage, Func<DateOnly>? today = null){
        _storage = storage;
        _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today));
        _selectedMonth = ReportCalculator.FirstOfMonth(_today());
    }

    public event Action? OnChange;

    public bool IsLoaded { get; private set; }

    public string? LoadWarning { get; private set; }

    public string? StorageError { get; private set; }

    public IReadOnlyList<Transaction> Transactions => _data.Transactions;

    public IReadOnlyList<BudgetLimit> Limits => _data.Limits;

    public AppSettings Settings => _data.Settings;

    public DateOnly SelectedMonth => _selectedMonth;

    public DateOnly Today => _today();

    public async Task InitializeAsync(){
        if(IsLoaded)
            return;

        string? raw = null;

        try{
            raw = await _storage.GetAsync(StorageKey);
        }catch(Exception){
            StorageError = "Browser storage is not available. Your changes will not be saved.";
        }

        switch(DataSerializer.Deserialize(raw)){
            case Result<AppData>.Ok ok:
                _data = ok.Value;
                break;

            case Result<AppData>.Fail:
                LoadWarning = "Saved data could not be read, so the app started empty. A copy of the damaged data was kept in the browser.";
                await TrySaveAsync(CorruptBackupKey, raw ?? "");
                break;
        }

        IsLoaded = true;
        NotifyChanged();
    }

    public void DismissLoadWarning(){
        LoadWarning = null;
        NotifyChanged();
    }

    public void DismissStorageError(){
        StorageError = null;
        NotifyChanged();
    }

    public async Task<Result<Transaction>> SaveTransactionAsync(TransactionDraft draft){
        var result = TransactionValidator.Validate(draft, Today);

        if(result is Result<Transaction>.Ok ok){
            var saved = ok.Value;
            var exists = _data.Transactions.Any(t => t.Id == saved.Id);

            var next = exists
                ? _data.Transactions.Select(t => t.Id == saved.Id ? saved : t).ToList()
                : _data.Transactions.Append(saved).ToList();

            await CommitAsync(_data with { Transactions = next });
        }

        return result;
    }

    public async Task DeleteTransactionAsync(Guid id){
        var next = _data.Transactions.Where(t => t.Id != id).ToList();
        await CommitAsync(_data with { Transactions = next });
    }

    public async Task<Result<BudgetLimit>> SetLimitAsync(string? category, decimal amount){
        var result = BudgetCheck.ValidateLimit(category, amount);

        if(result is Result<BudgetLimit>.Ok ok){
            var limit = ok.Value;
            var next = _data.Limits
                .Where(l => !l.Category.Equals(limit.Category, StringComparison.OrdinalIgnoreCase))
                .Append(limit)
                .ToList();

            await CommitAsync(_data with { Limits = next });
        }

        return result;
    }

    public async Task RemoveLimitAsync(string category){
        var next = _data.Limits
            .Where(l => !l.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();

        await CommitAsync(_data with { Limits = next });
    }

    public async Task SetThemeAsync(string theme){
        var normalized = theme == AppSettings.Dark ? AppSettings.Dark : AppSettings.Light;
        await CommitAsync(_data with { Settings = _data.Settings with { Theme = normalized } });
    }

    public async Task SetCurrencyAsync(string currency){
        if(!AppSettings.Currencies.Contains(currency))
            return;

        await CommitAsync(_data with { Settings = _data.Settings with { Currency = currency } });
    }

    public async Task ClearAllAsync() =>
        await CommitAsync(AppData.Empty with { Settings = _data.Settings });

    public void SetMonth(DateOnly month){
        _selectedMonth = ReportCalculator.FirstOfMonth(month);
        NotifyChanged();
    }

    public void ShiftMonth(int months) => SetMonth(_selectedMonth.AddMonths(months));

    private async Task CommitAsync(AppData next){
        _data = next;
        await PersistAsync();
        NotifyChanged();
    }

    private Task PersistAsync() => TrySaveAsync(StorageKey, DataSerializer.Serialize(_data));

    private async Task TrySaveAsync(string key, string value){
        try{
            await _storage.SetAsync(key, value);
            StorageError = null;
        }catch(Exception){
            StorageError = "Could not save to browser storage (it may be full or disabled). Changes will be lost when you close this tab.";
        }
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
