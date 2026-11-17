namespace BudgetTracker.Tests.Domain;

public class ReportCalculatorTests {
    private static readonly DateOnly March = new(2026, 3, 1);

    private static Transaction Tx(
        string category,
        decimal amount,
        DateOnly date,
        TransactionType type = TransactionType.Expense) =>
        new(Guid.NewGuid(), type, category, amount, date);

    [Fact]
    public void Summary_adds_up_income_and_expenses_of_the_month_only(){
        var data = new[] {
            Tx("Salary", 1000m, new DateOnly(2026, 3, 5), TransactionType.Income),
            Tx("Food", 200m, new DateOnly(2026, 3, 6)),
            Tx("Transport", 50m, new DateOnly(2026, 3, 20)),
            Tx("Food", 999m, new DateOnly(2026, 2, 28)),
            Tx("Salary", 5m, new DateOnly(2026, 4, 1), TransactionType.Income)
        };

        var summary = ReportCalculator.Summary(data, March);

        Assert.Equal(1000m, summary.Income);
        Assert.Equal(250m, summary.Expense);
        Assert.Equal(750m, summary.Balance);
    }

    [Fact]
    public void Summary_of_an_empty_month_is_zero(){
        var summary = ReportCalculator.Summary(Array.Empty<Transaction>(), March);

        Assert.Equal(0m, summary.Income);
        Assert.Equal(0m, summary.Expense);
        Assert.Equal(0m, summary.Balance);
    }

    [Fact]
    public void ByCategory_groups_ignoring_case_and_sorts_by_total(){
        var data = new[] {
            Tx("Food", 100m, new DateOnly(2026, 3, 1)),
            Tx("food", 50m, new DateOnly(2026, 3, 2)),
            Tx("Transport", 150m, new DateOnly(2026, 3, 3)),
            Tx("Other", 50m, new DateOnly(2026, 3, 4))
        };

        var result = ReportCalculator.ByCategory(data, March);

        Assert.Equal(3, result.Count);
        Assert.Equal("Food", result[0].Category);
        Assert.Equal(150m, result[0].Total);
        Assert.Equal("Transport", result[1].Category);
        Assert.Equal("Other", result[2].Category);
        Assert.Equal(50m, result[2].Total);
    }

    [Fact]
    public void ByCategory_shares_add_up_to_one(){
        var data = new[] {
            Tx("Food", 100m, new DateOnly(2026, 3, 1)),
            Tx("Transport", 150m, new DateOnly(2026, 3, 3)),
            Tx("Other", 100m, new DateOnly(2026, 3, 4))
        };

        var result = ReportCalculator.ByCategory(data, March);

        Assert.Equal(1m, result.Sum(c => c.Share), 5);
    }

    [Fact]
    public void ByCategory_only_counts_the_requested_type(){
        var data = new[] {
            Tx("Salary", 1000m, new DateOnly(2026, 3, 1), TransactionType.Income),
            Tx("Food", 100m, new DateOnly(2026, 3, 2))
        };

        var income = ReportCalculator.ByCategory(data, March, TransactionType.Income);

        var only = Assert.Single(income);
        Assert.Equal("Salary", only.Category);
        Assert.Equal(1m, only.Share);
    }

    [Fact]
    public void ByCategory_of_an_empty_month_is_empty(){
        Assert.Empty(ReportCalculator.ByCategory(Array.Empty<Transaction>(), March));
    }

    [Fact]
    public void Trend_returns_the_requested_months_ending_at_the_given_month(){
        var data = new[] {
            Tx("Food", 40m, new DateOnly(2026, 2, 10)),
            Tx("Salary", 500m, new DateOnly(2026, 3, 1), TransactionType.Income)
        };

        var trend = ReportCalculator.Trend(data, new DateOnly(2026, 3, 17), months: 3);

        Assert.Equal(3, trend.Count);
        Assert.Equal(new DateOnly(2026, 1, 1), trend[0].Month);
        Assert.Equal(new DateOnly(2026, 2, 1), trend[1].Month);
        Assert.Equal(new DateOnly(2026, 3, 1), trend[2].Month);
        Assert.Equal(40m, trend[1].Expense);
        Assert.Equal(500m, trend[2].Income);
        Assert.Equal(0m, trend[0].Income);
    }

    [Fact]
    public void Trend_crosses_year_boundaries(){
        var trend = ReportCalculator.Trend(Array.Empty<Transaction>(), new DateOnly(2026, 1, 20), months: 3);

        Assert.Equal(new DateOnly(2025, 11, 1), trend[0].Month);
        Assert.Equal(new DateOnly(2026, 1, 1), trend[2].Month);
    }
}
