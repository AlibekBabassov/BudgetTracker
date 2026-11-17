namespace BudgetTracker.Tests.Domain;

public class TransactionFilterTests {
    private static Transaction Tx(
        TransactionType type, string category, decimal amount, DateOnly date, string note = "") =>
        new(Guid.NewGuid(), type, category, amount, date, note);

    private static readonly Transaction[] Data = {
        Tx(TransactionType.Expense, "Food", 10m, new DateOnly(2026, 3, 5), "Pizza night"),
        Tx(TransactionType.Expense, "Transport", 5m, new DateOnly(2026, 3, 9), "Bus"),
        Tx(TransactionType.Income, "Salary", 900m, new DateOnly(2026, 3, 1), "March pay"),
        Tx(TransactionType.Expense, "Food", 8m, new DateOnly(2026, 2, 20), "Lunch")
    };

    [Fact]
    public void Without_conditions_everything_is_returned_newest_first(){
        var result = new TransactionFilter().Apply(Data);

        Assert.Equal(4, result.Count);
        Assert.Equal(new DateOnly(2026, 3, 9), result[0].Date);
        Assert.Equal(new DateOnly(2026, 2, 20), result[3].Date);
    }

    [Fact]
    public void Filters_by_type(){
        var result = new TransactionFilter(Type: TransactionType.Income).Apply(Data);

        Assert.Equal("Salary", Assert.Single(result).Category);
    }

    [Fact]
    public void Filters_by_category_ignoring_case(){
        var result = new TransactionFilter(Category: "food").Apply(Data);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal("Food", t.Category));
    }

    [Fact]
    public void Search_looks_in_notes_and_categories_ignoring_case(){
        Assert.Equal("Pizza night", Assert.Single(new TransactionFilter(Search: "PIZZA").Apply(Data)).Note);
        Assert.Equal("Transport", Assert.Single(new TransactionFilter(Search: "tran").Apply(Data)).Category);
    }

    [Fact]
    public void Filters_by_month(){
        var result = new TransactionFilter(Month: new DateOnly(2026, 2, 1)).Apply(Data);

        Assert.Equal("Lunch", Assert.Single(result).Note);
    }

    [Fact]
    public void Conditions_combine(){
        var filter = new TransactionFilter(TransactionType.Expense, "Food", null, new DateOnly(2026, 3, 1));

        var result = filter.Apply(Data);

        Assert.Equal("Pizza night", Assert.Single(result).Note);
    }

    [Fact]
    public void Blank_text_conditions_are_ignored(){
        var result = new TransactionFilter(Category: "  ", Search: "").Apply(Data);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void Applying_a_filter_does_not_change_the_source(){
        var before = Data.ToList();

        new TransactionFilter(Type: TransactionType.Income).Apply(Data);

        Assert.Equal(before, Data.ToList());
    }
}
