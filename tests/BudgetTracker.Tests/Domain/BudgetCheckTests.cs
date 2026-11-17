namespace BudgetTracker.Tests.Domain;

public class BudgetCheckTests {
    private static readonly DateOnly March = new(2026, 3, 1);

    private static Transaction Expense(string category, decimal amount) =>
        new(Guid.NewGuid(), TransactionType.Expense, category, amount, new DateOnly(2026, 3, 10));

    [Fact]
    public void Below_eighty_percent_is_ok(){
        Assert.Equal(LimitLevel.Ok, BudgetCheck.LevelFor(79m, 100m));
    }

    [Fact]
    public void From_eighty_percent_up_to_the_limit_is_a_warning(){
        Assert.Equal(LimitLevel.Warning, BudgetCheck.LevelFor(80m, 100m));
        Assert.Equal(LimitLevel.Warning, BudgetCheck.LevelFor(100m, 100m));
    }

    [Fact]
    public void Above_the_limit_is_exceeded(){
        Assert.Equal(LimitLevel.Exceeded, BudgetCheck.LevelFor(100.01m, 100m));
    }

    [Fact]
    public void Evaluate_sums_spending_per_category_and_sorts_by_how_full_the_limit_is(){
        var transactions = new[] {
            Expense("Food", 70m),
            Expense("food", 50m),
            Expense("Transport", 10m)
        };
        var limits = new[] { new BudgetLimit("Transport", 50m), new BudgetLimit("Food", 100m) };

        var result = BudgetCheck.Evaluate(transactions, limits, March);

        Assert.Equal("Food", result[0].Category);
        Assert.Equal(120m, result[0].Spent);
        Assert.Equal(LimitLevel.Exceeded, result[0].Level);
        Assert.Equal(-20m, result[0].Remaining);

        Assert.Equal("Transport", result[1].Category);
        Assert.Equal(LimitLevel.Ok, result[1].Level);
        Assert.Equal(40m, result[1].Remaining);
    }

    [Fact]
    public void A_limit_without_spending_is_ok_with_zero_spent(){
        var result = BudgetCheck.Evaluate(
            Array.Empty<Transaction>(), new[] { new BudgetLimit("Health", 200m) }, March);

        var status = Assert.Single(result);
        Assert.Equal(0m, status.Spent);
        Assert.Equal(LimitLevel.Ok, status.Level);
    }

    [Fact]
    public void Income_does_not_count_as_spending(){
        var transactions = new[] {
            new Transaction(Guid.NewGuid(), TransactionType.Income, "Food", 500m, new DateOnly(2026, 3, 2))
        };

        var result = BudgetCheck.Evaluate(transactions, new[] { new BudgetLimit("Food", 100m) }, March);

        Assert.Equal(0m, Assert.Single(result).Spent);
    }

    [Fact]
    public void ValidateLimit_accepts_a_category_and_a_positive_amount(){
        var result = BudgetCheck.ValidateLimit("  Food ", 250m);

        var ok = Assert.IsType<Result<BudgetLimit>.Ok>(result);
        Assert.Equal("Food", ok.Value.Category);
        Assert.Equal(250m, ok.Value.MonthlyLimit);
    }

    [Fact]
    public void ValidateLimit_rejects_missing_category_and_non_positive_amount(){
        var result = BudgetCheck.ValidateLimit("", 0m);

        var fail = Assert.IsType<Result<BudgetLimit>.Fail>(result);
        Assert.Equal(2, fail.Errors.Count);
    }
}
