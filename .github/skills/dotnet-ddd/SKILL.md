# Skill: dotnet-ddd

## Use when

Use for .NET application changes in
`src/app` and `src/web`.

## Rules

Use .NET 10.0.
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
Use async and await where applicable.
Propagate cancellation tokens.

## Avoid

Do not move business rules into UI.
Do not couple Domain to controllers.
Do not add projects merely for symmetry.
Do not introduce infrastructure into Domain.

## Validation

Build affected .NET projects.
Run relevant tests when available.
