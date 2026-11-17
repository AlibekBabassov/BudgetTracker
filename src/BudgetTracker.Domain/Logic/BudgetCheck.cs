namespace BudgetTracker.Domain.Logic;

public enum LimitLevel {
    Ok,
    Warning,
    Exceeded
}

public sealed record LimitStatus(string Category, decimal Limit, decimal Spent, LimitLevel Level){
    public decimal Ratio => Limit == 0 ? 0 : Spent / Limit;
    public decimal Remaining => Limit - Spent;
}

public static class BudgetCheck {
    public const decimal WarningThreshold = 0.8m;

    public static LimitLevel LevelFor(decimal spent, decimal limit) =>
        spent > limit ? LimitLevel.Exceeded
        : spent >= limit * WarningThreshold ? LimitLevel.Warning
        : LimitLevel.Ok;

    public static IReadOnlyList<LimitStatus> Evaluate(
        IEnumerable<Transaction> transactions,
        IEnumerable<BudgetLimit> limits,
        DateOnly month){
        var spentByCategory = ReportCalculator
            .ByCategory(transactions, month, TransactionType.Expense)
            .ToDictionary(c => c.Category, c => c.Total, StringComparer.OrdinalIgnoreCase);

        return limits
            .Select(limit => {
                var spent = spentByCategory.TryGetValue(limit.Category, out var value) ? value : 0m;
                return new LimitStatus(limit.Category, limit.MonthlyLimit, spent, LevelFor(spent, limit.MonthlyLimit));
            })
            .OrderByDescending(s => s.Ratio)
            .ThenBy(s => s.Category, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static Result<BudgetLimit> ValidateLimit(string? category, decimal amount){
        var errors = new List<string>();
        var name = category?.Trim() ?? "";

        if(name.Length == 0)
            errors.Add("Choose a category.");

        if(amount <= 0)
            errors.Add("Limit must be greater than zero.");
        else if(amount > TransactionValidator.MaxAmount)
            errors.Add("Limit is too large.");

        return errors.Count > 0
            ? Result<BudgetLimit>.Failure(errors)
            : Result<BudgetLimit>.Success(new BudgetLimit(name, amount));
    }
}
