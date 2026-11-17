namespace BudgetTracker.Domain.Logic;

public static class TransactionValidator {
    public const decimal MaxAmount = 1_000_000_000m;
    public const int MaxNoteLength = 100;
    public const int MaxCategoryLength = 30;
    public static readonly DateOnly EarliestDate = new(2000, 1, 1);

    public static Result<Transaction> Validate(TransactionDraft draft, DateOnly today){
        var errors = new List<string>();
        var category = draft.Category?.Trim() ?? "";
        var note = draft.Note?.Trim() ?? "";

        if(draft.Amount <= 0)
            errors.Add("Amount must be greater than zero.");
        else if(draft.Amount > MaxAmount)
            errors.Add("Amount is too large.");
        else if(decimal.Round(draft.Amount, 2) != draft.Amount)
            errors.Add("Amount can have at most 2 decimal places.");

        if(category.Length == 0)
            errors.Add("Choose a category.");
        else if(category.Length > MaxCategoryLength)
            errors.Add($"Category must be at most {MaxCategoryLength} characters.");

        if(draft.Date < EarliestDate)
            errors.Add("Date is too far in the past.");
        else if(draft.Date > today.AddYears(1))
            errors.Add("Date is too far in the future.");

        if(note.Length > MaxNoteLength)
            errors.Add($"Note must be at most {MaxNoteLength} characters.");

        return errors.Count > 0
            ? Result<Transaction>.Failure(errors)
            : Result<Transaction>.Success(
                new Transaction(draft.Id ?? Guid.NewGuid(), draft.Type, category, draft.Amount, draft.Date, note));
    }
}
