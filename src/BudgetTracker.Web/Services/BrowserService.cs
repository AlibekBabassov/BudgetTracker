using Microsoft.JSInterop;

namespace BudgetTracker.Web.Services;

public interface IBrowser {
    Task SetThemeAsync(string theme);

    Task ScrollToTopAsync();
}

public sealed class BrowserService : IBrowser {
    private readonly IJSRuntime _js;

    public BrowserService(IJSRuntime js) => _js = js;

    public async Task SetThemeAsync(string theme) =>
        await _js.InvokeVoidAsync("budgetInterop.setTheme", theme);

    public async Task ScrollToTopAsync() =>
        await _js.InvokeVoidAsync("budgetInterop.scrollToTop");
}
