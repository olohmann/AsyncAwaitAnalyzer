# AsyncAwaitAnalyzer
A set of Roslyn Diagnostic Analyzers and Code Fixes for Async/Await Programming in C#.

## Diagnostics

* [ASYNC-0001] Check async void signatures. This diagnostic verifies that async void method declarations are only used on event handlers.
* [ASYNC-0002] Check async method naming style. This diagnostic enforces a 'Async' suffix for methods doing async work.
* [ASYNC-0003] Check usage of ConfigureAwait. This diagnostic verifies that CongfigureAwait(false) is used in library code.

## Code Fixes
* Fix for [ASYNC-0002].