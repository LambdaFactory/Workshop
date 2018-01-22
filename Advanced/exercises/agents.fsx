#load "examples.fs"
open Examples
(*********************************************************************************************************************)

// In this module, we’ll look at the message-based (or actor-based) approach to concurrency.

// In this approach, when one task wants to communicate with another,
// it sends it a message, rather than contacting it directly.
// The messages are put on a queue, and the receiving task (known as an “actor” or “agent”)
// pulls the messages off the queue one at a time to process them.

//From a software design point of view, a message-based approach has a number of benefits:
// - You can manage shared data and resources without locks
// - You can easily follow the “single responsibility principle”, because each agent can be designed to do only one thing.
// - You can easily scale by just creating more agents
// - Errors can be handled gracefully, because the decoupling means that agents can be created and destroyed without
//   affecting their clients.

//F# has a built-in agent class called MailboxProcessor. These agents are very lightweight compared with threads -
//you can instantiate tens of thousands of them at the same time.

let printerAgent = MailboxProcessor.Start(fun inbox->

    // the message processing function
    let rec messageLoop() = async {

        // read a message
        let! msg = inbox.Receive()

        // process a message
        printfn "message is: %s" msg

        // loop to top
        return! messageLoop()
        }

    // start the loop
    messageLoop()
    )

//That's how you use it.
printerAgent.Post "hello"
printerAgent.Post "hello again"
printerAgent.Post "hello a third time"

//Agents in functional programming are solution to 2 big problems:
// - Managing shared state without locks
// - Serialized and buffered access to shared IO

(*********************************************************************************************************************)

type MessageBasedCounter () =

    static let updateState (count,sum) msg =

        // increment the counters and...
        let newSum = sum + msg
        let newCount = count + 1
        printfn "Count is: %i. Sum is: %i" newCount newSum

        // ...emulate a short delay
        Utility.RandomSleep()

        // return the new state
        (newCount,newSum)

    // create the agent
    static let agent = MailboxProcessor.Start(fun inbox ->

        // the message processing function
        let rec messageLoop oldState = async{

            // read a message
            let! msg = inbox.Receive()

            // do the core logic
            let newState = updateState oldState msg

            // loop to top
            return! messageLoop newState
            }

        // start the loop
        messageLoop (0,0)
        )

    // public interface to hide the implementation
    static member Add i = agent.Post i

(*********************************************************************************************************************)

type SerializedLogger() =

    // create the mailbox processor
    let agent = MailboxProcessor.Start(fun inbox ->

        // the message processing function
        let rec messageLoop () = async{

            // read a message
            let! msg = inbox.Receive()

            // write it to the log
            slowConsoleWrite msg

            // loop to top
            return! messageLoop ()
            }

        // start the loop
        messageLoop ()
        )

    // public interface
    member this.Log msg = agent.Post msg

// test in isolation
let serializedLogger = SerializedLogger()
serializedLogger.Log "hello"

(*********************************************************************************************************************)

//TODO: MailboxProcessor class is generic, you can define what messages its instance accept -
//      define an agent that represents calculator - its state is result of calculation so far,
//      and takes as message next opertation (for example `Add 2` or `Divide 7`)






(*********************************************************************************************************************)

// Two way communication - just as easily as we can send messages to MailboxProcessors, a MailboxProcessor can send
// replies back to consumers. For example, we can interrogate the value of a MailboxProcessor using the
// PostAndReply method as follows:

type msg =
    | Incr of int
    | Fetch of AsyncReplyChannel<int>

let counter =
    MailboxProcessor.Start(fun inbox ->
        let rec loop n =
            async { let! msg = inbox.Receive()
                    match msg with
                    | Incr(x) -> return! loop(n + x)
                    | Fetch(replyChannel) ->
                        replyChannel.Reply(n)
                        return! loop(n) }
        loop 0)

counter.Post(Incr 7)
counter.Post(Incr 50)
counter.PostAndReply(fun replyChannel -> Fetch replyChannel)

//TODO: go back to calculator agent and way to get current state of the agent