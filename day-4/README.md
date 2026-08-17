# Day 4 - Wire CI with GitHub Actions

## Objective

Set up continuous integration using GitHub Actions so that every push and every pull request targeting `main` is automatically built and tested.

The CI pipeline is defined in:

`.github/workflows/ci.yml`

## CI Pipeline

The workflow:

1. Checks out the repository.
2. Sets up .NET 10.
3. Restores project dependencies.
4. Builds the application.
5. Builds all test projects.
6. Runs the test suites.
7. Collects TRX test results.
8. Collects XPlat Code Coverage results.
9. Generates a combined coverage report.
10. Fails the job if line coverage is below 70%.
11. Uploads test results and coverage as GitHub Actions artifacts.

## Trigger Rules

CI runs on:

- Push to any branch.
- Pull requests targeting `main`.

## Test Projects

The pipeline runs:

- `Quotes.Tests.Unit`
- `Quotes.Tests.Integration`
- `QuotesApi.Tests`
- `Tests.Domain`

## Coverage

The local combined coverage report produced:

- Line coverage: **89.1%**
- Covered lines: **998**
- Uncovered lines: **122**
- Coverable lines: **1120**
- Branch coverage: **76.6%**
- Method coverage: **87%**

The required minimum line coverage is **70%**.

Therefore:

**89.1% >= 70% - coverage requirement passes.**

## Test Isolation

The integration/API test environment uses an isolated SQLite in-memory database through `CustomWebApplicationFactory`.

This prevents tests from depending on an existing local `quotes.db` file and makes the test environment more suitable for CI.

## What did I learn this session?

I learned how to wire a .NET project into GitHub Actions and make the build and test process reproducible in CI. I also learned how to collect test results and code coverage and enforce a minimum coverage threshold so that CI can reject changes that reduce coverage below the required level.

## What would break this?

The CI pipeline would fail if:

- The project no longer builds.
- Any test fails.
- A required dependency cannot be restored.
- A test project is missing or incorrectly configured.
- The coverage report cannot be generated.
- Line coverage falls below 70%.
- The GitHub Actions workflow contains invalid YAML or incorrect paths.

## Evidence

The CI workflow is located at:

`.github/workflows/ci.yml`

The GitHub Actions run and pull request will provide the final green-CI evidence for this task.
