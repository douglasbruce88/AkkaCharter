module Program

open System
open System.Windows.Forms
open AkkaCharter

Application.EnableVisualStyles ()
Application.SetCompatibleTextRenderingDefault false

[<STAThread>]    
do Application.Run (Form.load())
