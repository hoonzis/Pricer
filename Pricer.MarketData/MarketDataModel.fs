namespace Pricer.MarketData

open System

type Exchange = 
    | LSE
    | EURONEXT
    override x.ToString() = sprintf "%A" x

    

[<CLIMutable>]
type StockRefData = {
    Exchange: Exchange
    Ticker: string
}

type Tick = {
    Date:DateTime
    Close:float
    Open: float option
}

type VolatilityEstimationMethod = 
    | CloseVsClose
    | CloseVsOpen


