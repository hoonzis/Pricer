namespace Pricer.Fabled

open Pricer.Core

type StockViewModel(s:StockInfo) = 
    let mutable rate = s.Rate
    let mutable volatility = s.Volatility
    let mutable currentPrice = s.CurrentPrice

    member __.buildStock = {
        Rate = rate
        Volatility = volatility
        CurrentPrice = currentPrice
    }
