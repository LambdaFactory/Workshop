namespace Life.Server
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Files
open Suave.Successful
open Suave.RequestErrors

module Main =
    let getResourceFromReq<'a> (req : HttpRequest) =
        let getString rawForm =
            System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> JSON.deserialize<'a>



    [<EntryPoint>]
    let main argv =
        printfn "Current directory: %s"  <| System.IO.Directory.GetCurrentDirectory()
        let startState = { ServerState.Patterns = PatternSamples.all }
        let dataStore = DataStore.create startState
        choose [
            GET
                >=> path "/"
                >=> (fun ctx -> async {
                        let! state = dataStore |> DataStore.getState
                        let content = Page.render state
                        return! toHtml content ctx
                    })
            GET >=> pathScan "/static/%s" (fun s -> (file <| sprintf "./static/%s" s))
            POST
                >=> path "/getNext"
                >=> request(fun r ->
                      let (board : BoardState) = getResourceFromReq r
                      let grid = List.toArray2D false board.grid
                      let result = Game.computeNext grid
                      JSON.serialize result |> Successful.OK )
            PUT
                // todo save the pattern
                >=> pathScan "/pattern/%s" (fun s -> OK "ok")
            GET
                // todo load the pattern
                >=> pathScan "/pattern/%s" (fun s -> OK "ok")
            NOT_FOUND "Not found"
        ]
        |> startWebServer defaultConfig

        0 // return an integer exit code