namespace OptionsPricing
open System

[<CLIMutable>]
type StockInfo =
    {
        Rate:float
        Volatility: float
        CurrentPrice: float
    }

type Exchange = 
    | LSE
    | EURONEXT

[<CLIMutable>]
type Stock = {
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

