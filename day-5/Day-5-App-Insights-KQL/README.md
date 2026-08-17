# Day 5 — Verify in App Insights with your first KQL

## Exercise

The deployed QuotesApi was instrumented with OpenTelemetry and Azure Monitor and verified in Application Insights.

### KQL query

```kusto
requests
| where timestamp > ago(30m)
| summarize count(), p50=percentile(duration, 50), p99=percentile(duration, 99) by name
| order by p99 desc
```

### Result

| Endpoint | Count | p50 | p99 |
|---|---:|---:|---:|
| `GET /api/quotes/` | 23 | 2.549 | 499.9605 |
| `GET /health` | 10 | 0.4416 | 132.8278 |
| `GET /api/quotes/{id:int}` | 10 | 2.8025 | 26.3017 |

### Observation

`GET /api/quotes/` surprised me because it had the highest p99 latency at approximately 500 ms, making it the slowest endpoint in the captured telemetry.

### Screenshot

The required Application Insights KQL result screenshot should be uploaded to this folder as `app-insights-kql-result.png`.
