namespace Life.Server

open System.Text

open Suave
open Suave.Successful

[<AutoOpen>]
module WebHelpers =
    let toHtml (markup : seq<string>) =
        let sb = System.Text.StringBuilder()

        markup
        |> Seq.fold (fun (builder : StringBuilder) line -> builder.Append(line)) sb
        |> (fun x -> x.ToString())
        |> OK
