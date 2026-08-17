# Day 5 — Diagnose a Slow Endpoint Using Traces

## Deliverable Overview
We introduced an intentional delay into the `GET /api/quotes` endpoint, instrumented the ASP.NET Core Minimal API with **OpenTelemetry** + **Structured Logging**, captured the before/after traces, diagnosed the root cause, and verified the fix.

---

## 1. Before vs After Traces & Metrics

### Before Fix (Slow Trace)
```text
Activity.TraceId:            4cb2f29bb6bbe4b1761ef34546b209bb
Activity.SpanId:             0b1096713c15e107
Activity.DisplayName:        GET /api/quotes/
Activity.Kind:               Server
Activity.StartTime:          2026-08-14T05:29:27.2596734Z
Activity.Duration:           00:00:01.6219892  (1,621 ms / 1.62s)
Activity.Tags:
    http.request.method: GET
    server.address: localhost
    server.port: 5000
    url.path: /api/quotes
    http.response.status_code: 200
```
- **Terminal HTTP Benchmark**: `1.624s` total latency.
- **Log Correlation**:
  - `05:29:27.267Z`: `LogRecord` -> `Getting quotes page 1 with size 10`
  - `05:29:28.880Z`: `LogRecord` -> `Executed DbCommand (0ms)`
  - **Span Gap**: ~`1,613ms` delay between endpoint entry log and EF Core execution log.

---

### After Fix (Optimized Trace)
```text
Activity.TraceId:            c8ffa38d601e12fb131e32ad6fd3b1bb
Activity.SpanId:             281f971e84658c28
Activity.DisplayName:        GET /api/quotes/
Activity.Kind:               Server
Activity.StartTime:          2026-08-14T05:30:05.8564899Z
Activity.Duration:           00:00:00.0106554  (10.65 ms)
Activity.Tags:
    http.request.method: GET
    server.address: localhost
    server.port: 5000
    url.path: /api/quotes
    http.response.status_code: 200
```
- **Terminal HTTP Benchmark**: `0.012s` (12.3 ms) total latency.
- **Speedup**: **~131x performance enhancement!**

---

## 2. 100-Word Diagnosis Note

> **Diagnosis Note:**
> This trace (`4cb2f29bb6bbe4b1761ef34546b209bb`) showed the slow span was `GET /api/quotes/` taking `1,621 ms` because of an intentional `Thread.Sleep(1500)` call injected directly into the Minimal API request handler before calling the repository layer (`repository.GetPagedAsync`). OpenTelemetry span timestamps showed a 1.61-second latency gap between the endpoint entry log (`Getting quotes page 1`) and the EF Core SQL execution log (`Executed DbCommand (0ms)`), proving the database itself was fast (0ms execution time). I fixed it by removing the synchronous thread sleep block, restoring endpoint latency to a crisp 10.6ms.

---

## 3. The Fix Commit (`d05d9c3`)

```diff
[QuoteEndpointExtensions.cs]
@@ -46,7 +46,6 @@
                 currentPage,
                 pageSize);

-            System.Threading.Thread.Sleep(1500);

             var quotes = await repository.GetPagedAsync(
                 currentPage,
```

---

## 4. Bonus: KQL Query for Azure App Insights

To detect similar slow endpoints or N+1 query bottlenecks in production via **Azure Application Insights**, run the following KQL query in Azure Log Analytics:

```kql
// Find endpoints where request duration exceeds 1.5 seconds 
// and evaluate associated SQL dependency calls (detecting N+1 or thread blocks)
requests
| where timestamp > ago(24h)
| where duration > 1500 // Threshold: requests taking > 1.5s
| project operation_Id, name, duration, resultCode, timestamp
| join kind=leftouter (
    dependencies
    | where type == "SQL" or type == "SQLite"
    | summarize DbCallCount = count(), TotalDbDuration = sum(duration) by operation_Id
) on operation_Id
| project 
    Timestamp = timestamp,
    Endpoint = name,
    TotalDuration_ms = duration,
    DbCallCount = coalesce(DbCallCount, 0),
    TotalDbDuration_ms = coalesce(TotalDbDuration_ms, 0),
    AppCodeDuration_ms = duration - coalesce(TotalDbDuration_ms, 0),
    ResultCode = resultCode
| sort by TotalDuration_ms desc
```

### Explanation of KQL Insights:
1. **`AppCodeDuration_ms > 1000` & `DbCallCount <= 1`**: Identifies un-async / thread sleep blocking code within C# application handlers.
2. **`DbCallCount > 50` & `TotalDbDuration_ms` high**: Detects EF Core **N+1 query patterns** where an endpoint triggers repeated SQL queries inside a loop.
