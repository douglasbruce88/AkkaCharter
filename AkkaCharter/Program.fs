open FSharp.Charting
open System
open System.Windows.Forms

[<EntryPoint>]
[<STAThread>]
let main _ = 
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault false
    Application.Run(Chart.Line([ for x in 0..10 -> x, x * x ]).ShowChart())
    0
