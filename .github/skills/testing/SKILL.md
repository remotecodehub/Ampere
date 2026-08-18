# Skill: testing

## Use when

Use when creating or changing tests.

## Rules

Tests follow the same area split:

- `tests/app`
- `tests/iot`
- `tests/os`
- `tests/web`

Test behavior, not implementation details.
Keep tests isolated and deterministic.

Async tests must await operations.
Propagate cancellation when the API exposes
it.

Do not weaken production code merely to make
a test pass.

When a failure suggests an implementation
bug, verify the real production behavior
before changing the test.
