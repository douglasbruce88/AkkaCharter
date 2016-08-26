﻿namespace AkkaCharter

[<AutoOpen>]
module Form = 
    open Akka.Actor
    open Akka.FSharp
    open System.Drawing
    open System.Windows.Forms
    
    let btnRefresh = new Button(Name = "btnRefresh", Text = "Refresh", Location = Point(20, 200), Size = Size(110, 41), TabIndex = 1, UseVisualStyleBackColor = true)
    let form = new Form(Name = "Main", Visible = true, Text = "Tickers", AutoScaleDimensions = SizeF(6.F, 13.F), AutoScaleMode = AutoScaleMode.Font, ClientSize = Size(1000, 600))
    let listBox = new ListBox(Location = new Point(20, 30), FormattingEnabled = true, SelectionMode = SelectionMode.MultiExtended)
    let tickerPanel = new Panel(Location = new Point(0, 0), Size = new Size(800, 600), BorderStyle = BorderStyle.Fixed3D, Dock = DockStyle.Fill)
    let controlPanel = new Panel(Location = new Point(800, 0), Size = new Size(200, 600), BorderStyle = BorderStyle.Fixed3D, Dock = DockStyle.Right)
    let items = [ "AAPL"; "ADR"; "AMZN"; "GOOG"; "HPQ"; "IBM"; "MSFT"; "NOK"; "TWTR"; "TYO" ]
    
    List.map (fun item -> listBox.Items.Add(item)) items |> ignore
    form.Controls.Add tickerPanel
    form.Controls.Add controlPanel
    controlPanel.Controls.Add btnRefresh
    controlPanel.Controls.Add listBox
    
    let load (system : ActorSystem) = 
        let gatheringActor = spawn system "counters" (MyActors.gatheringActor tickerPanel form system)
        
        let listBoxAsArray (listBox : ListBox) = 
            Array.ofList [ for item in listBox.SelectedItems -> item.ToString() ]
        
        let ask = fun _ -> gatheringActor <! GetData(listBoxAsArray listBox)
        btnRefresh.Click.Add(ask)
        form
