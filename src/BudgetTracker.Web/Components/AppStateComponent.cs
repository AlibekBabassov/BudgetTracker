using BudgetTracker.Web.Services;
using Microsoft.AspNetCore.Components;

namespace BudgetTracker.Web.Components;

public abstract class AppStateComponent : ComponentBase, IDisposable {
    [Inject]
    protected AppState State { get; set; } = default!;

    protected override void OnInitialized() => State.OnChange += HandleStateChanged;

    private void HandleStateChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose(){
        State.OnChange -= HandleStateChanged;
        GC.SuppressFinalize(this);
    }
}
