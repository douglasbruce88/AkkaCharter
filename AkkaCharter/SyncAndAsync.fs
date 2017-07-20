namespace AkkaCharter

[<AutoOpen>]
module SyncAndAsync = 
    open Data
    open FSharp.Charting
    open FSharp.Charting.ChartTypes
    open System
    open System.Diagnostics
    open System.Windows.Forms
    
    let createChart startDate endDate ticker = 
        Chart.Line([ for row in getStockPrices (Exchanges.NASDAQ |> toString) ticker startDate endDate do
                         yield row.Date, row.Close ], Name = ticker)
    
    let getDataForChart = createChart (new DateTime(2014, 01, 01)) (new DateTime(2015, 01, 01))
    
    let getCharts (tickerPanel : System.Windows.Forms.Panel) mapfunction (list : string []) = 
        let sw = new Stopwatch()
        sw.Start()
        let charts = mapfunction getDataForChart list
        let chartControl = new ChartControl(Chart.Combine(charts).WithLegend(), Dock = DockStyle.Fill, Name = "Tickers")
        if tickerPanel.Controls.ContainsKey("Tickers") then tickerPanel.Controls.RemoveByKey("Tickers")
        tickerPanel.Controls.Add chartControl
        sw.Stop()
        MessageBox.Show(sprintf "Retrieved data in %d ms" sw.ElapsedMilliseconds) |> ignore
    
    let getChartsSync (tickerPanel : System.Windows.Forms.Panel) = getCharts tickerPanel Array.map
    let getChartsTasks (tickerPanel : System.Windows.Forms.Panel) = getCharts tickerPanel Array.Parallel.map
