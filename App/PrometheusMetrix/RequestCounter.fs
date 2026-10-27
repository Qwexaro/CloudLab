namespace CloudLab.App.PrometheusMetrix

open Prometheus

module RequestCounter =

    let requestCounter: Counter =

        Metrics.CreateCounter("app_requests_total", "Total requests", "method", "endpoint")
