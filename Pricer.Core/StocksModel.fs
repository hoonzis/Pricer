namespace Pricer.Core

open System

[<CLIMutable>]
type StockInfo =
    {
        Rate:float
        Volatility: float
        CurrentPrice: float
    }
