# Observation

The resilience test successfully triggered the intentional HTTP 503 transient failure, confirming that the configured resilience pipeline is being exercised.

The captured Azure Container Apps logs show `http.response.status_code: 503` for `GET /test/transient-failure`.

The required evidence screenshot is included in this exercise folder.