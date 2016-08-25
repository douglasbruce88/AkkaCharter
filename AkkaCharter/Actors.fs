namespace AkkaCharter

[<AutoOpen>]
module Messages = 
    open AkkaCharter.Data.Yahoo
    open System
    
    type DrawChart = 
        | GetDataBetweenDates of StartDate : DateTime * EndDate : DateTime
    
    type DataMessage = 
        | StockData of string * Stocks.Row list
        | GetData of string[]

[<AutoOpen>]
module MyActors = 
    open Akka.Actor
    open Akka.FSharp
    open AkkaCharter.Data.Yahoo
    open FSharp.Charting
    open FSharp.Charting.ChartTypes
    open System
    open System.Windows.Forms
    
    let tickerActor (ticker : string) (mailbox : Actor<_>) = 
        let rec loop = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetDataBetweenDates(startDate, endDate) -> 
                    let stockData = StockData((ticker, getStockPrices ticker startDate endDate))
                    mailbox.Sender() <! stockData
            }
        loop
    
    let createChart startDate endDate ticker = 
        Chart.Line([ for row in getStockPrices ticker startDate endDate do
                         yield row.Date, row.Close ], Name = ticker)
    
    let defaultChart = createChart (new DateTime(2014, 01, 01)) (new DateTime(2015, 01, 01))
    
    let gatheringActor (tickerPanel : System.Windows.Forms.Panel) (form : System.Windows.Forms.Form) (system : ActorSystem) (mailbox : Actor<_>) = 
        let rec waiting() = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetData d -> 
                    let dataActorRefs = 
                        [ for item in d do
                              yield spawn system (item.ToString()) (tickerActor (item.ToString())) ]
                    
                    let tell = fun dataActorRef -> dataActorRef <! (GetDataBetweenDates(new DateTime(2014, 01, 01), new DateTime(2015, 01, 01)))
                    List.map tell dataActorRefs |> ignore
                    return! gettingData (List.length dataActorRefs) []
                | _ -> return! waiting()
            }
        
        and gettingData (numberOfResultsToSee : int) (soFar : (string * Stocks.Row list) list) = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | StockData (tickerName, data) when numberOfResultsToSee = 1 -> 
                    let finalData = ((tickerName, data) :: soFar) 
                    let charts = 
                        List.map (fun rows -> 
                            Chart.Line([ for row : Stocks.Row in snd rows do
                                             yield row.Date, row.Close ], Name = fst rows)) finalData
                    
                    let chartControl = new ChartControl(Chart.Combine(charts).WithLegend(), Dock = DockStyle.Fill, Name = "Tickers")
                    if tickerPanel.Controls.ContainsKey("Tickers") then form.Controls.RemoveByKey("Tickers")
                    tickerPanel.Controls.Add chartControl
                    return! waiting()
                | StockData(tickerName, data) -> return! gettingData (numberOfResultsToSee - 1) ((tickerName, data) :: soFar)
                | _ -> return! waiting()
            }
        
        waiting()
    
    let pureGatheringActor (system : ActorSystem) (mailbox : Actor<_>) = 
        let rec waiting() = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetData tickers -> 
                    let dataActorRefs = 
                        [ for ticker in tickers do
                              yield spawn system ticker (tickerActor ticker) ]
                    
                    let tell = fun dataActorRef -> dataActorRef <! (GetDataBetweenDates(new DateTime(2014, 01, 01), new DateTime(2015, 01, 01)))
                    List.map tell dataActorRefs |> ignore
                    return! gettingData (List.length dataActorRefs) []
                | _ -> return! waiting()
            }
        
        and gettingData (numberOfResultsToSee : int) (soFar : (string * Stocks.Row list) list) = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | StockData (tickerName, data) when numberOfResultsToSee = 1 -> 
                    let finalData = ((tickerName, data) :: soFar) 
                    let charts = 
                        List.map (fun rows -> 
                            Chart.Line([ for row : Stocks.Row in snd rows do
                                             yield row.Date, row.Close ], Name = fst rows)) finalData
                    return charts
                | StockData(tickerName, data) -> return! gettingData (numberOfResultsToSee - 1) ((tickerName, data) :: soFar)
                | _ -> return! waiting() 
            }
        
        waiting()
