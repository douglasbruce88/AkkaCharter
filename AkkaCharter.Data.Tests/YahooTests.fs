namespace AkkaCharter.Data.Tests

module YahooTests = 
    open AkkaCharter.Data.Yahoo
    open NUnit.Framework
    open Swensen.Unquote
    open System
    
    [<Test>]
    let ``StartDateString formats correctly``() = 
        let expected = "&a=0&b=1&c=2014"
        let actual = startDateString (new DateTime(2014, 01, 01))
        actual =! expected
    
    [<Test>]
    let ``EndDateString formats correctly``() = 
        let expected = "&d=0&e=1&f=2015"
        let actual = endDateString (new DateTime(2015, 01, 01))
        actual =! expected
    
    [<Test>]
    let ``Get Chart Prices works on MSFT``() = 
        let expected = [ new Stocks.Row(new DateTime(2014, 01, 02), 37.349998, 37.400002, 37.099998, 37.16, 30632200L, 34.502134) ]
        let actual = List.take 1 (getStockPrices "MSFT" (new DateTime(2014, 01, 01)) (new DateTime(2015, 01, 01)))
        actual =! expected
