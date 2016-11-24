namespace Pricer.Core

open System

[<CLIMutable>]
type StockInfo =
    {
        Rate:float
        Volatility: float
        CurrentPrice: float
    }

type Tick = {
    Date:DateTime
    Close:float
    Open: float option
}

type VolatilityEstimationMethod = 
    | CloseVsClose
    | CloseVsOpen

