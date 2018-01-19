#load "examples.fs"
open System

(* F# supports composable async blocks
   they are superficially similar
   to the async and await capabilities of
   C# but more flexible about when and how they
   are run *)
open System.IO
open System.Net


//Creating Async blogs is done with `async {...}` syntax.
let sleepWorkflow  = async {
    printfn "Starting sleep workflow at %O" DateTime.Now.TimeOfDay
    do! Async.Sleep 2000
    printfn "Finished sleep workflow at %O" DateTime.Now.TimeOfDay
    }

Async.RunSynchronously sleepWorkflow

//Workflows can contain other async workflows nested inside them.
//Within the braces, the nested workflows can be blocked on by using the let! syntax.

let nestedWorkflow  = async{

    printfn "Starting parent"
    let! childWorkflow = Async.StartChild sleepWorkflow

    // give the child a chance and then keep working
    do! Async.Sleep 100
    printfn "Doing something useful while waiting "

    // block on the child
    let! result = childWorkflow

    // done
    printfn "Finished parent"
    }

// run the whole workflow
Async.RunSynchronously nestedWorkflow

//One very convenient thing about async workflows is that they support a built-in cancellation mechanism. No special code is needed.

let testLoop = async {
    for i in [1..100] do
        // do something
        printf "%i before.." i

        // sleep a bit
        do! Async.Sleep 10
        printfn "..after"
    }

open System
open System.Threading

// create a cancellation source
let cancellationSource = new CancellationTokenSource()

// start the task, but this time pass in a cancellation token
Async.Start (testLoop,cancellationSource.Token)

// wait a bit
Thread.Sleep(200)

// cancel after 200ms
cancellationSource.Cancel()




// This function will be useful in many of our later examples. It downloads the content of a url asynchronously
//It also demonstrates that async code can be composed sequentially
let fetchAsync (url : string) = async {
    let req = WebRequest.Create(url)
    let! resp = req.AsyncGetResponse()
    let stream = resp.GetResponseStream()
    let reader = new StreamReader(stream)
    let! content = reader.ReadToEndAsync() |> Async.AwaitTask
    return content
}

let fetchGoogle () = async {
  let! content = fetchAsync "http://www.google.com.au"
  return content
}

// the above function was written to demonstrate
// this use of an async block. The function below is
// exactly the same (ie we didn't need the block in this case):
let fetchGoogle2 () = fetchAsync "http://www.google.com.au"

Examples.test "Can get google homepage" (fun () ->
    let content = fetchGoogle () |> Async.RunSynchronously
    content.Contains("Google")
)

let testPages = [
  ("Google", "http://www.google.com.au")
  ("Facebook", "http://facebook.com")
  ("GitHub", "http://github.com")
]

// start by retrieving the pages sequentially.
// can you use Async.Parallel to retrieve them in parallel?
let fetchAllPages (pages : list<(string * string)>) : Async<list<(string * string)>> =
    failwith "todo"

Examples.test "Can get all pages" (fun () ->
    let result =
        testPages
        |> fetchAllPages
        |> Async.RunSynchronously

    let checkContainsString state (site, _) =
        match state with
        | false -> false
        | true ->
            match result |> List.tryFind (fun (site',_) -> site' = site) with
            | Some (_, content) ->
                content.Contains (site)
            | None -> false

    testPages
    |> List.fold checkContainsString true
)