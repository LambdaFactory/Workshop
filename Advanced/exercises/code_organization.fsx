#load "examples.fs"
open Examples
(*********************************************************************************************************************)

// A newcomer to F# might be tempted to organize code in classes just like in C#.
// One class per file, in alphabetical order. After all, F# supports the same object-oriented features that C# does, right?
// So surely the F# code can be organized the same way as C# code?

// After a while, this is often followed by the discovery that F# requires files (and code within a file)
// to be in dependency order. That is, you cannot use forward references to code that hasn’t been seen by the compiler yet
// This is followed by general annoyance and swearing. How can F# be so stupid?
// Surely it impossible to write any kind of large project!

// The other important difference between OO and FP design is that we sepearate types and behaviour.
// The data types are simple and “dumb”. And then separately, you have a number of functions that act on those data types.
// This is the exact opposite of an object-oriented design, where behavior and data are meant to be combined.
// After all, that’s exactly what a class is. In a truly object-oriented design in fact,
// you should have nothing but behavior – the data is private and can only be accessed via methods.

// In fact, in OOD, not having enough behavior around a data type is considered a Bad Thing, and even has a name: the “anemic domain model”.
// In functional design though, having “dumb data” with transparency is preferred.
// It is normally fine for the data to be exposed without being encapsulated.
// The data is immutable, so it can’t get “damaged” by a misbehaving function.
// And it turns out that the focus on transparent data allows for more code that is more flexible and generic.

(*********************************************************************************************************************)

//Let's revisit some of the functionalites from previous tasks but now devided into nice layers


/// ===========================================
/// Common types and functions shared across multiple projects
/// ===========================================
module CommonLibrary =

    // the two-track type
    type Result<'TSuccess,'TFailure> =
        | Success of 'TSuccess
        | Failure of 'TFailure

    // convert a single value into a two-track result
    let succeed x =
        Success x

    // convert a single value into a two-track result
    let fail x =
        Failure x

    // appy either a success function or failure function
    let either successFunc failureFunc twoTrackInput =
        match twoTrackInput with
        | Success s -> successFunc s
        | Failure f -> failureFunc f


    // convert a switch function into a two-track function
    let bind f =
        either f fail

    // pipe a two-track value into a switch function
    let (>>=) x f =
        bind f x

    // compose two switches into another switch
    let (>=>) s1 s2 =
        s1 >> bind s2

    // convert a one-track function into a switch
    let switch f =
        f >> succeed

    // convert a one-track function into a two-track function
    let map f =
        either (f >> succeed) fail

    // convert a dead-end function into a one-track function
    let tee f x =
        f x; x

    // convert a one-track function into a switch with exception handling
    let tryCatch f exnHandler x =
        try
            f x |> succeed
        with
        | ex -> exnHandler ex |> fail

    // convert two one-track functions into a two-track function
    let doubleMap successFunc failureFunc =
        either (successFunc >> succeed) (failureFunc >> fail)

    // add two switches in parallel
    let plus addSuccess addFailure switch1 switch2 x =
        match (switch1 x),(switch2 x) with
        | Success s1,Success s2 -> Success (addSuccess s1 s2)
        | Failure f1,Success _  -> Failure f1
        | Success _ ,Failure f2 -> Failure f2
        | Failure f1,Failure f2 -> Failure (addFailure f1 f2)


/// ===========================================
/// Global types for this project
/// ===========================================
module DomainTypes =

    open CommonLibrary

    /// The DTO for the request
    type Request = {name:string; email:string}

    // Many more types coming soon!

/// ===========================================
/// Logging functions
/// ===========================================
module Logger =

    open CommonLibrary
    open DomainTypes

    let log twoTrackInput =
        let success x = printfn "DEBUG. Success so far: %A" x; x
        let failure x = printfn "ERROR. %A" x; x
        doubleMap success failure twoTrackInput

/// ===========================================
/// Validation functions
/// ===========================================
module Validation =

    open CommonLibrary
    open DomainTypes

    let validate1 input =
       if input.name = "" then Failure "Name must not be blank"
       else Success input

    let validate2 input =
       if input.name.Length > 50 then Failure "Name must not be longer than 50 chars"
       else Success input

    let validate3 input =
       if input.email = "" then Failure "Email must not be blank"
       else Success input

    // create a "plus" function for validation functions
    let (&&&) v1 v2 =
        let addSuccess r1 r2 = r1 // return first
        let addFailure s1 s2 = s1 + "; " + s2  // concat
        plus addSuccess addFailure v1 v2

    let combinedValidation =
        validate1
        &&& validate2
        &&& validate3

    let canonicalizeEmail input =
       { input with email = input.email.Trim().ToLower() }

/// ===========================================
/// Database functions
/// ===========================================
module CustomerRepository =

    open CommonLibrary
    open DomainTypes

    let updateDatabase input =
       ()   // dummy dead-end function for now

    // new function to handle exceptions
    let updateDatebaseStep =
        tryCatch (tee updateDatabase) (fun ex -> ex.Message)

/// ===========================================
/// All the use cases or services in one place
/// ===========================================
module UseCases =

    open CommonLibrary
    open DomainTypes

    let handleUpdateRequest =
        Validation.combinedValidation
        >> map Validation.canonicalizeEmail
        >> bind CustomerRepository.updateDatebaseStep
        >> Logger.log