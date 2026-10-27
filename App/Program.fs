open Microsoft.AspNetCore.Builder
open Falco
open Falco.Routing
open CloudLab.App.ConfigDataBase.ConfigDataBase
open Prometheus
open CloudLab.App.Handlers.Todos

[<EntryPoint>]
let main args =

    initDB ()

    let builder: WebApplicationBuilder = WebApplication.CreateBuilder args

    let app: WebApplication = builder.Build()

    app.UseRouting() |> ignore

    HttpMetricsMiddlewareExtensions.UseHttpMetrics app |> ignore
    MetricServerMiddlewareExtensions.MapMetrics app |> ignore

    app.Use(fun ctx next -> metricsMiddleware(next).Invoke ctx) |> ignore

    app.UseFalco
        [
          get "/" indexHandler
          get "/api/todos" getTodosHandler
          post "/api/todos" createTodoHandler
          put "/api/todos/{id:int}" (fun ctx ->
              let route: RequestData = Request.getRoute ctx
              let id: int = route.GetInt "id"
              toggleTodoHandler id ctx
          )
        ]
    |> ignore

    app.Run "http://0.0.0"

    0
