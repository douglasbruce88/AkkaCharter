open Akka.FSharp
open AkkaCharter
open System
open System.Windows.Forms

let system = System.create "ChartActors" (Configuration.load())

Application.EnableVisualStyles()
Application.SetCompatibleTextRenderingDefault false
[<STAThread>]
do Application.Run(Form.load system)
