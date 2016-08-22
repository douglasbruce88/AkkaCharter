namespace AkkaCharter

[<AutoOpen>]
module Form = 
    open AkkaCharter.Data.Yahoo
    open FSharp.Charting
    open FSharp.Charting.ChartTypes
    open System
    open System.Drawing
    open System.Windows.Forms
    
    let createChart startDate endDate ticker = 
        Chart.Line([ for row in getStockPrices ticker startDate endDate do
                         yield row.Date, row.Close ], Name = ticker)
    
    let defaultChart = createChart (new DateTime(2014, 01, 01)) (new DateTime(2015, 01, 01))
    let btnRefresh = new Button(Name = "btnRefresh", Text = "Refresh", Location = Point(20, 200), Size = Size(110, 41), TabIndex = 1, UseVisualStyleBackColor = true)
    let form = new Form(Name = "Main", Visible = true, Text = "Tickers", AutoScaleDimensions = SizeF(6.F, 13.F), AutoScaleMode = AutoScaleMode.Font, ClientSize = Size(1000, 600))
    let listBox = new ListBox(Location = new Point(20, 30), FormattingEnabled = true, SelectionMode = SelectionMode.MultiExtended)

    let tickerPanel = new Panel(Location = new Point(0, 0), Size = new Size(800, 600), BorderStyle = BorderStyle.Fixed3D, Dock = DockStyle.Fill)
    let controlPanel = new Panel(Location = new Point(800, 0), Size = new Size(200, 600), BorderStyle = BorderStyle.Fixed3D, Dock = DockStyle.Right)
    
    listBox.Items.Add("AAPL") |> ignore
    listBox.Items.Add("GOOG") |> ignore
    listBox.Items.Add("MSFT") |> ignore

    form.Controls.Add tickerPanel
    form.Controls.Add controlPanel
    controlPanel.Controls.Add btnRefresh
    controlPanel.Controls.Add listBox

    btnRefresh.Click.Add(fun _ -> 
        let charts = 
            [ for item in listBox.SelectedItems do
                  yield defaultChart (item.ToString()) ]
        
        let chartControl = new ChartControl(Chart.Combine(charts).WithLegend(), Dock = DockStyle.Fill, Name = "Tickers")
        if tickerPanel.Controls.ContainsKey("Tickers") then form.Controls.RemoveByKey("Tickers")
        tickerPanel.Controls.Add chartControl)
    
    let load() = form
