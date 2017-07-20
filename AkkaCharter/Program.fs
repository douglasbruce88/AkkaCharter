open Akka.FSharp
open AkkaCharter
open System
open System.Windows.Forms

Application.EnableVisualStyles()
Application.SetCompatibleTextRenderingDefault false
[<STAThread>]
do Application.Run(Form.load())
