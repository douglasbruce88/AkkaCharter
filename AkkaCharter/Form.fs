namespace AkkaCharter

[<AutoOpen>]
module Form = 
    open Akka.Actor
    open Akka.FSharp
    open AkkaCharter.Data
    open System.Drawing
    open System.Windows.Forms
    open System.Diagnostics
    
    let btnRunActors = 
        new Button(Name = "btnRunActors", Text = "Run using Actors", Location = Point(20, 200), Size = Size(110, 41), 
                   TabIndex = 1, UseVisualStyleBackColor = true)
    let btnRunSync = 
        new Button(Name = "btnRunSync", Text = "Run synchronously", Location = Point(20, 250), Size = Size(110, 41), 
                   TabIndex = 1, UseVisualStyleBackColor = true)
    let btnRunTasks = 
        new Button(Name = "btnRunTasks", Text = "Run using Tasks", Location = Point(20, 300), Size = Size(110, 41), 
                   TabIndex = 1, UseVisualStyleBackColor = true)
    let form = 
        new Form(Name = "Main", Visible = true, Text = "Tickers", AutoScaleDimensions = SizeF(6.F, 13.F), 
                 AutoScaleMode = AutoScaleMode.Font, ClientSize = Size(1000, 600))
    let listBox = 
        new ListBox(Location = new Point(20, 30), FormattingEnabled = true, SelectionMode = SelectionMode.MultiExtended, 
                    Height = 150)
    let tickerPanel = 
        new Panel(Location = new Point(0, 0), Size = new Size(800, 600), BorderStyle = BorderStyle.Fixed3D, 
                  Dock = DockStyle.Fill)
    let controlPanel = 
        new Panel(Location = new Point(800, 0), Size = new Size(200, 600), BorderStyle = BorderStyle.Fixed3D, 
                  Dock = DockStyle.Right)
    
    let items = 
        tickers
        |> Seq.map (fun x -> x.Symbol)
        |> Seq.take 10
        |> Seq.toList
    
    List.map (fun item -> listBox.Items.Add(item)) items |> ignore
    form.Controls.Add tickerPanel
    form.Controls.Add controlPanel
    controlPanel.Controls.Add btnRunActors
    controlPanel.Controls.Add btnRunSync
    controlPanel.Controls.Add btnRunTasks
    controlPanel.Controls.Add listBox
    
    let load (system : ActorSystem) = 
        let sw = new Stopwatch()
        let gatheringActor = spawn system "counters" (MyActors.gatheringActor tickerPanel sw system)
        
        let listBoxAsArray (listBox : ListBox) = 
            Array.ofList [ for item in listBox.SelectedItems -> item.ToString() ]
        
        let ask = fun _ -> gatheringActor <! GetData(listBoxAsArray listBox)
        btnRunActors.Click.Add(ask)
        let getChartsSync = fun _ -> MyActors.getChartsSync tickerPanel (listBoxAsArray listBox)
        btnRunSync.Click.Add(getChartsSync)
        let getChartsTasks = fun _ -> MyActors.getChartsTasks tickerPanel (listBoxAsArray listBox)
        btnRunTasks.Click.Add(getChartsTasks)
        form
