namespace Pricer.Fabled

open Pricer.Core

type StockViewModel(s:StockInfo) = 
    let mutable rate = s.Rate.ToString()
    let mutable volatility = s.Volatility.ToString()
    let mutable currentPrice = s.CurrentPrice.ToString()

    member __.buildStock = 
        {
            Rate = float rate
            Volatility = float volatility
            CurrentPrice = float currentPrice
        }
