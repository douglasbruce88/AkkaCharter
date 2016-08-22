open AkkaCharter.Data.Yahoo
open FSharp.Charting
open System
open System.Windows.Forms

let createChart startDate endDate = 
    [ for row in getStockPrices "MSFT" startDate endDate do
          yield row.Date, row.High, row.Low, row.Open, row.Close ]
    |> Chart.Candlestick
    |> Chart.WithArea.AxisY(Minimum = 30.0, Maximum = 52.0)

[<EntryPoint>]
[<STAThread>]
let main _ = 
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault false
    let chart = createChart (new DateTime(2014, 01, 01)) (new DateTime(2015, 01, 01))
    Application.Run(chart.ShowChart())
    0
