#load "examples.fs"
open Examples
(*********************************************************************************************************************)

// As mentioned before functional programming is based on the idea of pure functions - functions that take input, and always return some output.
// In C# way too often we are using throwing exceptions as mechanism of control flow - even if situation is not really exceptional
// but is just result of simple "bad path" in our business logic, for example validation failure.

// Instead of using throwing exception for our business logic we can use F# domain to model output type that can represent
// both success and failure.

type Result<'TSuccess,'TFailure> =
    | Success of 'TSuccess
    | Failure of 'TFailure

// Here is how we would use it in practice:

type Request = {name:string; email:string}

let validateInput input =
   if input.name = "" then Failure "Name must not be blank"
   else if input.email = "" then Failure "Email must not be blank"
   else Success input  // happy path

// Very often in our applications the use case can be modeled as set of steps - small seperate functions.
// Let's take as an example web service updating email address for user with given name.

// The typical workflow would look like that:
// RecieveRequest -> ValidateRequest -> ReadExistingUserRecord -> UpdateExistingUserRecord
// -> SendVerificationEmailIfChanged -> ShowResult.

//However this workflow describes only "happy path" - each step can fail.
//For example validation can failed, user may not exist in DB, or sending can fail due to SMTP error
//Using method presented above we can modify all our functions to return Result type.

let validateName input =
   if input.name = "" then Failure "Name must not be blank"
   else Success input

let validateEmail input =
   if input.email = "" then Failure "Email must not be blank"
   else Success input

let readExistingUser (request: Request) : Result<int * Request, string> =
    if request.name = "failOnExistCheck" then
        Failure "Record not existing"
    else
        Success (42, request)

let updateExistingUserRecord (id: int, request: Request) : Result<Request, string> =
    if request.name = "failOnUpdate" then
        Failure "Update failed"
    else

        Success request

let sendEmail (request: Request) : Result<Request, string> =
    if request.name = "failOnEmail" then
        Failure "SMTP error"
    else
        Success request

let showResult (input: Result<Request, string>) =
    match input with
    | Success _ -> "OK"
    | Failure errorMsg -> errorMsg

// TODO: Create one function that composes all above functions together.

let useCase (request: Request) : string =
    failwith "TODO"

test "Everything fine" (fun _ ->
    let t = {name = "Chris"; email = "abc@abc.com"}
    useCase t = "OK"
)

test "Validation can fail" (fun _ ->
    let t = {name = ""; email = "abc@abc.com"}
    useCase t = "Name must not be blank"
)

test "Validation can fail 2" (fun _ ->
    let t = {name = "abc"; email = ""}
    useCase t = "Email must not be blank"
)

test "Validation can fail 3" (fun _ ->
    let t = {name = ""; email = ""}
    useCase t = "Name must not be blank;Email must not be blank"
)

test "Exist Check can fail" (fun _ ->
    let t = {name = "failOnExistCheck"; email = "abc@abc.com"}
    useCase t = "Record not existing"
)

test "Update can fail" (fun _ ->
    let t = {name = "failOnUpdate"; email = "abc@abc.com"}
    useCase t = "Update failed"
)

test "Send email can fail" (fun _ ->
    let t = {name = "failOnEmail"; email = "abc@abc.com"}
    useCase t = "SMTP error"
)

// As you can see the composing those functions together is not super nice.
// But it can be solved with set of helper functions

// In the description below "switch function" means function
// that takes "normal" value as an input and returns Result.
// The "result function" means function that takes Result and returns Result.

// convert a single value into a result
let succeed (x :'a) : Result<'a,_> =
    failwith "TODO"

// convert a single value into a result
let fail (x: 'a) : Result<_, 'a> =
    failwith "TODO"

// apply either a success function or failure function
let either (successFunc: 'a -> 'b) (failureFunc: 'c -> 'b) (twoTrackInput : Result<'a, 'c>) : 'b =
    failwith "TODO"

// convert a switch function into a result function
let bind (f : 'a -> Result<'b,'c> ) : (Result<'a,'c> -> Result<'b, 'c>) =
    failwith "TODO"

// pipe a result value into a switch function
let (>>=) (x : Result<'a,'b>) (f : 'a -> Result<'c,'b>) : Result<'c,'b> =
    failwith "TODO"

// compose two switches into another switch
let (>=>) (s1: 'a -> Result<'b,'c>) (s2 : 'b -> Result<'d,'c>) : ('a -> Result<'d,'c>) =
    failwith "TODO"

// convert a normal function into a switch
let switch (f: 'a -> 'b) : ('a -> Result<'b,'c>)=
    failwith "TODO"

// convert a normal function into a two-track function
let map (f: 'a -> 'b) : (Result<'a,'c> -> Result<'b,'c>)=
    failwith "TODO"

// convert a side-effect function into a normal function
let tee (f: 'a -> unit) (x: 'a) : 'a =
    failwith "TODO"

// convert a normal function into a switch with exception handling
let tryCatch (f: 'a -> 'b) (exnHandler: exn -> 'c) (x: 'a) : Result<'b,'c> =
    failwith "TODO"

// add two switches in parallel
let plus (addSuccess: 'a -> 'b -> 'c) (addFailure: 'd -> 'd -> 'd) (switch1: 'e -> Result<'a,'d>) (switch2: 'e -> Result<'b,'d>) (x: 'e) : Result<'c,'d> =
    failwith "TODO"



// TODO: Create useCase handler, this time using helper functions we've created above.

let useCase (request: Request) : string =
    failwith "TODO"

test "Everything fine" (fun _ ->
    let t = {name = "Chris"; email = "abc@abc.com"}
    useCase t = "OK"
)

test "Validation can fail" (fun _ ->
    let t = {name = ""; email = "abc@abc.com"}
    useCase t = "Name must not be blank"
)

test "Validation can fail 2" (fun _ ->
    let t = {name = "abc"; email = ""}
    useCase t = "Email must not be blank"
)

test "Validation can fail 3" (fun _ ->
    let t = {name = ""; email = ""}
    useCase t = "Name must not be blank;Email must not be blank"
)

test "Exist Check can fail" (fun _ ->
    let t = {name = "failOnExistCheck"; email = "abc@abc.com"}
    useCase t = "Record not existing"
)

test "Update can fail" (fun _ ->
    let t = {name = "failOnUpdate"; email = "abc@abc.com"}
    useCase t = "Update failed"
)

test "Send email can fail" (fun _ ->
    let t = {name = "failOnEmail"; email = "abc@abc.com"}
    useCase t = "SMTP error"