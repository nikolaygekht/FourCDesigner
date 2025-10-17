# TESTING STRATEGY

This document defines a test strategy based on a **true test pyramid**: most tests are unit-level, but **all tests across levels reflect real-world usage scenarios**, so we can trust lower-level tests when higher-level ones fail.

## Test Pyramid Structure

 ▲ End-to-End (UI + API)
 │ Real user interaction flow
 │
 ├─────────────
 │ Controller Scenario Tests
 │ Test complete use cases via internal controller API
 │
 ├─────────────
 │ Unit Tests
 │ Test all classes (Entities, Boundaries, Controllers) in isolation
 ▼


## UNIT TESTS (Base of the Pyramid)

### Goal

Test each class (Entity, Boundary, Controller) independently using mocks/stubs. Ensure logic is correct and edge cases are handled.

### What to test

- **Entities:**
  - Data mutation methods (e.g. `.addMessage()`, `.getHistory()`)
  - Invariant rules or validations (if any)

- **Boundaries:**
  - Test interfaces with mocked external services (Claude API, database)
  - Simulate timeouts, failures, and retries

- **Controllers:**
  - Test business logic in isolation using stubbed boundaries/entities

### Recommended Tools
- **Test runner:** xUnit
- **Mocking:** Moq
- **Assertions:** FluentAssertions

### xUnit Best Practices

**IMPORTANT: Use `[Theory]` instead of `[Fact]` for parameterized tests**

When you need to test the same logic with multiple different inputs and expected outputs, always use `[Theory]` with `[InlineData]` or other data sources instead of creating multiple `[Fact]` tests.

```csharp
// ❌ WRONG - Multiple similar Fact tests
[Fact]
public void ValidateUser_WithEmptyEmail_ReturnsNull()
{
    var result = controller.ValidateUser("", "password");
    result.Should().BeNull();
}

[Fact]
public void ValidateUser_WithEmptyPassword_ReturnsNull()
{
    var result = controller.ValidateUser("email@example.com", "");
    result.Should().BeNull();
}

[Fact]
public void ValidateUser_WithNullEmail_ReturnsNull()
{
    var result = controller.ValidateUser(null, "password");
    result.Should().BeNull();
}

// ✅ CORRECT - Single Theory test with multiple cases
[Theory]
[InlineData("", "password")]
[InlineData("email@example.com", "")]
[InlineData(null, "password")]
[InlineData("email@example.com", null)]
[InlineData("   ", "password")]
[InlineData("email@example.com", "   ")]
public void ValidateUser_WithEmptyCredentials_ReturnsNull(string email, string password)
{
    var result = controller.ValidateUser(email, password);
    result.Should().BeNull();
}
```

**Benefits of using Theory:**
- Reduces code duplication
- Makes it easy to add new test cases
- Clearly shows all edge cases in one place
- Better test organization and readability

## CONTROLLER SCENARIO TESTS

### Goal

Verify full logical flow by testing the controller as a unit, using real instances of Entity classes and mocks of external boundaries.

### Characteristics
- Each test maps to a **real user scenario**
- Mock only boundaries (AI API, DB access)
- Validate the state of entities and output results

### Example Scenarios

- A user adds multiple messages, then requests a summary.
- A controller handles a retry after Claude API fails.
- A business rule branches the flow based on input or previous state.

### Implementation
- Group tests by **use case**
- Use spies/mocks to track boundary usage and error handling
- Simulate different Claude responses via prompt stubs

## END-TO-END API TESTS

### Goal:

Simulate full API calls from a REST client, through the Node.js server, interacting with real controller logic and (optionally) a test double or memory-based backend.

### Test Setup

- Use **a separate test server instance** with test config
- Use SQLite as a database and pre-seeded database with the users
- Mock Claude API and database layer via adapters or config flags
- Use predefined sessions or test tokens

### What to test

- All public REST endpoints (e.g. `/api/summarize`, `/api/session`)
- Response structure, status codes, and side effects
- Error handling for timeouts, invalid inputs, broken auth

### Tools

- **Supertest** (recommended) for HTTP integration tests in Node.js
- **Jest/Vitest** as test runners
- **Mock Service Worker (MSW)** to intercept and simulate Claude API responses
- **In-memory DB** SQLite implementation with in-memory preseeded database

## END-TO-END UI + API TESTS (Optional but Valuable)

### Goal:

Test from the user's point of view: click, type, submit → system responds correctly.

### Recommended Stack

- **Frontend:** Assuming HTML/JS UI
- **Tool:** [Playwright](https://playwright.dev/) or [Cypress](https://www.cypress.io/)

### Implementation

- Start the real dev server (`node.js + static front-end`)
- **Mock Service Worker (MSW)** to intercept and simulate Claude API responses
- **In-memory DB** SQLite implementation with in-memory preseeded database
- Use test config to mock Claude/database layers
- Automate flows like:
  - Add message → See message appear
  - Request summary → See loading → Show summary
  - Handle timeouts/errors gracefully in UI

### Tips

- Use **data-test-id** attributes in UI for stable selectors
- Reset session between tests
- Run in headless mode for CI, visible for debugging

## Note on Test Data

- Define test fixtures per scenario
- Use structured mock responses for Claude (store in JSON/YAML)
- Include "bad" test cases: long prompts, invalid formats, service errors

## Summary

| Layer        | Tooling                                        | Purpose                                |
|--------------|------------------------------------------------|----------------------------------------|
| Unit         | xUnit + FluentAssertion + Moq                  | Validate logic of each class           |
| Controller   | xUnit + FluentAssertion + Moq                  | Test real use case flows               |
| API (E2E)    | xUnit + FluentAssestion + Preseeded SQLite     | Validate full backend endpoints        |
| UI + API E2E | xUnit + FluentAssestion + Playwright + Preseeded SQLite | Simulate real user flow                |



