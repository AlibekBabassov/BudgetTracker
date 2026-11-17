namespace BudgetTracker.Domain.Models;

public static class Categories {
    public static IReadOnlyList<string> ExpenseCategories { get; } =
        new[] { "Food", "Transport", "Housing", "Health", "Entertainment", "Shopping", "Education", "Other" };

    public static IReadOnlyList<string> IncomeCategories { get; } =
        new[] { "Salary", "Scholarship", "Gift", "Other" };

    public static IReadOnlyList<string> ForType(TransactionType type) =>
        type == TransactionType.Income ? IncomeCategories : ExpenseCategories;
}
