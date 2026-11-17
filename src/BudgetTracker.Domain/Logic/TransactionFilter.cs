namespace BudgetTracker.Domain.Logic;

public sealed record TransactionFilter(
    TransactionType? Type = null,
    string? Category = null,
    string? Search = null,
    DateOnly? Month = null){
    public IReadOnlyList<Transaction> Apply(IEnumerable<Transaction> source){
        var query = source;

        if(Month is { } month)
            query = ReportCalculator.InMonth(query, month);

        if(Type is { } type)
            query = query.Where(t => t.Type == type);

        if(!string.IsNullOrWhiteSpace(Category)){
            var category = Category.Trim();
            query = query.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if(!string.IsNullOrWhiteSpace(Search)){
            var text = Search.Trim();
            query = query.Where(t =>
                t.Note.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                t.Category.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderByDescending(t => t.Date)
            .ThenBy(t => t.Category, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
