namespace AkkaCharter

[<AutoOpen>]
module Messages = 
    open AkkaCharter.Data.Yahoo
    open System
    
    type DrawChart = 
        | GetDataBetweenDates of StartDate : DateTime * EndDate : DateTime
    
    type DataMessage = 
        | StockData of string * Stocks.Row list
        | GetData of string []

[<AutoOpen>]
module MyActors = 
    open Akka.Actor
    open Akka.FSharp
    open AkkaCharter.Data.Yahoo
    open FSharp.Charting
    open FSharp.Charting.ChartTypes
    open System
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
        let rec loop() = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetDataBetweenDates(startDate, endDate) -> 
                    let stockData = StockData((ticker, getStockPrices ticker startDate endDate))
                    mailbox.Sender() <! stockData
                    mailbox.Self <! (PoisonPill.Instance)
            }
        loop()
    
    let createChart startDate endDate ticker = 
        Chart.Line([ for row in getStockPrices ticker startDate endDate do
                         yield row.Date, row.Close ], Name = ticker)
    
    let defaultChart = createChart (new DateTime(2014, 01, 01)) (new DateTime(2015, 01, 01))
    
    let gatheringActor (tickerPanel : System.Windows.Forms.Panel) (sw : System.Diagnostics.Stopwatch) (system : ActorSystem) (mailbox : Actor<_>) = 
        let rec waiting() = 
            actor { 
                let! message = mailbox.Receive()
                match message with
                | GetData d -> 
                    sw.Restart()
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
                | StockData(tickerName, data) when numberOfResultsToSee = 1 -> 
                    let finalData = ((tickerName, data) :: soFar)
                    createCharts tickerPanel finalData
                    sw.Stop()
                    MessageBox.Show(sprintf "Retrieved data in %d ms" sw.ElapsedMilliseconds) |> ignore
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
                | StockData(tickerName, data) when numberOfResultsToSee = 1 -> 
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
    
    let getCharts (tickerPanel : System.Windows.Forms.Panel) mapfunction (list : string []) = 
        let sw = new System.Diagnostics.Stopwatch()
        sw.Start()

        let charts = mapfunction defaultChart list
        let chartControl = new ChartControl(Chart.Combine(charts).WithLegend(), Dock = DockStyle.Fill, Name = "Tickers")
        if tickerPanel.Controls.ContainsKey("Tickers") then tickerPanel.Controls.RemoveByKey("Tickers")
        tickerPanel.Controls.Add chartControl

        sw.Stop()
        MessageBox.Show(sprintf "Retrieved data in %d ms" sw.ElapsedMilliseconds) |> ignore
    
    let getChartsSync (tickerPanel : System.Windows.Forms.Panel) = getCharts tickerPanel Array.map
    let getChartsTasks (tickerPanel : System.Windows.Forms.Panel)= getCharts tickerPanel Array.Parallel.map
