namespace BudgetTracker.Domain.Logic;

public sealed record MonthSummary(decimal Income, decimal Expense){
    public decimal Balance => Income - Expense;
}

public sealed record CategoryTotal(string Category, decimal Total, decimal Share);

public sealed record MonthPoint(DateOnly Month, decimal Income, decimal Expense);

public static class ReportCalculator {
    public static DateOnly FirstOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    public static IEnumerable<Transaction> InMonth(IEnumerable<Transaction> transactions, DateOnly month) =>
        transactions.Where(t => t.Date.Year == month.Year && t.Date.Month == month.Month);

    public static MonthSummary Summary(IEnumerable<Transaction> transactions, DateOnly month){
        var inMonth = InMonth(transactions, month).ToList();
        return new MonthSummary(
            inMonth.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            inMonth.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount));
    }

    public static IReadOnlyList<CategoryTotal> ByCategory(
        IEnumerable<Transaction> transactions,
        DateOnly month,
        TransactionType type = TransactionType.Expense){
        var totals = InMonth(transactions, month)
            .Where(t => t.Type == type)
            .GroupBy(t => t.Category, StringComparer.OrdinalIgnoreCase)
            .Select(g => (Category: g.Key, Total: g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var grandTotal = totals.Sum(x => x.Total);

        return totals
            .Select(x => new CategoryTotal(x.Category, x.Total, grandTotal == 0 ? 0 : x.Total / grandTotal))
            .ToList();
    }

    public static IReadOnlyList<MonthPoint> Trend(IEnumerable<Transaction> transactions, DateOnly endMonth, int months = 6){
        var all = transactions.ToList();
        var end = FirstOfMonth(endMonth);

        return Enumerable.Range(0, months)
            .Select(i => end.AddMonths(i - (months - 1)))
            .Select(month => {
                var summary = Summary(all, month);
                return new MonthPoint(month, summary.Income, summary.Expense);
            })
            .ToList();
    }
}
