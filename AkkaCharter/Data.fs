namespace AkkaCharter

module Data = 
    open FSharp.Data
    open Microsoft.FSharp.Reflection
    open System
    open System.Net
    open System.Net.Security
    
    [<Literal>]
    let IndexComponents = "Symbol,Name,LastSale,MarketCap,IPOyear,Sector,industry,Summary Quote"
    
    type Index = CsvProvider< AssumeMissingValues=true, IgnoreErrors=true, Sample=IndexComponents >
    
    let path = "C:\Projects\AkkaCharter\Tickers\companylist.csv"
    let tickers = Index.Load(path).Rows
    
    [<Literal>]
    let Sample = "Date (Date),Open (float),High (float),Low (float),Close (float),Volume (int64)"
    
    type Exchanges = 
        | NASDAQ
        | NYSE
    
    let toString (x : 'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name
    
    type Stocks = CsvProvider< AssumeMissingValues=true, IgnoreErrors=true, Sample=Sample >
    
    let url = "https://www.google.com/finance/historical?output=csv&q="
    let startDateString (date : DateTime) = sprintf "&startdate=%s" (date.ToString("MMM dd yyyy"))
    let endDateString (date : DateTime) = sprintf "&enddate=%s" (date.ToString("MMM dd yyyy"))
    
    let getStockPrices exchange stock startDate endDate = 
        ServicePointManager.ServerCertificateValidationCallback <- RemoteCertificateValidationCallback
                                                                       (fun _ _ _ _ -> true)
        let fullUrl = url + exchange + ":" + stock + startDateString startDate + endDateString endDate
        Stocks.Load(fullUrl).Rows
        |> Seq.toList
        |> List.rev
