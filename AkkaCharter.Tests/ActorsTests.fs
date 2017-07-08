namespace AkkaCharter.Tests

module ActorsTests = 
    open NUnit.Framework
    open Swensen.Unquote
    open System
    open Akka.FSharp
    open AkkaCharter
    open FSharp.Charting
    open AkkaCharter.Data
    open FSharp.Data
    open Akka.TestKit

    let GetTime(list : string[]) mapfunction  =  
        let startDate = new DateTime(2014, 01 , 01)
        let endDate = new DateTime(2015, 01 , 01)
        let sw = new System.Diagnostics.Stopwatch()
        sw.Start()
        let data = mapfunction (fun item -> getStockPrices item startDate endDate) list
        sw.Stop()
        sw.ElapsedMilliseconds

    [<Test>]
    let ``Test using sequential list, threads, and actors``() = 
        let system = System.create "ChartActors" (Configuration.load())

        let gatheringActor = spawn system "counters" (MyActors.pureGatheringActor system)
                
        let list = [|"AAPL"
                     "AMZN"
                     "GOOG"
                     "HPQ"
                     "IBM"
                     "MSFT"
                     "NOK"
                     "SNY"
                     "TWTR"
                     "TYO"|]

        let ask = fun _ -> gatheringActor <? GetData(list)

        let GetTimeP = GetTime list

         // Initial warmup
        GetTimeP Array.map |> ignore
        GetTimeP Array.Parallel.map |> ignore

        // Repeats
        let times = 10

        let s = seq { for _ in 1 .. times -> GetTimeP Array.map }
        let sTime = Seq.sum s / (int64 times)
        printf "Sequential time: %i \r\n" sTime

        let p = seq { for _ in 1 .. times -> GetTimeP Array.Parallel.map }
        let pTime = Seq.sum p / (int64 times)
        printf "Parallel time: %i \r\n" pTime

        // Actors 
        
//        let sw = new System.Diagnostics.Stopwatch()
//        sw.Start()
//        let r = ask() 
//        //let response = Async.RunSynchronously r
//        sw.Stop()

//        printf "Actor time: %i \r\n" sw.ElapsedMilliseconds

        sTime <=! pTime

        