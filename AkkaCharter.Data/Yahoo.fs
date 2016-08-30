namespace AkkaCharter.Data

module Yahoo = 
    open FSharp.Data
    open System
    
    type Stocks = CsvProvider< AssumeMissingValues=true, IgnoreErrors=true, Sample="Date (Date),Open (float),High (float),Low (float),Close (float),Volume (int64),Adj Close (float)" >
    
    let url = "http://ichart.finance.yahoo.com/table.csv?s="
    let startDateString (date : DateTime) = sprintf "&a=%d&b=%d&c=%d" (date.Month - 1) date.Day date.Year
    let endDateString (date : DateTime) = sprintf "&d=%d&e=%d&f=%d" (date.Month - 1) date.Day date.Year
    
    let getStockPrices stock startDate endDate = 
        let fullUrl = url + stock + startDateString startDate + endDateString endDate
        Stocks.Load(fullUrl).Rows
        |> Seq.toList
        |> List.rev
