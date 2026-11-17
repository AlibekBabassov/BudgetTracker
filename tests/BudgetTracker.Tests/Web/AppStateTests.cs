using BudgetTracker.Tests.Support;
using BudgetTracker.Web.Services;

namespace BudgetTracker.Tests.Web;

public class AppStateTests {
    private static readonly DateOnly Today = new(2026, 3, 15);

    private static AppState Create(FakeStorage? storage = null) => new(storage ?? new FakeStorage(), () => Today);

    private static async Task<AppState> CreateLoadedAsync(FakeStorage? storage = null){
        var state = Create(storage);
        await state.InitializeAsync();
        return state;
    }

    private static TransactionDraft Draft(decimal amount = 25m, string category = "Food") =>
        new(null, TransactionType.Expense, category, amount, Today, "note");

    [Fact]
    public async Task A_fresh_start_is_loaded_empty_and_on_the_current_month(){
        var state = await CreateLoadedAsync();

        Assert.True(state.IsLoaded);
        Assert.Empty(state.Transactions);
        Assert.Null(state.LoadWarning);
        Assert.Null(state.StorageError);
        Assert.Equal(new DateOnly(2026, 3, 1), state.SelectedMonth);
    }

    [Fact]
    public async Task Saving_a_valid_transaction_stores_it_and_announces_the_change(){
        var storage = new FakeStorage();
        var state = await CreateLoadedAsync(storage);
        var changes = 0;
        state.OnChange += () => changes++;

        var result = await state.SaveTransactionAsync(Draft());

        Assert.IsType<Result<Transaction>.Ok>(result);
        Assert.Single(state.Transactions);
        Assert.Equal(1, changes);
        Assert.True(storage.Data.ContainsKey(AppState.StorageKey));
    }

    [Fact]
    public async Task Saved_data_is_restored_by_a_new_state_container(){
        var storage = new FakeStorage();
        var first = await CreateLoadedAsync(storage);
        await first.SaveTransactionAsync(Draft(42m));
        await first.SetLimitAsync("Food", 300m);

        var second = await CreateLoadedAsync(storage);

        Assert.Equal(42m, Assert.Single(second.Transactions).Amount);
        Assert.Equal(300m, Assert.Single(second.Limits).MonthlyLimit);
    }

    [Fact]
    public async Task An_invalid_transaction_changes_nothing(){
        var storage = new FakeStorage();
        var state = await CreateLoadedAsync(storage);
        var changes = 0;
        state.OnChange += () => changes++;

        var result = await state.SaveTransactionAsync(Draft(amount: 0m));

        Assert.IsType<Result<Transaction>.Fail>(result);
        Assert.Empty(state.Transactions);
        Assert.Equal(0, changes);
        Assert.False(storage.Data.ContainsKey(AppState.StorageKey));
    }

    [Fact]
    public async Task Saving_with_an_existing_id_replaces_the_transaction(){
        var state = await CreateLoadedAsync();
        var created = Assert.IsType<Result<Transaction>.Ok>(await state.SaveTransactionAsync(Draft(10m))).Value;

        await state.SaveTransactionAsync(new TransactionDraft(created.Id, TransactionType.Expense, "Food", 99m, Today, ""));

        var only = Assert.Single(state.Transactions);
        Assert.Equal(created.Id, only.Id);
        Assert.Equal(99m, only.Amount);
    }

    [Fact]
    public async Task Deleting_removes_only_that_transaction(){
        var state = await CreateLoadedAsync();
        var first = Assert.IsType<Result<Transaction>.Ok>(await state.SaveTransactionAsync(Draft(10m))).Value;
        await state.SaveTransactionAsync(Draft(20m));

        await state.DeleteTransactionAsync(first.Id);

        Assert.Equal(20m, Assert.Single(state.Transactions).Amount);
    }

    [Fact]
    public async Task Unreadable_saved_data_starts_empty_keeps_a_backup_and_warns(){
        var storage = new FakeStorage();
        storage.Data[AppState.StorageKey] = "{broken";

        var state = await CreateLoadedAsync(storage);

        Assert.True(state.IsLoaded);
        Assert.Empty(state.Transactions);
        Assert.NotNull(state.LoadWarning);
        Assert.Equal("{broken", storage.Data[AppState.CorruptBackupKey]);
    }

    [Fact]
    public async Task The_load_warning_can_be_dismissed(){
        var storage = new FakeStorage();
        storage.Data[AppState.StorageKey] = "{broken";
        var state = await CreateLoadedAsync(storage);

        state.DismissLoadWarning();

        Assert.Null(state.LoadWarning);
    }

    [Fact]
    public async Task When_storage_refuses_writes_the_data_stays_in_memory_and_the_user_is_told(){
        var storage = new FakeStorage();
        var state = await CreateLoadedAsync(storage);
        storage.FailWrites = true;

        var result = await state.SaveTransactionAsync(Draft());

        Assert.IsType<Result<Transaction>.Ok>(result);
        Assert.Single(state.Transactions);
        Assert.NotNull(state.StorageError);
    }

    [Fact]
    public async Task A_later_successful_write_clears_the_storage_error(){
        var storage = new FakeStorage();
        var state = await CreateLoadedAsync(storage);
        storage.FailWrites = true;
        await state.SaveTransactionAsync(Draft());
        storage.FailWrites = false;

        await state.SaveTransactionAsync(Draft(30m));

        Assert.Null(state.StorageError);
    }

    [Fact]
    public async Task When_storage_cannot_be_read_the_app_still_starts(){
        var storage = new FakeStorage { FailReads = true };

        var state = await CreateLoadedAsync(storage);

        Assert.True(state.IsLoaded);
        Assert.NotNull(state.StorageError);
        Assert.Empty(state.Transactions);
    }

    [Fact]
    public async Task Setting_a_limit_twice_for_a_category_keeps_only_the_newest(){
        var state = await CreateLoadedAsync();

        await state.SetLimitAsync("Food", 100m);
        await state.SetLimitAsync("food", 250m);

        var limit = Assert.Single(state.Limits);
        Assert.Equal(250m, limit.MonthlyLimit);
    }

    [Fact]
    public async Task An_invalid_limit_is_rejected(){
        var state = await CreateLoadedAsync();

        var result = await state.SetLimitAsync("Food", 0m);

        Assert.IsType<Result<BudgetLimit>.Fail>(result);
        Assert.Empty(state.Limits);
    }

    [Fact]
    public async Task Removing_a_limit_works_ignoring_case(){
        var state = await CreateLoadedAsync();
        await state.SetLimitAsync("Food", 100m);

        await state.RemoveLimitAsync("FOOD");

        Assert.Empty(state.Limits);
    }

    [Fact]
    public async Task Changing_the_month_announces_a_change(){
        var state = await CreateLoadedAsync();
        var changes = 0;
        state.OnChange += () => changes++;

        state.ShiftMonth(-1);

        Assert.Equal(new DateOnly(2026, 2, 1), state.SelectedMonth);
        Assert.Equal(1, changes);
    }

    [Fact]
    public async Task The_theme_is_saved_and_unknown_values_fall_back_to_light(){
        var storage = new FakeStorage();
        var state = await CreateLoadedAsync(storage);

        await state.SetThemeAsync("dark");
        Assert.Equal("dark", (await CreateLoadedAsync(storage)).Settings.Theme);

        await state.SetThemeAsync("purple");
        Assert.Equal("light", state.Settings.Theme);
    }

    [Fact]
    public async Task Unknown_currencies_are_ignored(){
        var state = await CreateLoadedAsync();

        await state.SetCurrencyAsync("$");
        await state.SetCurrencyAsync("???");

        Assert.Equal("$", state.Settings.Currency);
    }

    [Fact]
    public async Task Deleting_all_data_keeps_the_settings(){
        var state = await CreateLoadedAsync();
        await state.SetThemeAsync("dark");
        await state.SaveTransactionAsync(Draft());
        await state.SetLimitAsync("Food", 100m);

        await state.ClearAllAsync();

        Assert.Empty(state.Transactions);
        Assert.Empty(state.Limits);
        Assert.Equal("dark", state.Settings.Theme);
    }

    [Fact]
    public async Task Unsubscribed_handlers_are_no_longer_called(){
        var state = await CreateLoadedAsync();
        var calls = 0;
        void Handler() => calls++;

        state.OnChange += Handler;
        await state.SaveTransactionAsync(Draft());
        state.OnChange -= Handler;
        await state.SaveTransactionAsync(Draft(30m));

        Assert.Equal(1, calls);
    }
}
