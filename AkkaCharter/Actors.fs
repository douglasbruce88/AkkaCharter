namespace AkkaCharter

[<AutoOpen>]
module Messages = 
    open AkkaCharter.Data
    open System
    
    type DrawChart = 
        | GetDataBetweenDates of StartDate : DateTime * EndDate : DateTime
        | ClearCache
    
    type DataMessage = 
        | StockData of string * Stocks.Row list
        | GetData of string []

[<AutoOpen>]
module MyActors = 
    open Akka.Actor
    open Akka.FSharp
    open AkkaCharter.Data
    open FSharp.Charting
    open FSharp.Charting.ChartTypes
    open System
    open System.Diagnostics
    open System.Windows.Forms
    
    let createCharts (tickerPanel : System.Windows.Forms.Panel) finalData = 
        let charts = 
            List.map (fun rows -> 
                Chart.Line([ for row : Stocks.Row in snd rows do
                                 yield row.Date, row.Close ], Name = fst rows)) finalData
        
        let chartControl = new ChartControl(Chart.Combine(charts).WithLegend(), Dock = DockStyle.Fill, Name = "Tickers")
        if tickerPanel.Controls.ContainsKey("Tickers") then tickerPanel.Controls.RemoveByKey("Tickers")
        tickerPanel.Controls.Add chartControl
    
    let tickerActor (ticker : string) (mailbox : Actor<_>) = 
        let rec doesNotHaveData() = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetDataBetweenDates(startDate, endDate) -> 
                    let exchange = Exchanges.NASDAQ |> toString
                    let stockData = StockData((ticker, getStockPrices exchange ticker startDate endDate))
                    // TODO: switch for caching
                    mailbox.Sender() <! stockData
                    return! doesNotHaveData() // hasData(stockData)
                | ClearCache -> return! doesNotHaveData()
            }
        
        and hasData (stockData : DataMessage) = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetDataBetweenDates(_) -> 
                    mailbox.Sender() <! stockData
                    return! hasData (stockData)
                | ClearCache -> return! doesNotHaveData()
            }
        
        doesNotHaveData()
    
    let gatheringActor (tickerPanel : System.Windows.Forms.Panel) (sw : System.Diagnostics.Stopwatch) 
        (system : ActorSystem) (mailbox : Actor<_>) = 
        let rec waiting (existingActorRefs : IActorRef Set) = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetData d -> 
                    sw.Restart()
                    let existingNames = existingActorRefs |> Set.map (fun (x : IActorRef) -> x.Path.Name)
                    let newActors = existingNames |> Set.difference (Set.ofArray d)
                    
                    let newActorRefs = 
                        [ for item in newActors do
                              yield spawn system (item.ToString()) (tickerActor (item.ToString())) ]
                    
                    let combinedActorRefs = existingActorRefs |> Set.union (Set.ofList newActorRefs)
                    // TODO: configurable start/end dates
                    let tell = 
                        fun dataActorRef -> 
                            dataActorRef 
                            <! (GetDataBetweenDates(new DateTime(2014, 01, 01), new DateTime(2015, 01, 01)))
                    Set.map tell combinedActorRefs |> ignore
                    return! gettingData (Set.count combinedActorRefs) combinedActorRefs []
                | _ -> return! waiting (existingActorRefs)
            }
        
        and gettingData (numberOfResultsToSee : int) (existingActorRefs : IActorRef Set) 
            (soFar : (string * Stocks.Row list) list) = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | StockData(tickerName, data) when numberOfResultsToSee = 1 -> 
                    let finalData = ((tickerName, data) :: soFar)
                    createCharts tickerPanel finalData
                    sw.Stop()
                    MessageBox.Show(sprintf "Retrieved data in %d ms" sw.ElapsedMilliseconds) |> ignore
                    return! waiting existingActorRefs
                | StockData(tickerName, data) -> 
                    return! gettingData (numberOfResultsToSee - 1) existingActorRefs ((tickerName, data) :: soFar)
                | _ -> return! waiting existingActorRefs
            }
        
        waiting (Set.empty)
    











































    let testActor (system : ActorSystem) (mailbox : Actor<_>) = 
        let rec waiting() = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetData tickers -> 
                    let dataActorRefs = 
                        [ for ticker in tickers do
                              yield spawn system ticker (tickerActor ticker) ]
                    
                    let tell = 
                        fun dataActorRef -> 
                            dataActorRef 
                            <! (GetDataBetweenDates(new DateTime(2014, 01, 01), new DateTime(2015, 01, 01)))
                    List.map tell dataActorRefs |> ignore
                    return! gettingData (List.length dataActorRefs) []
                | _ -> return! waiting()
            }
        
        and gettingData (numberOfResultsToSee : int) (soFar : (string * Stocks.Row list) list) = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | StockData(tickerName, data) when numberOfResultsToSee = 1 -> 
                    let finalData = ((tickerName, data) :: soFar)
                    
                    let charts = 
                        List.map (fun rows -> 
                            Chart.Line([ for row : Stocks.Row in snd rows do
                                             yield row.Date, row.Close ], Name = fst rows)) finalData
                    return charts
                | StockData(tickerName, data) -> 
                    return! gettingData (numberOfResultsToSee - 1) ((tickerName, data) :: soFar)
                | _ -> return! waiting()
            }
        
        waiting()
