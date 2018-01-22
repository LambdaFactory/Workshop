module Examples
  let test name f =
    try
      match f () with
      | true -> printfn "%s: passed" name
      | false -> printfn "%s: failed" name
    with
      | e when e.Message = "todo" ->
        printfn "%s: TODO" name
      | e ->
        printfn "%s: Exception %A" name e

// a utility function
  type Utility() =
    static let rand = new System.Random()

    static member RandomSleep() =
        let ms = rand.Next(1,10)
        System.Threading.Thread.Sleep ms

  let slowConsoleWrite msg =
    msg |> String.iter (fun ch->
        System.Threading.Thread.Sleep(1)
        System.Console.Write ch
        )