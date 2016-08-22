namespace AkkaCharter.Data

module Yahoo = 
    open System
    open System.Net
    
    // URL of a service that generates price data
    let url = "http://ichart.finance.yahoo.com/table.csv?s="
    
    let startDateString (date: DateTime) = sprintf "&a=%d&b=%d&c=%d" (date.Month-1) date.Day date.Year
    let endDateString (date: DateTime) = sprintf "&d=%d&e=%d&f=%d" (date.Month-1) date.Day date.Year

    /// Returns prices (as tuple) of a given stock for a 
    /// specified number of days (starting from the most recent)
    let getStockPrices stock count startDate endDate = 
        // Download the data and split it into lines
        let fullUrl = url + stock + startDateString startDate + endDateString endDate
        let wc = new WebClient()
        let data = wc.DownloadString(fullUrl)
        let dataLines = data.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries)
        // Parse lines of the CSV file and take specified
        // number of days using in the oldest to newest order
        seq { 
            for line in dataLines |> Seq.skip 1 do
                let infos = line.Split(',')
                yield float infos.[1], float infos.[2], float infos.[3], float infos.[4]
        }
        |> Seq.take count
        |> Array.ofSeq
        |> Array.rev
