#load "examples.fs"
open Examples
(*********************************************************************************************************************)

// As has been stressed many times before, F# is fundamentally a functional language at heart,
// yet the OO features have been nicely integrated and do not have a “tacked-on” feeling.
// As a result, it is quite viable to use F# just as an OO language, as an alternative to C#, say.

type CustomerName(firstName, middleInitial, lastName) =
    member this.FirstName = firstName
    member this.MiddleInitial = middleInitial
    member this.LastName = lastName

// Let's compare it to C# equivalent:

// public class CustomerName
// {
//     public CustomerName(string firstName,
//        string middleInitial, string lastName)
//     {
//         this.FirstName = firstName;
//         this.MiddleInitial = middleInitial;
//         this.LastName = lastName;
//     }

//     public string FirstName { get; private set; }
//     public string MiddleInitial { get; private set; }
//     public string LastName { get; private set; }
// }

type CustomerName2(firstName : string, middleInitial : string, lastName : string) =
    member this.FirstName = firstName
    member this.MiddleInitial = middleInitial
    member this.LastName = lastName


// The example class above has three read-only instance properties. In F#, both properties and methods use the member keyword.

// Also, in the example above, you see the word “this” in front of each member name.
// This is a “self-identifier” that can be used to refer to the current instance of the class.
// Every non-static member must have a self-identifier, even it is not used (as in the properties above).
// There is no requirement to use a particular word, just as long as it is consistent.
// You could use “this” or “self” or “me” or any other word that commonly indicates a self reference.


//TODO: To above class add new property `FullName` that will return all names combined as one string.


// Properties can be divided into three groups:

// * Immutable properties, where there is a “get” but no “set”.
// * Mutable properties, where there is a “get” and also a (possibly private) “set”.
// * Write-only properties, where there is a “set” but no “get”. These are so unusual that I won’t discuss them here, but the MSDN documentation describes the syntax if you ever need it.

type PropertyExample(seed) =
    // private mutable value
    let mutable myProp = seed

    // private function
    let square x = x * x

    // immutable property
    // using a constructor parameter
    member this.Seed = seed

    // immutable property
    // using a private function
    member this.SeedSquared = square seed

    // mutable property
    // changing a private mutable value
    member this.MyProp
        with get() = myProp
        and set(value) =  myProp <- value

    // mutable property with private set
    member this.MyProp2
        with get() = myProp
        and private set(value) =  myProp <- value

    // automatic immutable property
    member val ReadOnlyAuto = 1

    // automatic mutable property
    member val ReadWriteAuto = 1 with get,set

//TODO: Create instance of above class and try to get and set values for each property.


//A method definition is very like a function definition, except that it has the member keyword and the self-identifier instead of just the let keyword.

type MethodExample() =

    // standalone method
    member this.AddOne x =
        x + 1

    // calls another method
    member this.AddTwo x =
        this.AddOne x |> this.AddOne

    // parameterless method
    member this.Pi() =
        3.14159

//After the class declaration, you can optionally have a set of “let” bindings, typically used for defining private fields and functions.

type PrivateValueExample(seed) =

    // private immutable value
    let privateValue = seed + 1

    // private mutable value
    let mutable mutableValue = 42

    // private function definition
    let privateAddToSeed input =
        seed + input

    // public wrapper for private function
    member this.AddToSeed x =
        privateAddToSeed x

    // public wrapper for mutable value
    member this.SetMutableValue x =
        mutableValue <- x

// In addition to the primary constructor embedded in its declaration, a class can have additional constructors.
// These are indicated by the new keyword and must call the primary constructor as their last expression.

type MultipleConstructors(param1, param2) =
    do printfn "Param1=%i Param12=%i" param1 param2

    // secondary constructor
    new(param1) =
        MultipleConstructors(param1,-1)

    // secondary constructor
    new() =
        printfn "Constructing..."
        MultipleConstructors(13,17)

///Just as in C#, classes can have static members, and this is indicated with the static keyword. The static modifier comes before the member keyword.

type StaticExample() =
    member this.InstanceValue = 1
    static member StaticValue = 2  // no "this"


//TODO: Create instance of above classes and play with them


//F# just like C# supports inharitance.

type BaseClass(param1) =
    member this.Param1 = param1

type DerivedClass(param1, param2) =
    inherit BaseClass(param1)
    member this.Param2 = param2


[<AbstractClass>]
type AbstractClass() =
    //Abstract method
    abstract member Add: int -> int -> int
    //Abstract property
    abstract member Pi : float

// with default implementations
type DefaultImplementationClass() =
   // abstract method
   abstract member Add: int -> int -> int
   // abstract property
   abstract member Pi : float

   // defaults
   default this.Add x y = x + y
   default this.Pi = 3.14


//To override an abstract method or property in a subclass, use the override keyword instead of the member keyword.
//Other than that change, the overridden method is defined in the usual way.

[<AbstractClass>]
type Animal() =
   abstract member MakeNoise: unit -> unit

type Dog() =
   inherit Animal()
   override this.MakeNoise () = printfn "woof"

//And to call a base method, use the base keyword, just as in C#.
type Vehicle() =
   abstract member TopSpeed: unit -> int
   default this.TopSpeed() = 60

type Rocket() =
   inherit Vehicle()
   override this.TopSpeed() = base.TopSpeed() * 10

//TODO: Implement classical shape hierarchy including types like Shape, Circle, Rectangle, Square, Triangle and functionalities like calculating areas, perimeters, and returning nice formatted string with information about object (TIP: You can override `ToString` method).

(*********************************************************************************************************************)

// Interfaces are available and fully supported in F#, but there are number of important ways in which
// their usage differs from what you might be used to in C#.

// Definition of interface is really simillar to Abstract Class. The differences are - lack of any additional attribute
// and no parenthesis - interfaces have no constructor

type MyInterface =
   // abstract method
   abstract member Add: int -> int -> int

   // abstract immutable property
   abstract member Pi : float

   // abstract read/write property
   abstract member Area : float with get,set

// When it comes time to implement an interface in a class, F# is quite different from C#.
// In C#, you can add a list of interfaces to the class definition and implement the interfaces implicitly.
// Not so in F#. In F#, all interfaces must be explicitly implemented.
// In an explicit interface implementation, the interface members can only be accessed through an interface instance
// (e.g. by casting the class to the interface type). The interface members are not visible as part of the class itself.

type IAddingService =
    abstract member Add: int -> int -> int

type MyAddingService() =

    interface IAddingService with
        member this.Add x y =
            x + y

    interface System.IDisposable with
        member this.Dispose() =
            printfn "disposed"

//Using interfaces

let mas = new MyAddingService()
// mas.Add 1 2 //error
let adder = mas :> IAddingService
adder.Add 1 2  // ok

// This might seem incredibly awkward, but in practice it is not a problem as in most cases the casting is done implicitly for you.

// For example, you will typically be passing an instance to a function that specifies an interface parameter.
// In this case, the casting is done automatically:

// function that requires an interface
let testAddingService (adder:IAddingService) =
    printfn "1+2=%i" <| adder.Add 1 2  // ok

testAddingService mas // cast automatically

(*********************************************************************************************************************)


// An Object Expression is an F# expression that  creates a new type "on the fly".
// The new type created is anonymous, meaning it has no accessibility and must be based on an existing base type interface or set of interfaces.
// Object Expressions are at the heart of object oriented programming in F#.
// They provide a concise syntax to create an object that inherits from an existing type.

type ISession<'a> =
    abstract member Get : unit -> 'a
    abstract member Put : 'a -> unit

let counter initialState =
  let mutable state = initialState
  { new ISession<int> with
      member x.Put(value) =
        state <- state + value
      member x.Get() =
        state
  }

//TODO: Implement IEnumerator<T> to encapsulate two collection iterators
//so that you can iterate over both at the same time and perform calculation on each item.

open System.Collections.Generic

let map2 f (e1 : IEnumerator<'a>) (e2 : IEnumerator<'b>) : IEnumerator<'c> =
    failwith ""