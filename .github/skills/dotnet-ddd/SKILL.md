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

Feature handlers invoke feature services.
Feature services may inject application
abstractions such as repositories and unit
of work.

Feature services must not inject DbContext.
Feature services must not access DbSet<T>.
Feature services must not reference EF Core.
Feature services must not access SQL directly.

Use IRepository<TEntity> for persistence.
Use IUnitOfWork for persistence coordination.
Repositories must not call SaveChangesAsync.
Use ITransactionalRequest for state-changing
Mediator requests that require transactions.

Keep one type per file.
Use file-scoped namespaces.
Use primary constructors.
Do not use dynamic.
Do not use implicit local typing.
Do not inject types through reflection.
Use explicit, compile-time service wiring.
Use async and await where applicable.
Propagate cancellation tokens.

Persisted entities implement IEntityBase.
Domain entities should inherit EntityBase.

Code must not introduce compiler or
analyzer warnings.
Do not suppress diagnostics to hide defects.

## Avoid

Do not move business rules into UI.
Do not couple Domain to controllers.
Do not add projects merely for symmetry.
Do not introduce infrastructure into Domain.
Do not use reflection to discover services.
Do not make services access DbContext.
Do not make repositories commit changes.

## Validation

Build affected .NET projects.
Run relevant tests when available.
The affected build must be warning-free.
