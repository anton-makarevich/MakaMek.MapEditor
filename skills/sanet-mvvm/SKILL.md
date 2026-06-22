---
name: sanet-mvvm
description: "Use this skill whenever working in a project that references Sanet.MVVM packages (Sanet.MVVM.Core, Sanet.MVVM.Navigation.Avalonia, Sanet.MVVM.Views.Avalonia, Sanet.MVVM.DI.Avalonia). Triggers include: creating or editing ViewModels, setting up navigation, implementing modal dialogs, registering views/VMs, configuring DI, wiring up commands, managing lifecycle (AttachHandlers / DetachHandlers), or bootstrapping an AvaloniaUI app with this framework. Also trigger when the user asks how to pass data between screens, show action dialogs, compose child ViewModels, or handle reactive subscriptions inside a Sanet.MVVM project. Consult this skill even for seemingly small tasks like adding a command or a new screen — the framework has specific patterns that must be followed."
metadata:
  author: Anton Makarevich
  version: "1.0"
  framework: Sanet.MVVM
  platform: AvaloniaUI
---
# Sanet.MVVM – Agent Reference

Lightweight MVVM framework for AvaloniaUI. Key packages:

`Sanet.MVVM.Core` · `Sanet.MVVM.Navigation.Avalonia` ·
`Sanet.MVVM.Views.Avalonia` · `Sanet.MVVM.DI.Avalonia`

---

## Quick Decision Tree

Before writing any code, pick the right path:

| Task | Go to |
|---|---|
| New screen / ViewModel | §3 ViewModel + §4 View + §5 Registration |
| Navigate to existing screen | §6 Navigation |
| Modal / popup dialog (with or without a return value) | §7 Modal Dialogs |
| Simple yes/no action dialog | §8 Action Dialogs |
| First-time app setup | §1 Bootstrap + §2 DI |
| Pass data to the next screen | §6 → Data Passing |
| Subscriptions / observables | §9 Lifecycle Patterns |
| Child VM (no navigation) | §9 → Child ViewModels |

---

## §1 — App Bootstrap

Bootstrap is split across three locations:

**1. `Program.cs` (per-platform)** — Configures the app builder and registers platform-specific services:

```csharp
// Desktop/Program.cs
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseDependencyInjection(services => services.RegisterPlatformServices())
    .StartWithClassicDesktopLifetime(args);
```

**2. Extension methods** — Register common / domain services in a static class (called from any platform):

```csharp
public static void RegisterServices(this IServiceCollection services)
{
    services.AddSingleton<IFileService, AvaloniaFileService>();
    services.AddTransient<MainViewModel>();
    services.AddTransient<DetailViewModel>();
}
```

**3. `App.axaml.cs`** — Wire up navigation, register views, and navigate to the first screen:

```csharp
// App.axaml.cs — OnFrameworkInitializationCompleted
if (Resources[AppBuilderExtensions.ServiceCollectionResourceKey]
        is not IServiceCollection services)
    throw new Exception("Services not initialized");

services.RegisterServices();        // calls extension methods from step 2
services.RegisterViewModels();      // registers all VMs as transient

var sp = services.BuildServiceProvider();

if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
{
    var navService = new NavigationService(desktop, sp);
    RegisterViews(navService);              // see §5
    var mainVm = navService.GetViewModel<MainViewModel>();
    desktop.MainWindow = new MainWindow { Content = new MainMenuView { ViewModel = mainVm } };
}
else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
{
    var wrapper = new ContentControl();
    var navService = new SingleViewNavigationService(singleView, wrapper, sp);
    RegisterViews(navService);
    var mainVm = navService.GetViewModel<MainViewModel>();
    wrapper.Content = new MainMenuView { ViewModel = mainVm };
}
```

---

## §2 — Dependency Injection

**Rules:**
- `INavigationService` → created manually in `App.axaml.cs` with the lifetime object and service provider.
- ViewModels → **transient** (fresh state on every `GetNewViewModel<T>()` / `GetViewModel<T>()` call).
- Shared services (repositories, settings, etc.) → **singleton**.

```csharp
// INavigationService — created in App.axaml.cs (not in DI container)
var navService = new NavigationService(desktop, sp);

// ViewModels — always transient
services.AddTransient<MainViewModel>();
services.AddTransient<DetailViewModel>();
services.AddTransient<FilterViewModel>();

// Domain services — singleton
services.AddSingleton<IGameService, GameService>();
```

---

## §3 — ViewModels

Inherit from `BaseViewModel`. **Do not** start work in the constructor.

```csharp
public class MyViewModel : BaseViewModel
{
    // ── Properties ────────────────────────────────────────────────────
    public string Title
    {
        get;
        set => SetProperty(ref field, value);   // raises PropertyChanged (C# 13+ field keyword)
    }

    // ── Commands ───────────────────────────────────────────────────────
    public IAsyncCommand GoToDetailCommand => field ??= new AsyncCommand(GoToDetail);

    public MyViewModel(IMyService service)
    {
        _service = service;
        // NO startup work here — use AttachHandlers instead
    }

    // ── Lifecycle ──────────────────────────────────────────────────────
    public override void AttachHandlers()   // view attached to visual tree
    {
        base.AttachHandlers();
        LoadDataAsync().SafeFireAndForget();
        _service.SomethingChanged += OnSomethingChanged;
    }

    public override void DetachHandlers()   // view detached
    {
        _service.SomethingChanged -= OnSomethingChanged;
        base.DetachHandlers();
    }

    // ── Navigation helper ──────────────────────────────────────────────
    private async Task GoToDetail()
    {
        var vm = NavigationService.GetViewModel<DetailViewModel>();
        if (vm == null) return; // not registered
        vm.ItemId = _selectedId;
        await NavigationService.NavigateToViewModelAsync(vm);
    }
}
```

**Key points:**
- Use `=> field ??= new AsyncCommand(...)` for commands — this is the modern C# 13+ pattern (no explicit backing field needed). These commands cannot be mocked via NSubstitute (no setter); call `ExecuteAsync()` directly in tests.
- Keep the constructor lightweight — move data loading and event subscriptions to `AttachHandlers`.
- Use `GetViewModel<T>()` (returns null if unregistered) instead of `GetNewViewModel<T>()` (throws) unless you want exception semantics.

**Provided by `BaseViewModel`:**

| Member | Purpose |
|---|---|
| `NavigationService` | Lazy `INavigationService`; throws if not injected |
| `SetNavigationService(INavigationService)` | Injects the navigation service (required for VMs not navigated to through the framework, e.g. modal VMs created with `new`) |
| `IsBusy` | Bindable busy flag |
| `ExpectsResult` | Set by framework for modal VMs |
| `BackCommand` | Calls `NavigateBackAsync` |
| `CloseAsync(result)` | Closes the view, fires `OnResult` |
| `SetProperty(ref field, value)` | Property + change notification |
| `NotifyPropertyChanged(nameof(X))` | Manual single-prop notification |
| `NotifyAllPropertiesChanged()` | Refresh all bindings |

---

## §4 — Views

```csharp
// MyView.axaml.cs
public partial class MyView : BaseView<MyViewModel>
{
    public MyView() => InitializeComponent();

    // Optional — called once after ViewModel is assigned (DataContext already set)
    protected override void OnViewModelSet()
    {
        // safe to access ViewModel here
    }
}
```

The framework automatically calls `AttachHandlers()` / `DetachHandlers()` on visual-tree events — no manual wiring needed.

---

## §5 — View Registration

Every ViewModel–View pair **must** be registered. Do this after building the service provider, before the first navigation.

```csharp
void RegisterViews(INavigationService nav)
{
    nav.RegisterViews(typeof(MainView),     typeof(MainViewModel));
    nav.RegisterViews(typeof(DetailView),   typeof(DetailViewModel));
    nav.RegisterViews(typeof(FilterView),   typeof(FilterViewModel));
    nav.RegisterViews(typeof(SettingsView), typeof(SettingsViewModel));

    // Responsive / platform variants
    if (IsMobile())
        nav.RegisterViews(typeof(DetailViewNarrow), typeof(DetailViewModel));
    else
        nav.RegisterViews(typeof(DetailViewWide),   typeof(DetailViewModel));
}
```

> ⚠️ Missing registration → runtime exception on first navigation to that VM.

---

## §6 — Navigation

```csharp
// ── Forward — resolve existing VM (returns null if unregistered) ──────
var vm = NavigationService.GetViewModel<DetailViewModel>();
if (vm == null) return;
vm.ItemId = selectedId;          // pass data via properties before navigating
await NavigationService.NavigateToViewModelAsync(vm);

// ── Forward — create new VM (throws if unregistered) ──────────────────
var vm2 = NavigationService.GetNewViewModel<DetailViewModel>()
    ?? throw new InvalidOperationException("DetailViewModel not registered");
await NavigationService.NavigateToViewModelAsync(vm2);

// ── Forward — generic overload (no VM instance needed) ────────────────
await NavigationService.NavigateToViewModelAsync<NewMapViewModel>();

// ── Forward (reuse existing state) ────────────────────────────────────
var shared = NavigationService.GetViewModel<SharedViewModel>();
await NavigationService.NavigateToViewModelAsync(shared);

// ── Back ───────────────────────────────────────────────────────────────
await NavigationService.NavigateBackAsync();

// ── Root ───────────────────────────────────────────────────────────────
await NavigationService.NavigateToRootAsync();

// ── Check registration ─────────────────────────────────────────────────
bool ok = NavigationService.HasViewModel<SomeViewModel>();
```

**Data-passing patterns:**

```csharp
// 1. Property injection (preferred for simple values)
vm.Item = selectedItem;
await NavigationService.NavigateToViewModelAsync(vm);

// 2. Initialize method (preferred for multiple / complex args)
vm.Initialize(game, reason);
await NavigationService.NavigateToViewModelAsync(vm);
```

---

## §7 — Modal Dialogs (Overlay / Popup Style)

Use `ShowViewModelForResultAsync` whenever you need a **modal overlay** — whether or not the dialog returns a value. The overlay is rendered via OverlayLayer with a semi-transparent backdrop and is **not** pushed onto the back stack.

### 7a — Modal without a return value

When you just need to show a popup and don't need data back, implement `IResultProvider<bool>` (or any throwaway type) and resolve it on close:

```csharp
public class InfoDialogViewModel : BaseViewModel, IResultProvider<bool>
{
    private readonly TaskCompletionSource<bool> _tcs = new();

    public Task<bool> GetResultAsync() => _tcs.Task;

    public async Task Close()
    {
        _tcs.SetResult(true);   // value is ignored by caller
        await CloseAsync();
    }
}

// ── Caller ────────────────────────────────────────────────────────────
var vm = NavigationService.GetViewModel<InfoDialogViewModel>();
await NavigationService.ShowViewModelForResultAsync<InfoDialogViewModel, bool>(vm);
// execution continues here after the user closes the dialog
```

### 7b — Modal with a typed return value

When the dialog must hand data back to the caller, use a meaningful result type:

```csharp
public class FilterViewModel : BaseViewModel, IResultProvider<FilterCriteria?>
{
    private readonly TaskCompletionSource<FilterCriteria?> _tcs = new();

    public Task<FilterCriteria?> GetResultAsync() => _tcs.Task;

    public async Task Apply()
    {
        _tcs.SetResult(BuildCriteria());
        await CloseAsync();
    }

    public async Task Cancel()
    {
        _tcs.SetResult(null);
        await CloseAsync();
    }
}

// ── Caller ────────────────────────────────────────────────────────────
var filterVm = NavigationService.GetViewModel<FilterViewModel>();
var criteria = await NavigationService
    .ShowViewModelForResultAsync<FilterViewModel, FilterCriteria?>(filterVm);

if (criteria != null)
    ApplyFilter(criteria);
```

**Rules for all modal dialogs:**
- Always resolve `_tcs` before calling `CloseAsync()` — never leave it hanging.
- Do **not** use `BackCommand` or `NavigateBackAsync` inside modal VMs; close via `CloseAsync()`.
- The caller's `await` unblocks as soon as `GetResultAsync()` completes.

### 7c — Sub-navigation from a modal VM

`ShowViewModelForResultAsync` does **not** inject `NavigationService` into the target VM. If a modal VM needs to show its own sub-dialogs (e.g. an info popup triggered from within the modal), the parent must call `SetNavigationService` before showing the modal:

```csharp
// Parent VM (has NavigationService from navigation framework)
var modalVm = new MyModalViewModel(data, service);
modalVm.SetNavigationService(NavigationService);    // required for sub-navigation
await NavigationService.ShowViewModelForResultAsync<MyModalViewModel, ResultType>(modalVm);
```

> ⚠️ **Missing `SetNavigationService`** → the modal VM's `NavigationService` property will throw `ArgumentNullException` on first access.

---

## §8 — Action Dialogs (Built-in)

For simple user choices that don't need a custom screen.

```csharp
var yes = new UiAction { Title = "Delete" };
var no  = new UiAction { Title = "Cancel" };

var chosen = await NavigationService.AskForActionAsync(
    "Delete item?",
    "This cannot be undone.",
    yes, no);

if (chosen == yes)
    await DeleteAsync();
```

`UiAction` also supports `Command` / `CommandParameter` for button-specific logic.

---

## §9 — Lifecycle Patterns

### Reactive subscriptions

```csharp
private IDisposable? _sub;

public override void AttachHandlers()
{
    base.AttachHandlers();
    _sub = _service.Updates
        .ObserveOn(_dispatcher.Scheduler)
        .Subscribe(OnUpdate);
}

public override void DetachHandlers()
{
    _sub?.Dispose();
    base.DetachHandlers();
}
```

### Child ViewModels (composed inline, no navigation)

```csharp
public class ParentViewModel : BaseViewModel
{
    public MapConfigViewModel MapConfig { get; }

    public ParentViewModel(IMapService mapService)
    {
        MapConfig = new MapConfigViewModel(mapService);
    }
}
```

### IDisposable

Implement when the VM owns resources that outlive visual-tree detach:

```csharp
public class MyViewModel : BaseViewModel, IDisposable
{
    public void Dispose()
    {
        _subscription?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

### PropertyChanged forwarding (child VM to parent)

When a parent VM needs to react to changes in a child VM's properties, subscribe in the constructor and unsubscribe in `DetachHandlers`:

```csharp
public class ParentViewModel : BaseViewModel
{
    private readonly PropertyChangedEventHandler? _childHandler;

    public ChildViewModel Child { get; }

    public ParentViewModel()
    {
        Child = new ChildViewModel();
        _childHandler = (_, _) => NotifyPropertyChanged(nameof(Child));
        Child.PropertyChanged += _childHandler;
    }

    public override void DetachHandlers()
    {
        Child.PropertyChanged -= _childHandler;
        base.DetachHandlers();
    }
}
```

### ILocalizationService integration

VMs that need localized strings should receive `ILocalizationService` via constructor injection. Expose it as a public property so views and tests can access it:

```csharp
public class MyViewModel : BaseViewModel
{
    public ILocalizationService LocalizationService { get; }

    public MyViewModel(ILocalizationService localizationService)
    {
        LocalizationService = localizationService;
    }
}
```

---

## §10 — Checklist for New Screens

When adding a screen, do **all** of these steps:

- [ ] Create `XyzViewModel : BaseViewModel` in the ViewModels project
- [ ] Register `services.AddTransient<XyzViewModel>()` in DI setup
- [ ] Create `XyzView : BaseView<XyzViewModel>` in the Views project
- [ ] Call `nav.RegisterViews(typeof(XyzView), typeof(XyzViewModel))` in `RegisterViews`
- [ ] Navigate using `GetViewModel<XyzViewModel>()` or `GetNewViewModel<XyzViewModel>()` (not `new XyzViewModel(...)`)
- [ ] Put startup logic in `AttachHandlers`, not the constructor
- [ ] Clean up subscriptions/resources in `DetachHandlers`
- [ ] Use `AsyncCommand` for any async `ICommand` (prefer `=> field ??= new AsyncCommand(...)` syntax)

---

## §11 — Anti-Patterns to Avoid

| ❌ Don't | ✅ Do instead |
|---|---|
| `new MyViewModel(...)` directly | `NavigationService.GetNewViewModel<MyViewModel>()` |
| Start async work in constructor | Use `AttachHandlers` |
| Subscribe in constructor | Subscribe in `AttachHandlers`, unsubscribe in `DetachHandlers` |
| Forget to register view/VM pair | Always add to `RegisterViews` |
| Use `NavigateToViewModelAsync` for modal/popup dialogs | Use `ShowViewModelForResultAsync` |
| Resolve `NavigationService` in constructor | Access it lazily via the `NavigationService` property |
| Use `NavigationService` in a modal VM without calling `SetNavigationService` first | Parent calls `modalVm.SetNavigationService(NavigationService)` before `ShowViewModelForResultAsync` |
| Use old-style `{ get; set; }` with explicit backing field for commands | Use `=> field ??= new AsyncCommand(...)` (C# 13+ field keyword) |
| Expose `ILocalizationService` as a private field | Expose as a public property so views and tests can access it |
