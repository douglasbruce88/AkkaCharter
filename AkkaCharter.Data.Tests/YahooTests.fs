namespace AkkaCharter.Data.Tests

module YahooTests = 
    open AkkaCharter.Data.Yahoo
    open NUnit.Framework
    open Swensen.Unquote
    open System
    
    [<Test>]
    let ``Get Chart Prices works on MSFT``() = 
        let expected = [| (46.73, 47.439999, 46.450001, 46.450001) |]
        let actual = getStockPrices "MSFT" 1 (new DateTime(2014, 01, 01)) (new DateTime(2015, 01, 01))
        actual =! expected
