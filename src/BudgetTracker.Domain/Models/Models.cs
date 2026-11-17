namespace BudgetTracker.Domain.Models;

public enum TransactionType {
    Expense = 0,
    Income = 1
}

public sealed record Transaction(
    Guid Id,
    TransactionType Type,
    string Category,
    decimal Amount,
    DateOnly Date,
    string Note = "");

public sealed record TransactionDraft(
    Guid? Id,
    TransactionType Type,
    string? Category,
    decimal Amount,
    DateOnly Date,
    string? Note);

public sealed record BudgetLimit(string Category, decimal MonthlyLimit);

public sealed record AppSettings(string Theme, string Currency){
    public const string Light = "light";
    public const string Dark = "dark";

    public static AppSettings Default { get; } = new(Light, "₸");

    public static IReadOnlyList<string> Currencies { get; } = new[] { "₸", "$", "€", "₽", "£" };
}

public sealed record AppData(
    IReadOnlyList<Transaction> Transactions,
    IReadOnlyList<BudgetLimit> Limits,
    AppSettings Settings){
    public static AppData Empty { get; } = new(
        Array.Empty<Transaction>(),
        Array.Empty<BudgetLimit>(),
        AppSettings.Default);
}
