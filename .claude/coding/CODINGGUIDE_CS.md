# .NET Coding Style Guide

## Overview

This guide defines the coding standards for .NET C# development. It combines strict SOLID principles with Microsoft's C# coding conventions and specific team preferences.

## Architecture Principles

### SOLID Principles - Extreme Application

- **Single Responsibility**: Each class should have ONE reason to change - literally one thing
- **Lego-Style Architecture**: Prefer many small, focused classes over fewer large ones
- **Small, Isolated Units**: Each class should be a small, composable building block
- **Interface Segregation**: Interfaces defined by client use cases, not by object behavior
- **Immutable Interfaces**: Favor immutable interfaces and data structures
- **Composition over Inheritance**: Embrace composition aggressively

### SOLID Examples

**Class Granularity:**
- ❌ BAD: UserService with CreateUser(), ValidateEmail(), HashPassword(), SendWelcomeEmail()
- ✅ GOOD: Separate classes: UserCreator, EmailValidator, PasswordHasher, WelcomeEmailSender

**Interface Segregation:**
- ❌ BAD: Create IDataAccessor interface that provides access to ALL kind of data
- ✅ BETTER: Create IUserAccessor, IOrderAccessor interfaces for each entity
- ✅ BEST: Create ILoginAccessor interface for each consumer so every piece of code knows only what they should know

**Dependency Injection:**
```csharp
// Prefer this level of granularity - each dependency does ONE thing
public class OrderProcessor
{
    private readonly IOrderValidator mValidator;
    private readonly IInventoryChecker mInventoryChecker;
    private readonly IPaymentProcessor mPaymentProcessor;
    private readonly IOrderPersister mPersister;
    private readonly IOrderConfirmationSender mConfirmationSender;
}
```

## Indentation & Formatting

- **Spaces, not tabs** for indentation
- **4 spaces** per indentation level
- **Opening curly brace on the next line** (Allman style) - braces align with current indentation level
- **One statement per line**
- **One declaration per line**
- **NEVER use curly braces for single-line statements** after `if`/`else`/`for`/`while`/`foreach` - omit the braces when the body is a single statement
- **Always use curly braces for multi-statement blocks** - even if it's just 2 statements
- **Limit lines to 65 characters** for better readability
- **Line breaks before binary operators** when needed

### Brace Style Examples

```csharp
// ✅ CORRECT - Single statement, no braces
if (user == null)
    throw new ArgumentNullException(nameof(user));

// ✅ CORRECT - Multiple statements, use braces
if (user == null)
{
    LogError("User is null");
    throw new ArgumentNullException(nameof(user));
}

// ❌ WRONG - Single statement should not have braces
if (user == null)
{
    throw new ArgumentNullException(nameof(user));
}

// ✅ CORRECT - Single statement in loop, no braces
for (int i = 0; i < count; i++)
    ProcessItem(i);

// ✅ CORRECT - Multiple statements in loop, use braces
for (int i = 0; i < count; i++)
{
    var item = GetItem(i);
    ProcessItem(item);
}
```

## Naming Conventions

### Casing
- **PascalCase** for: class names, method names, public properties, constants, namespaces
- **camelCase** for: local variables, method parameters

### Prefixes
- ❌ **No prefix**: Classes, methods, public properties, constants
- ✅ **`I` prefix**: Interfaces (e.g., `IUserRepository`)
- ✅ **`A` prefix**: Abstract classes (e.g., `ABaseService`)
- ✅ **`m` prefix**: Instance fields (e.g., `mConnection`)
- ✅ **`g` prefix**: Static fields (e.g., `gDefaultTimeout`)

### General Naming Rules
- Use meaningful and descriptive names
- Prefer clarity over brevity
- Avoid abbreviations except for widely known ones
- Avoid single-letter names except for simple loop counters
- Don't use two consecutive underscores

## Fields & Encapsulation

- **No public fields** except constants
- Use properties for public access
- Mark fields as `readonly` when they won't change after construction
- Private instance fields use `m` prefix
- Static fields use `g` prefix

## Argument Validation

- **Validate all arguments in public methods**
- Check for null on reference type parameters
- Validate ranges, empty collections, and business rules as appropriate
- Throw `ArgumentNullException` for null arguments
- Throw `ArgumentException` for invalid arguments

```csharp
public void ProcessOrder(Order order)
{
    if (order == null)
        throw new ArgumentNullException(nameof(order));

    if (order.Items.Count == 0)
        throw new ArgumentException("Order must contain at least one item", nameof(order));

    // Process order...
}
```

## Type Usage

- **Use language keywords** over runtime types: `string` not `String`, `int` not `Int32`
- **Use `int` rather than unsigned types** for consistency and interoperability
- **Use `var` when type is obvious** from the right side of assignment (e.g., `new`, literal values, explicit casts)
- **Don't use `var`** when type isn't clear from the expression

```csharp
// Good use of var
var message = "This is clearly a string";
var customer = new Customer();
var count = 42;

// Don't use var here
int iterations = Convert.ToInt32(Console.ReadLine());
Customer result = FindCustomer(id);
```

## Modern C# Features

### String Handling
- **String interpolation** for concatenating strings: `$"{lastName}, {firstName}"`
- **StringBuilder** for string concatenation in loops
- **Raw string literals** for multi-line strings with escape sequences

### Collections & Objects
- **Collection expressions** to initialize collections: `string[] vowels = ["a", "e", "i", "o", "u"];`
- **Object initializers**: `new Person { Name = "John", Age = 30 }`
- **Target-typed `new()`** when variable type is explicit: `Person person = new();`

### Namespaces & Initialization
- **File-scoped namespace declarations**: `namespace MyApp.Features;`
- **Primary constructors** for simple initialization
- **Required properties** instead of constructors when forcing initialization

## LINQ Usage

- **Avoid LINQ unless specifically requested or clearly beneficial**
- Prefer explicit loops and conditionals for clarity
- LINQ can obscure performance characteristics and make debugging harder
- When using LINQ:
  - Use meaningful names for query variables
  - Use implicit typing (`var`) for LINQ query variables
  - Align query clauses under the `from` clause

## Async & Exception Handling

### Async Operations
- **Use async/await** for I/O-bound operations
- Be cautious of deadlocks
- Use `ConfigureAwait(false)` when appropriate in library code

### Exception Handling
- **Catch specific exceptions** - avoid catching `System.Exception` without filters
- **Use `using` statements** for `IDisposable` resources (prefer the concise form without braces)
- **Use `try-catch`** for exception handling
- **Rethrow with `throw;`** (not `throw ex;`) to preserve stack trace

```csharp
// Preferred using statement (no braces)
using Font font = new Font("Arial", 10.0f);

// Exception handling
try
{
    ProcessData();
}
catch (InvalidOperationException ex)
{
    LogError(ex);
    throw; // Preserves stack trace
}
```

## Operators & Logic

- **Use `&&` and `||`** (not `&` and `|`) for boolean operations to enable short-circuiting
- Use parentheses to make precedence clear when needed

```csharp
if ((divisor != 0) && (dividend / divisor > threshold))
{
    // Process...
}
```

## Delegates & Events

- **Prefer `Func<>` and `Action<>`** over custom delegate types
- **Use lambda expressions** for event handlers that don't need removal

```csharp
// Good
Action<string> log = message => Console.WriteLine(message);
Func<int, int, int> add = (x, y) => x + y;

// Event handler
this.Click += (s, e) => HandleClick(e);
```

## Comments & Documentation

### XML Documentation
**XML comments (`///`) required for all public APIs:**
- `<summary>` describing what the method/class does
- `<param>` for each parameter explaining its purpose
- `<returns>` describing the return value
- `<exception>` for any exceptions that may be thrown

### Inline Comments
**Single-line comments (`//`) for explaining WHY, not WHAT:**
- Explain the reason or intent behind code blocks
- Explain non-obvious behavior or business rules
- **Avoid commenting self-explanatory code**
- Don't state the obvious (e.g., "Loop through items", "Check if null")

### Comment Style
- **One space** between `//` and comment text
- **Start comments with uppercase**, end with period
- **Place comments on separate lines**, not at end of code lines

```csharp
/// <summary>
/// Processes the payment for an order.
/// </summary>
/// <param name="order">The order to process payment for.</param>
/// <param name="paymentMethod">The payment method to use.</param>
/// <returns>True if payment succeeded, false otherwise.</returns>
/// <exception cref="ArgumentNullException">Thrown when order or paymentMethod is null.</exception>
public bool ProcessPayment(Order order, PaymentMethod paymentMethod)
{
    if (order == null)
        throw new ArgumentNullException(nameof(order));

    if (paymentMethod == null)
        throw new ArgumentNullException(nameof(paymentMethod));

    // Business rule: orders over $10,000 require manager approval
    if (order.Total > 10000 && !order.HasManagerApproval)
        return false;

    return mPaymentGateway.Charge(paymentMethod, order.Total);
}
```

## Namespace & Using Directives

- **File-scoped namespaces** for single-namespace files: `namespace MyApp.Features;`
- **Place `using` directives outside namespace** to avoid ambiguity and context-sensitive resolution
- Avoid `global::` modifier by placing using directives correctly

```csharp
using System;
using System.Collections.Generic;

namespace MyCompany.OrderProcessing;

public class OrderService
{
    // Implementation...
}
```

## Complete Example

```csharp
using System;
using System.Collections.Generic;

namespace MyCompany.OrderProcessing;

/// <summary>
/// Validates orders before processing.
/// </summary>
public class OrderValidator : ABaseValidator, IOrderValidator
{
    private static readonly int gMaxOrderItems = 100;
    private readonly IInventoryChecker mInventoryChecker;
    private readonly IPriceCalculator mPriceCalculator;

    /// <summary>
    /// Initializes a new instance of the OrderValidator class.
    /// </summary>
    /// <param name="inventoryChecker">The checker used to verify item availability.</param>
    /// <param name="priceCalculator">The calculator used to compute order prices.</param>
    /// <exception cref="ArgumentNullException">Thrown when inventoryChecker or priceCalculator is null.</exception>
    public OrderValidator(IInventoryChecker inventoryChecker, IPriceCalculator priceCalculator)
    {
        if (inventoryChecker == null)
            throw new ArgumentNullException(nameof(inventoryChecker));

        if (priceCalculator == null)
            throw new ArgumentNullException(nameof(priceCalculator));

        mInventoryChecker = inventoryChecker;
        mPriceCalculator = priceCalculator;
    }

    /// <summary>
    /// Validates the order and returns validation result.
    /// </summary>
    /// <param name="order">The order to validate.</param>
    /// <returns>A validation result indicating success or failure with details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when order is null.</exception>
    public ValidationResult Validate(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.Items.Count == 0)
            return ValidationResult.Failed("Order must contain at least one item");

        // Business rule: prevent oversized orders to avoid system overload
        if (order.Items.Count > gMaxOrderItems)
            return ValidationResult.Failed($"Order exceeds maximum of {gMaxOrderItems} items");

        List<int> availableItems = mInventoryChecker.CheckAvailability(order.Items);

        // Must check each item individually since partial availability requires different handling
        for (int i = 0; i < order.Items.Count; i++)
        {
            OrderItem item = order.Items[i];
            bool isAvailable = false;

            for (int j = 0; j < availableItems.Count; j++)
            {
                if (availableItems[j] == item.Id)
                {
                    isAvailable = true;
                    break;
                }
            }

            if (!isAvailable)
                return ValidationResult.Failed($"Item {item.Id} is not available");
        }

        return ValidationResult.Success();
    }
}
```

## References

- [Microsoft C# Identifier Naming Rules](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [Microsoft .NET Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Runtime Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)