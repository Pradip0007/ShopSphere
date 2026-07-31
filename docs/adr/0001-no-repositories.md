# 0001. No Repository Layer — Use DbContext Directly in Handlers

- Status: Accepted
- Date: 2026-07-25

## Context

Traditional layered architectures (Clean, Onion, Hexagonal as commonly
taught) introduce a repository interface between application services
and the persistence layer:

```
Application Handler → IProductRepository → EF DbContext → SQL
```

The stated benefits are:

1. Testability — you can mock `IProductRepository` in unit tests.
2. Persistence-agnostic domain — swap EF for Dapper or Mongo without
   touching handlers.
3. Query encapsulation — hide LINQ noise behind named methods.

ShopSphere uses Vertical Slice Architecture (see ADR-002, forthcoming).
Each feature is one folder with one handler. Handlers are already the
smallest unit of code — adding a repository per aggregate multiplies
files and indirection without a clear payoff.

Additionally, EF Core already provides all three benefits natively:

1. `DbContext` is easy to test — either against SQLite in-memory or via
   integration tests with Testcontainers, which we prefer as it exercises
   real SQL Server semantics (rowversion, indexes, collation).
2. Swapping EF for another ORM is a myth — we have never done it once in
   production across multiple projects. Preparing for it costs real
   design tax now to avoid a hypothetical migration later.
3. Named repository methods (`GetPublishedByCategoryAsync`) drift toward
   god-objects and duplicated query variants. Inline LINQ inside a handler
   is co-located with the use case that needs it, easier to change, and
   easier to delete.

## Decision

Handlers use `ShopSphereDbContext` directly. No `IRepository<T>` or
`IProductRepository` types exist. Aggregate mutation goes through the
aggregate's public API; persistence is `db.SaveChangesAsync()`.

For reads, handlers write LINQ directly against `DbSet`s. Read models
(projections) may be added later as separate files under
`Features/<feature>/ReadModels/` when reuse across handlers emerges.

## Consequences

Positive:

- Zero interfaces to maintain for the common CRUD-adjacent paths.
- One less concept for newcomers to learn.
- Handlers read top-to-bottom without indirection — you see the whole
  use case in one file.
- LINQ noise stays local — a complex query is not "hidden" from the
  reviewer of that feature.

Negative / accepted trade-offs:

- Unit-testing handlers without SQL is harder. We mitigate with
  Testcontainers-backed integration tests (Day 89) and by keeping
  business logic in aggregates (which unit-test trivially).
- If we ever wanted to swap ORM, the surface area of the change is
  larger. We accept this cost given zero historical precedent for
  actually doing so.
- Cross-cutting query concerns (soft delete, tenant filter) rely on
  EF's query filters rather than a shared repository base. Fine —
  query filters are the correct place for them anyway.

## Alternatives considered

1. **Repository per aggregate** (`IProductRepository`, `ICategoryRepository`).
   Rejected — multiplies file count, encourages god-objects, adds an
   indirection with no real testability win over Testcontainers.

2. **Generic repository** (`IRepository<T>`).
   Rejected — a leaky abstraction over `DbSet<T>` that hides EF's
   powerful query composition behind a smaller, worse API.

3. **CQRS with separate read/write stores from day one**.
   Deferred — see the forthcoming ADR on CQRS. Overkill until we have a
   read model that materially diverges from the write model.

## References

- Jimmy Bogard, "Wither the Repository" (2015).
- David Fowler / Damian Edwards, various .NET architecture talks
  advocating vertical slices.
- EF Core team guidance: DbContext is already a Unit of Work +
  Repository — you rarely need another layer on top.