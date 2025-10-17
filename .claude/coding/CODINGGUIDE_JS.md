# JavaScript and TypeScript generic coding guide

## Size and complexity

  * No files must be longer than 1000 lines of code.
  * No function or method must be longer than 50 lines of code.
  * Cyclomatic complexity of any method must not exceed 15.

## Names
  * Use PascalCase for type names.
  * Do not use I as a prefix for interface names.
  * Use PascalCase for enum values.
  * Use camelCase for function names.
  * Use camelCase for property names and local variables.
  * Do not use _ as a prefix for private properties.
  * Use whole words in names when possible.

## Components
  * One file per logical component (e.g. parser, scanner, emitter, checker).

## Types
  * Do not export types/functions unless you need to share it across multiple components.
  * Do not introduce new types/values to the global namespace.
  * Shared types should be defined in types.ts.
  * Within a file, type definitions should come first.

## null and undefined
  * Use undefined. Do not use null.

## General Assumptions
  * Consider objects like Nodes, Symbols, etc. as immutable outside the component that created them. Do not change them.
  * Consider arrays as immutable by default after creation.

## Classes
  * For consistency, do not use classes in the core compiler pipeline. Use function closures instead.

## Flags
  * More than 2 related Boolean properties on a type should be turned into a flag.

## Comments
  * Use JSDoc style comments for functions, interfaces, enums, and classes.

## Strings
  * Use double quotes for strings.
  * All strings visible to the user need to be localized (make an entry in diagnosticMessages.json).

## Style
  * Use arrow functions over anonymous function expressions.
  * Only surround arrow function parameters when necessary.
     * For example, `(x) => x + x` is wrong but the following are correct:
       * `x => x + x`
       * `(x, y) => x + y`
       * `<T>(x: T, y: T) => x === y`
  * Always surround loop and conditional bodies with curly braces. Statements on the same line are allowed to omit braces.
  * Open curly braces always go on the same line as whatever necessitates them.
  * Parenthesized constructs should have no surrounding whitespace.
  * A single space follows commas, colons, and semicolons in those constructs. For example:
       * `for (var i = 0, n = str.length; i < 10; i++) { }`
       * `if (x < 10) { }`
       * `function f(x: number, y: string): void { }`
 * Use a single declaration per variable statement (i.e. use `var x = 1; var y = 2; over var x = 1, y = 2;`).
 * `else` goes on a separate line from the closing curly brace.
 * Use 4 spaces per indentation.

## Front-End JavaScript specific

 * Pure Javascript only must be used on the client side.
 * Frond-end JavaScript file names must be in lower case.
 * Scripts specific for a particular HTML page must be in a separate file and the name of this file must start with the name of the page.

## Back-End JavaScript and TypeScript specific

 * Both, JavaScript and TypeScript (preferred) can be used on the back-end
 * Back-end JavaScript file names must be in upper case.
