namespace AkkaCharter

[<AutoOpen>]
module ActorSystemRefs = 

    open Akka.FSharp
    open AkkaCharter
    let system = System.create "ChartActors" (Configuration.load())

