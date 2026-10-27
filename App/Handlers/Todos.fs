namespace CloudLab.App.Handlers

open Falco
open Npgsql
open CloudLab.App.ConfigDataBase
open CloudLab.App.PrometheusMetrix.RequestCounter
open Microsoft.AspNetCore.Http
open CloudLab.App.Todo
open CloudLab.App.CreateTodoRequest

module Todos =

    let getTodosHandler: HttpHandler =

        fun ctx ->

            task {

                use connection: NpgsqlConnection =

                  new NpgsqlConnection(connectionString = ConfigDataBase.getDBConnection ())

                connection.Open()

                use command: NpgsqlCommand = connection.CreateCommand()

                command.CommandText <- "SELECT id, title, done FROM todos ORDER BY id"

                use reader = command.ExecuteReader()

                let todos =
                    [

                      while reader.Read() do

                          yield
                              {| Id = reader.GetInt32 0
                                 Title = reader.GetString 1
                                 Done = reader.GetBoolean 2 |}

                      ]

                return! Response.ofJson todos ctx

            }


    let createTodoHandler: HttpHandler = fun ctx ->

      task {

          let! data: CreateTodoRequest = Request.getJson<CreateTodoRequest> ctx

          use connection: NpgsqlConnection = new NpgsqlConnection(connectionString = ConfigDataBase.getDBConnection())

          connection.Open()

          use command: NpgsqlCommand = connection.CreateCommand()

          command.CommandText <- "INSERT INTO todos (title) VALUES (@title) RETURNING id"

          command.Parameters.AddWithValue("title", data.Title) |> ignore

          let newId: int = command.ExecuteScalar() :?> int

          ctx.Response.StatusCode <- 201

          return! Response.ofJson { Id = newId; Title = data.Title; Done = false } ctx

        }


    let toggleTodoHandler(id: int) : HttpHandler = fun ctx ->

      task {

          use connection: NpgsqlConnection = new NpgsqlConnection(connectionString = ConfigDataBase.getDBConnection())

          connection.Open()

          use command: NpgsqlCommand = connection.CreateCommand()

          command.CommandText <- "UPDATE todos SET done = NOT done WHERE id = @id"

          command.Parameters.AddWithValue("id", id) |> ignore

          command.ExecuteNonQuery() |> ignore

          return! Response.ofJson {| status = "updated" |} ctx

        }

 
    let indexHandler : HttpHandler = fun ctx ->

      Response.ofPlainText "F# Todo API App. Use /api/todos" ctx


    let metricsMiddleware(next: RequestDelegate) =

      RequestDelegate(fun ctx ->

        requestCounter.WithLabels(ctx.Request.Method, ctx.Request.Path.Value).Inc()

        next.Invoke ctx

      )
