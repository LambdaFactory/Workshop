namespace Life.Server

open System.IO
open Newtonsoft.Json
open Suave

module JSON =

    let deserialize<'a> json =
        JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

    let serialize (data:'t) : string =
        JsonConvert.SerializeObject(data)