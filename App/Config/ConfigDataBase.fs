namespace CloudLab.App.ConfigDataBase

open System
open Npgsql

module ConfigDataBase =

    let getDBConnection () : string =

        let host: string =

            Environment.GetEnvironmentVariable "DB_HOST"

            |> Option.ofObj

            |> Option.defaultValue "localhost"

        let db: string =

            Environment.GetEnvironmentVariable "DB_NAME"

            |> Option.ofObj

            |> Option.defaultValue "tododb"

        let username: string =

            Environment.GetEnvironmentVariable "DB_USER"

            |> Option.ofObj

            |> Option.defaultValue "todouser"

        let password: string =

            Environment.GetEnvironmentVariable "DB_PASSWORD"

            |> Option.ofObj

            |> Option.defaultValue "todopass"


        let connectionStr: string =

            $"Host={host};DataBase={db};Username={username};Password={password}"

        connectionStr


    let initDB () =

        use connection: NpgsqlConnection =

            new NpgsqlConnection(connectionString = getDBConnection ())

        connection.Open()

        use command: NpgsqlCommand = connection.CreateCommand()

        command.CommandText <-
            "CREATE TABLE IF NOT EXISTS todos (id SERIAL PRIMARY KEY, title VARCHAR(200) NOT NULL, done BOOLEAN DEFAULT FALSE);"

        command.ExecuteNonQuery() |> ignore
