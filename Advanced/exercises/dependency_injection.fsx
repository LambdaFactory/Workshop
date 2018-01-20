#load "examples.fs"
open Examples
(*********************************************************************************************************************)

// One of the most used patterns in C# are dependency injection and inversion of control.
// Those are really important patterns to build loosly coupled, testeable systems.
// In F#, instead of using some magical frameworks, to achive same goal (building testable systems) we use
// simple language features we've already discussed before - mainly partial application, higher order functions and sometimes object expressions.

//If we think about it, simplest possible example of dependency injection in F# are built-in collection functions:

let someImplementation = fun a -> a * a

let differentImplementation = fun a -> a + 1

[1.. 10] |> List.map someImplementation
[1.. 10] |> List.map differentImplementation

// Beacuse functions in F# can be easily passed as values we don't need to wrap them into interfaces.
// Actually in modern SOLID based OO programming we often see interfaces that contains only one function (Single Responsibility Principle)
// In F# we get same behaviour without any additional boilerplate code.

// Another important feature that F# developers use is partial application.
// This language feature is often used to simplify passing higher order functions
// and transform function to have type that's required by other functions.

let mapper (f : int list -> int list) a =
    printfn "Orignal list: %A" a
    let res = f a
    printfn "Result list: %A" a
    res

let mapWithSomeImplementation = List.map someImplementation
let mapWithDifferentImplementation = List.map differentImplementation

([1.. 10] |> mapper mapWithSomeImplementation) = ([1.. 10] |> mapper (List.map someImplementation))
([1.. 10] |> mapper mapWithDifferentImplementation) = ([1.. 10] |> mapper (List.map differentImplementation))

//Partial application is often used for general dependencies such as injecting configurations, or infrastracture (logging for example)

// Passing every dependency as a function can get bit annoying if you have couple of those dependencies
// However, since F# has OO capabilities nothing stops us from creating interface grouping those injected functions together.


type ILogger =
    abstract member LogPre: string -> unit
    abstract member LogPost: string -> unit

let mapperWithLogger (logger: ILogger) (f : int list -> int list) a =
    logger.LogPre "Pre"
    let res = f a
    logger.LogPost "Post"
    res

//Now we can use object expressions to easily implement different loggers
let fileLogger =
    { new ILogger with
        member x.LogPre s = System.IO.File.WriteAllText("C:/abc.txt", s)
        member x.LogPost s = System.IO.File.WriteAllText("C:/abc.txt", s)
    }

let testLogger =
    { new ILogger with
        member x.LogPre s = printfn "%s" s
        member x.LogPost s = printfn "%s" s
    }

let mapperWeUseInApp = mapperWithLogger fileLogger
let mapperWeUseInTest = mapperWithLogger testLogger