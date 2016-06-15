namespace Pricer

open System

type BsPricer(math:IMathProvider) = 
    member this.cashPricing (leg:CashLeg) = {
        Premium = leg.Price
        Delta = 1.0
    }

    // dummy pricing for CBs
    member this.convertiblePricing (stock:StockInfo) (leg:ConvertibleLeg) = {
        Premium = leg.ReferencePrice
        Delta = 0.45
    }

    member this.blackScholes (stock:StockInfo) (option:OptionLeg) =
        let price,delta =
            // We can only calculate if the option concerns the future
            if option.TimeToExpiry > 0.0 then
                // Calculate d1 and d2 and pass them to cumulative distribution
                let d1 =
                    ( log(stock.CurrentPrice / option.Strike) +
                        (stock.Rate + 0.5 * pown stock.Volatility 2) * option.TimeToExpiry ) /
                    ( stock.Volatility * sqrt option.TimeToExpiry )
                let d2 = d1 - stock.Volatility * sqrt option.TimeToExpiry
                let N1 = math.cdf(d1)
                let N2 = math.cdf(d2)

                let discountedStrike = option.Strike * exp (-stock.Rate * option.TimeToExpiry)
                let call = stock.CurrentPrice * N1 - discountedStrike * N2
                match option.Kind with
                    | Call -> call, N1
                    | Put -> call + discountedStrike - stock.CurrentPrice, N1 - 1.0
            else
                // If the option has expired, calculate payoff directly
                match option.Kind with
                    | Call -> max (stock.CurrentPrice - option.Strike) 0.0,1.0
                    | Put -> max (option.Strike - stock.CurrentPrice) 0.0,1.0
        {
            Premium = price
            Delta = delta
        }

    
