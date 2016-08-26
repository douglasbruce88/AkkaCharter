open AkkaCharter
open System
open System.Windows.Forms

let system = Akka.FSharp.System.create "ChartActors" (Akka.FSharp.Configuration.load())

Application.EnableVisualStyles()
Application.SetCompatibleTextRenderingDefault false
[<STAThread>]
do Application.Run(Form.load system)
