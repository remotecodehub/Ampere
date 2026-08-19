# Skill: dotnet-ddd

## Use when

Use for .NET application changes in
`src/app` and `src/web`.

## Rules

Use .NET 10.0.
Follow current .NET 10 best practices.
Preserve the single-project DDD model.
Organize Application by feature.
Organize Domain by feature.
Organize Infrastructure by feature.
Use Composition for dependency injection.

Respect dependency direction:
Presentation -> Application -> Domain.
Infrastructure implements boundaries and
is composed by the application root.

Keep one type per file.
Use file-scoped namespaces.
Use primary constructors.
Do not use dynamic.
Do not use implicit local typing.
Do not inject types through reflection.
Use explicit, compile-time service wiring.
Use async and await where applicable.
Propagate cancellation tokens.

Code must not introduce compiler or
analyzer warnings.
Do not suppress diagnostics to hide defects.

## Avoid

Do not move business rules into UI.
Do not couple Domain to controllers.
Do not add projects merely for symmetry.
Do not introduce infrastructure into Domain.
Do not use reflection to discover services.

## Validation

Build affected .NET projects.
Run relevant tests when available.
The affected build must be warning-free.
