using Bunit;
using BudgetTracker.Web.Components;
using Microsoft.AspNetCore.Components;
using TestContext = Bunit.TestContext;

namespace BudgetTracker.Tests.Web;

public class MoneyTextTests {
    [Fact]
    public void Uses_the_default_currency_when_none_is_cascaded(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<MoneyText>(p => p.Add(c => c.Value, 1234.5m));

        Assert.Equal("1,234.50 ₸", cut.Find("span").TextContent);
    }

    [Fact]
    public void Uses_the_cascaded_currency(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<CascadingValue<AppSettings>>(p => p
            .Add(c => c.Value, new AppSettings("light", "$"))
            .AddChildContent<MoneyText>(m => m.Add(x => x.Value, 12.5m)));

        Assert.Equal("12.50 $", cut.Find("span").TextContent);
    }

    [Fact]
    public void Signed_income_gets_a_plus_and_the_income_class(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<MoneyText>(p => p
            .Add(c => c.Value, 10m)
            .Add(c => c.Kind, TransactionType.Income)
            .Add(c => c.Signed, true));

        var span = cut.Find("span");
        Assert.StartsWith("+", span.TextContent);
        Assert.Contains("income", span.ClassName);
    }

    [Fact]
    public void Signed_expense_gets_a_minus_and_the_expense_class(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<MoneyText>(p => p
            .Add(c => c.Value, 10m)
            .Add(c => c.Kind, TransactionType.Expense)
            .Add(c => c.Signed, true));

        var span = cut.Find("span");
        Assert.StartsWith("−", span.TextContent);
        Assert.Contains("expense", span.ClassName);
    }
}

public class AlertBannerTests {
    [Fact]
    public void Shows_the_message_and_details(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<AlertBanner>(p => p
            .Add(c => c.Kind, "danger")
            .Add(c => c.Message, "Something failed")
            .Add(c => c.Details, new[] { "Line 2: bad date", "Line 3: bad amount" }));

        Assert.Contains("Something failed", cut.Markup);
        Assert.Equal(2, cut.FindAll("li").Count);
        Assert.Contains("alert-danger", cut.Markup);
    }

    [Fact]
    public void Has_no_dismiss_button_unless_a_handler_is_given(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<AlertBanner>(p => p
            .Add(c => c.Kind, "info")
            .Add(c => c.Message, "Hello"));

        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void Dismiss_button_raises_the_callback(){
        using var ctx = new TestContext();
        var dismissed = false;

        var cut = ctx.RenderComponent<AlertBanner>(p => p
            .Add(c => c.Kind, "info")
            .Add(c => c.Message, "Hello")
            .Add(c => c.OnDismiss, EventCallback.Factory.Create(ctx, (Action)(() => dismissed = true))));

        cut.Find("button").Click();

        Assert.True(dismissed);
    }
}

public class ConfirmDialogTests {
    [Fact]
    public void Renders_nothing_while_hidden(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<ConfirmDialog>(p => p.Add(c => c.Visible, false));

        Assert.DoesNotContain("modal", cut.Markup);
    }

    [Fact]
    public void Shows_title_and_message_when_visible(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<ConfirmDialog>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.Title, "Delete it?")
            .Add(c => c.Message, "This cannot be undone."));

        Assert.Contains("Delete it?", cut.Markup);
        Assert.Contains("This cannot be undone.", cut.Markup);
    }

    [Fact]
    public void Confirm_and_cancel_raise_their_own_callbacks(){
        using var ctx = new TestContext();
        var confirmed = 0;
        var cancelled = 0;

        var cut = ctx.RenderComponent<ConfirmDialog>(p => p
            .Add(c => c.Visible, true)
            .Add(c => c.OnConfirm, EventCallback.Factory.Create(ctx, (Action)(() => confirmed++)))
            .Add(c => c.OnCancel, EventCallback.Factory.Create(ctx, (Action)(() => cancelled++))));

        cut.Find(".btn-danger").Click();
        Assert.Equal(1, confirmed);
        Assert.Equal(0, cancelled);

        cut.Find(".modal-actions .btn:not(.btn-danger)").Click();
        Assert.Equal(1, cancelled);
    }
}

public class LimitBarTests {
    [Fact]
    public void An_exceeded_limit_is_marked_and_the_bar_is_capped_at_100_percent(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<LimitBar>(p => p
            .Add(c => c.Status, new LimitStatus("Food", 100m, 120m, LimitLevel.Exceeded)));

        Assert.Contains("limit-exceeded", cut.Markup);
        Assert.Contains("Over the limit by", cut.Markup);
        Assert.Contains("width:100%", cut.Markup);
    }

    [Fact]
    public void A_limit_that_is_fine_shows_the_real_percentage(){
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<LimitBar>(p => p
            .Add(c => c.Status, new LimitStatus("Food", 100m, 40m, LimitLevel.Ok)));

        Assert.Contains("limit-ok", cut.Markup);
        Assert.Contains("width:40%", cut.Markup);
        Assert.Contains("left", cut.Markup);
    }

    [Fact]
    public void Remove_button_passes_the_category_to_the_callback(){
        using var ctx = new TestContext();
        string? removed = null;

        var cut = ctx.RenderComponent<LimitBar>(p => p
            .Add(c => c.Status, new LimitStatus("Transport", 50m, 10m, LimitLevel.Ok))
            .Add(c => c.OnRemove, EventCallback.Factory.Create<string>(ctx, (Action<string>)(c => removed = c))));

        cut.Find("button").Click();

        Assert.Equal("Transport", removed);
    }
}

public class DataTableTests {
    private static IRenderedComponent<DataTable<string>> Render(TestContext ctx, IReadOnlyList<string> items, int pageSize = 25) =>
        ctx.RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.PageSize, pageSize)
            .Add(c => c.EmptyText, "Nothing here")
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, item => $"<td>{item}</td>"));

    [Fact]
    public void Shows_the_empty_text_when_there_are_no_items(){
        using var ctx = new TestContext();

        var cut = Render(ctx, new List<string>());

        Assert.Contains("Nothing here", cut.Markup);
        Assert.Empty(cut.FindAll("table"));
    }

    [Fact]
    public void Renders_the_header_and_one_row_per_item(){
        using var ctx = new TestContext();

        var cut = Render(ctx, new List<string> { "a", "b", "c" });

        Assert.Contains("Name", cut.Find("thead").TextContent);
        Assert.Equal(3, cut.FindAll("tbody tr").Count);
    }

    [Fact]
    public void Long_lists_show_a_page_first_and_grow_on_demand(){
        using var ctx = new TestContext();
        IReadOnlyList<string> items = Enumerable.Range(1, 30).Select(i => $"item {i}").ToList();

        var cut = Render(ctx, items, pageSize: 25);

        Assert.Equal(25, cut.FindAll("tbody tr").Count);

        cut.Find("button").Click();

        Assert.Equal(30, cut.FindAll("tbody tr").Count);
        Assert.Empty(cut.FindAll("button"));
    }
}

public class TransactionFormTests {
    private static readonly DateOnly Today = new(2026, 3, 15);

    private static readonly Transaction Sample =
        new(Guid.NewGuid(), TransactionType.Expense, "Food", 10m, Today, "");

    private static IRenderedComponent<TransactionForm> Render(
        TestContext ctx,
        Func<TransactionDraft, Task<Result<Transaction>>> save,
        Action<Transaction>? onSaved = null) =>
        ctx.RenderComponent<TransactionForm>(p => p
            .Add(c => c.Today, Today)
            .Add(c => c.Save, save)
            .Add(c => c.OnSaved, EventCallback.Factory.Create<Transaction>(ctx, onSaved ?? (_ => { }))));

    [Fact]
    public void The_date_starts_as_today(){
        using var ctx = new TestContext();

        var cut = Render(ctx, _ => Task.FromResult(Result<Transaction>.Success(Sample)));

        Assert.Equal("2026-03-15", cut.Find("#tx-date").GetAttribute("value"));
    }

    [Fact]
    public void A_text_that_is_not_a_number_is_reported_and_nothing_is_saved(){
        using var ctx = new TestContext();
        var called = false;
        var cut = Render(ctx, _ => {
            called = true;
            return Task.FromResult(Result<Transaction>.Success(Sample));
        });

        cut.Find("#tx-amount").Input("abc");
        cut.Find("form").Submit();

        Assert.False(called);
        Assert.Contains("Amount must be a number", cut.Markup);
    }

    [Fact]
    public void Validation_errors_from_the_save_function_are_shown(){
        using var ctx = new TestContext();
        var cut = Render(ctx, _ => Task.FromResult(Result<Transaction>.Failure("Choose a category.")));

        cut.Find("#tx-amount").Input("10");
        cut.Find("form").Submit();

        Assert.Contains("Choose a category.", cut.Markup);
    }

    [Fact]
    public void A_valid_form_sends_the_typed_values_notifies_and_resets(){
        using var ctx = new TestContext();
        TransactionDraft? sent = null;
        Transaction? saved = null;
        var cut = Render(
            ctx,
            draft => {
                sent = draft;
                return Task.FromResult(Result<Transaction>.Success(Sample));
            },
            t => saved = t);

        cut.FindAll(".seg")[1].Click();
        cut.Find("#tx-amount").Input("12,5");
        cut.Find("#tx-note").Change("March pay");
        cut.Find("form").Submit();

        Assert.NotNull(sent);
        Assert.Equal(TransactionType.Income, sent!.Type);
        Assert.Equal("Salary", sent.Category);
        Assert.Equal(12.5m, sent.Amount);
        Assert.Equal(new DateOnly(2026, 3, 15), sent.Date);
        Assert.Equal("March pay", sent.Note);
        Assert.Same(Sample, saved);
        Assert.Equal("", cut.Find("#tx-amount").GetAttribute("value") ?? "");
    }

    [Fact]
    public void Editing_fills_the_form_with_the_existing_transaction(){
        using var ctx = new TestContext();
        var existing = new Transaction(Guid.NewGuid(), TransactionType.Income, "Gift", 55.5m, new DateOnly(2026, 2, 1), "Birthday");

        var cut = ctx.RenderComponent<TransactionForm>(p => p
            .Add(c => c.Today, Today)
            .Add(c => c.Editing, existing)
            .Add(c => c.Save, _ => Task.FromResult(Result<Transaction>.Success(existing))));

        Assert.Equal("55.5", cut.Find("#tx-amount").GetAttribute("value"));
        Assert.Equal("2026-02-01", cut.Find("#tx-date").GetAttribute("value"));
        Assert.Equal("Birthday", cut.Find("#tx-note").GetAttribute("value"));
        Assert.Contains("Save changes", cut.Markup);
        Assert.Contains("Cancel", cut.Markup);
    }
}
