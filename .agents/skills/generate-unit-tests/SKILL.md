---
name: generate-unit-tests
description: Generate xUnit tests following established patterns for a given C# source file, covering untested public methods using NSubstitute for mocks and Shouldly for assertions. Use this skill whenever the user asks to add tests, generate unit tests, write test coverage, or create a test file for a C# class — even if they just say "add tests for this" or "what needs test coverage".
---

# Generate Unit Tests

Generate xUnit tests following established patterns for a given file, covering untested public methods using NSubstitute for mocks and Shouldly for assertions.

## Context Validation Checkpoints

- Is the source file path provided and accessible?
- Does the target class have public methods that need testing?
- Are there existing tests for this class to avoid duplication?
- Are dependencies identifiable for mocking with NSubstitute?
- Is the test project location known for placing the new test file?
- If any checkpoint cannot be resolved with confidence, stop and ask the user for clarification before generating or modifying tests.

## Implementation Steps

### Step 1: Analyze Source File
Read the source file to identify the class name, namespace, public methods, and dependencies. Determine which methods need test coverage based on the optional method name parameter.

### Step 2: Check Existing Tests
Search for existing test files matching the pattern `*Tests.cs` for the target class. Identify which public methods are already tested to avoid duplication.

### Step 3: Determine Test File Location
Locate the appropriate test project (in `tests/` directory). Determine if a new test file is needed or if tests should be added to an existing one.

The test namespace follows the convention `MakaMek.MapEditor.Test.{Category}` (e.g., `MakaMek.MapEditor.Test.ViewModels`, `MakaMek.MapEditor.Test.Services`).

### Step 4: Generate Test Class Structure
Create the test class with proper naming (`<ClassName>Tests`), namespace matching the test project structure, and `_sut` field for the system under test.
Always use single test class in a separate file for each class under test.

```csharp
using NSubstitute;
using Shouldly;
using Xunit;
using Sanet.MVVM.Core.Services;

namespace MakaMek.MapEditor.Test.ViewModels;

public class NewMapViewModelTests
{
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly NewMapViewModel _sut;

    public NewMapViewModelTests()
    {
        _sut = new NewMapViewModel(/* dependencies */);
        _sut.SetNavigationService(_navigationService); // required if VM uses NavigationService
    }
}
```

**Key rules:**
- Call `_sut.SetNavigationService(_navigationService)` in the constructor for any ViewModel that accesses `NavigationService`.
- For field-backed commands (`IAsyncCommand Cmd => field ??= new AsyncCommand(...)`), call `_sut.Cmd.ExecuteAsync()` directly in tests — these commands have no setter and cannot be mocked via NSubstitute.

### Step 5: Generate Test Methods
For each untested public method, generate test methods using the arrange-act-assert pattern with descriptive underscore-separated names.

```csharp
[Fact]
public void MethodName_Condition_ExpectedResult()
{
    // Arrange
    _rulesProvider.GetRules().Returns(new Rules());
    
    // Act
    var result = _sut.MethodName();
    
    // Assert
    result.ShouldBeTrue();
}
```

### Step 6: Add Parameterized Tests Where Applicable
For methods with multiple input scenarios, use `[Theory]` and `[InlineData]` for parameterized testing.

```csharp
[Theory]
[InlineData(2, 100.0)]
[InlineData(12, 0.0)]
public void CalculateHitProbability_WithRoll_ReturnsExpectedValue(int roll, double expected)
{
    var result = _sut.CalculateHitProbability(roll);
    result.ShouldBe(expected);
}
```

### Step 7: Add Required Usings and Constructor
Include all necessary using statements (xUnit, NSubstitute, Shouldly, Sanet.MVVM.Core.Services) and implement the constructor with dependency initialization.

**Core usings:**
```csharp
using Xunit;
using NSubstitute;
using Shouldly;
using Sanet.MVVM.Core.Services; // INavigationService
```

**Additional usings for common MapEditor test patterns:**
```csharp
using Microsoft.Extensions.Logging;          // ILogger<T>
using System.Reactive.Concurrency;           // IScheduler, ImmediateScheduler
using Sanet.MakaMek.Localization;            // ILocalizationService
using Sanet.MakaMek.Services;               // IFileService
using Sanet.MakaMek.Assets.Services;        // ITerrainAssetService
```

**Infrastructure dependency mocking patterns:**
```csharp
private readonly ILogger<EditMapViewModel> _logger = Substitute.For<ILogger<EditMapViewModel>>();
private readonly IScheduler _scheduler = ImmediateScheduler.Instance; // or Substitute.For<IScheduler>()
private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();

public ConstructorName()
{
    _localizationService.GetString(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());
    _sut = new MyViewModel(/* ... */);
}
```

### Step 8: Write or Update Test File
Create the new test file or append tests to the existing one, ensuring proper formatting and organization.
If file location, class mapping, or coverage status is ambiguous, request clarification first.

### Common Patterns in MapEditor Tests

**Testing field-backed AsyncCommands (no setter, cannot be mocked):**
```csharp
// VM: public IAsyncCommand CreateMapCommand => field ??= new AsyncCommand(Execute);
// Test:
await _sut.CreateMapCommand.ExecuteAsync();
_navigationService.Received(1).NavigateToViewModelAsync(Arg.Any<EditMapViewModel>());
```

**Mocking ILocalizationService to return keys by default:**
```csharp
_localizationService.GetString(Arg.Any<string>()).Returns(callInfo => callInfo.Arg<string>());
```

**Mocking INavigationService.GetViewModel to return a substitute:**
```csharp
var editVm = Substitute.For<EditMapViewModel>( /* deps */ );
_navigationService.GetViewModel<EditMapViewModel>().Returns(editVm);
```

**Asserting navigation was called (or not called) with async commands:**
```csharp
await _navigationService.Received(1).NavigateToViewModelAsync(editVm);
await _navigationService.DidNotReceive().NavigateToViewModelAsync(Arg.Any<EditMapViewModel>());
```