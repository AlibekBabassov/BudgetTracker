using System.Globalization;

namespace BudgetTracker.Tests.Domain;

public class TransactionValidatorTests {
    private static readonly DateOnly Today = new(2026, 3, 15);

    private static TransactionDraft Draft(
        decimal amount = 10m,
        string? category = "Food",
        DateOnly? date = null,
        string? note = "lunch",
        TransactionType type = TransactionType.Expense,
        Guid? id = null) =>
        new(id, type, category, amount, date ?? Today, note);

    private static IReadOnlyList<string> ErrorsOf(Result<Transaction> result) =>
        Assert.IsType<Result<Transaction>.Fail>(result).Errors;

    [Fact]
    public void Valid_draft_becomes_a_transaction(){
        var result = TransactionValidator.Validate(Draft(), Today);

        var ok = Assert.IsType<Result<Transaction>.Ok>(result);
        Assert.Equal("Food", ok.Value.Category);
        Assert.Equal(10m, ok.Value.Amount);
        Assert.Equal(Today, ok.Value.Date);
        Assert.NotEqual(Guid.Empty, ok.Value.Id);
    }

    [Fact]
    public void Existing_id_is_kept_when_editing(){
        var id = Guid.NewGuid();

        var result = TransactionValidator.Validate(Draft(id: id), Today);

        Assert.Equal(id, Assert.IsType<Result<Transaction>.Ok>(result).Value.Id);
    }

    [Fact]
    public void Category_and_note_are_trimmed(){
        var result = TransactionValidator.Validate(Draft(category: "  Food  ", note: "  tea "), Today);

        var ok = Assert.IsType<Result<Transaction>.Ok>(result);
        Assert.Equal("Food", ok.Value.Category);
        Assert.Equal("tea", ok.Value.Note);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public void Amount_must_be_positive(string amountText){
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        var errors = ErrorsOf(TransactionValidator.Validate(Draft(amount: amount), Today));

        Assert.Contains("Amount must be greater than zero.", errors);
    }

    [Fact]
    public void Amount_cannot_have_more_than_two_decimals(){
        var errors = ErrorsOf(TransactionValidator.Validate(Draft(amount: 10.005m), Today));

        Assert.Contains("Amount can have at most 2 decimal places.", errors);
    }

    [Fact]
    public void Amount_cannot_be_absurdly_large(){
        var errors = ErrorsOf(TransactionValidator.Validate(Draft(amount: TransactionValidator.MaxAmount + 1), Today));

        Assert.Contains("Amount is too large.", errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Category_is_required(string? category){
        var errors = ErrorsOf(TransactionValidator.Validate(Draft(category: category), Today));

        Assert.Contains("Choose a category.", errors);
    }

    [Fact]
    public void Date_more_than_a_year_ahead_is_rejected(){
        var tooFar = Today.AddYears(1).AddDays(1);

        var errors = ErrorsOf(TransactionValidator.Validate(Draft(date: tooFar), Today));

        Assert.Contains("Date is too far in the future.", errors);
    }

    [Fact]
    public void Date_exactly_one_year_ahead_is_allowed(){
        var result = TransactionValidator.Validate(Draft(date: Today.AddYears(1)), Today);

        Assert.IsType<Result<Transaction>.Ok>(result);
    }

    [Fact]
    public void Date_before_the_year_2000_is_rejected(){
        var errors = ErrorsOf(TransactionValidator.Validate(Draft(date: new DateOnly(1999, 12, 31)), Today));

        Assert.Contains("Date is too far in the past.", errors);
    }

    [Fact]
    public void Note_has_a_maximum_length(){
        var errors = ErrorsOf(TransactionValidator.Validate(
            Draft(note: new string('x', TransactionValidator.MaxNoteLength + 1)), Today));

        Assert.Contains($"Note must be at most {TransactionValidator.MaxNoteLength} characters.", errors);
    }

    [Fact]
    public void All_problems_are_reported_together(){
        var errors = ErrorsOf(TransactionValidator.Validate(Draft(amount: 0m, category: ""), Today));

        Assert.Equal(2, errors.Count);
    }
}
