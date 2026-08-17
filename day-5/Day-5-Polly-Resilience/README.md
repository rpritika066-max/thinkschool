# Day 5 — Polly Resilience

## Exercise
Add Polly-based resilience to outbound HTTP calls using `Microsoft.Extensions.Http.Resilience`.

### Resilience configuration
- Named `HttpClient`: `my-service`
- 3 retry attempts
- Exponential backoff with jitter
- Retry logging through `OnRetry`
- Circuit breaker: 50% failure ratio over a 30-second sampling window
- Timeout: 10 seconds
- Transient test endpoint intentionally returns HTTP 503

### Validation
- `/health` returned HTTP 200 and `Healthy`.
- `/test/transient-failure` returned HTTP 503.
- `/test/resilience` exercised the configured resilience pipeline.
- Azure Container Apps console telemetry recorded the intentional HTTP 503 and `/test/transient-failure` route.

## Evidence
See `evidence/polly-resilience-evidence.png` for the captured Azure test evidence.

> Note: the captured Container Apps console stream did not expose the explicit `POLLY RETRY` text, so the evidence is described only as far as the captured logs support.
