namespace BudgetTracker.Domain.Common;

public abstract record Result<T> {
    public sealed record Ok(T Value) : Result<T>;

    public sealed record Fail(IReadOnlyList<string> Errors) : Result<T>;

    public bool IsOk => this is Ok;

    public static Result<T> Success(T value) => new Ok(value);

    public static Result<T> Failure(params string[] errors) => new Fail(errors);

    public static Result<T> Failure(IEnumerable<string> errors) => new Fail(errors.ToList());

    public TOut Match<TOut>(Func<T, TOut> ok, Func<IReadOnlyList<string>, TOut> fail) => this switch {
        Ok success => ok(success.Value),
        Fail failure => fail(failure.Errors),
        _ => throw new InvalidOperationException("Unknown result case.")
    };

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) => this switch {
        Ok success => Result<TOut>.Success(mapper(success.Value)),
        Fail failure => Result<TOut>.Failure(failure.Errors),
        _ => throw new InvalidOperationException("Unknown result case.")
    };
}
