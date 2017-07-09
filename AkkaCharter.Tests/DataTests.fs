namespace AkkaCharter.Tests

module DataTests = 
    open AkkaCharter.Data
    open NUnit.Framework
    open Swensen.Unquote
    open System
    
    [<Test>]
    let ``StartDateString formats correctly``() = 
        let expected = "&startdate=Jan 01 2014"
        let actual = startDateString (new DateTime(2014, 01, 01))
        actual =! expected
    
    [<Test>]
    let ``EndDateString formats correctly``() = 
        let expected = "&enddate=Jan 01 2015"
        let actual = endDateString (new DateTime(2015, 01, 01))
        actual =! expected
    
    [<Test>]
    let ``Get Chart Prices works on MSFT``() = 
        let expected = new Stocks.Row(new DateTime(2014, 01, 02), 37.35, 37.4, 37.1, 37.16, 30643745L)
        let exchange = Exchanges.NASDAQ |> toString
        let actual = 
            List.head (getStockPrices exchange "MSFT" (new DateTime(2014, 01, 01)) (new DateTime(2015, 01, 01)))
        actual =! expected
