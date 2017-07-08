namespace AkkaCharter

module Data = 
    open FSharp.Data
    open System
    open System.Net
    open System.Net.Security
    
    [<Literal>]
    let Sample = "Date (Date),Open (float),High (float),Low (float),Close (float),Volume (int64)"
    
    type Stocks = CsvProvider< AssumeMissingValues=true, IgnoreErrors=true, Sample=Sample >
    
    let url = "https://www.google.com/finance/historical?output=csv&q=NASDAQ:"
    let startDateString (date : DateTime) = sprintf "&startdate=%s" (date.ToString("MMM dd yyyy"))
    let endDateString (date : DateTime) = sprintf "&enddate=%s" (date.ToString("MMM dd yyyy"))
    
    let getStockPrices stock startDate endDate = 
        ServicePointManager.ServerCertificateValidationCallback <- RemoteCertificateValidationCallback
                                                                       (fun _ _ _ _ -> true)
        let fullUrl = url + stock + startDateString startDate + endDateString endDate
        Stocks.Load(fullUrl).Rows
        |> Seq.toList
        |> List.rev
