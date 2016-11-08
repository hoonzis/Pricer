namespace Pricer.Core

open System

module StrategiesExamples = 

    let testStrikes stock =
        floor(stock.CurrentPrice*1.05),floor(stock.CurrentPrice*1.1)

    let buildOptionLeg direction strike expiry kind = 
        {
            Definition = Option {
                Direction = direction
                Strike = strike
                Expiry = expiry
                Kind = kind
                Style = European
                PurchaseDate = DateTime.Now
            }
            Pricing = None
        }

    let strangle stock expiry = 
        let strike1, strike2 = testStrikes stock
        {
            Name = "Long Strangle"
            Legs = [ 
                    buildOptionLeg 1.0 strike1 expiry Call
                    buildOptionLeg 1.0 strike2 expiry Put
            ]
            Stock = stock
        }

    let straddle stock expiry =  
        let strike,_ = testStrikes stock
        {
            Name = "Straddle"
            Legs = [ 
                    buildOptionLeg 1.0 strike expiry Call
                    buildOptionLeg 1.0 strike expiry Put
            ]
            Stock = stock
        }

    let butterfly stock expiry = 
        {
            Name = "Butterfly"
            Legs = [ 
                    buildOptionLeg 1.0 (stock.CurrentPrice*1.05) expiry Call
                    buildOptionLeg -1.0 (stock.CurrentPrice*1.1) expiry Call
                    buildOptionLeg -1.0 (stock.CurrentPrice*1.1) expiry Call
                    buildOptionLeg 1.0 (stock.CurrentPrice*1.15) expiry Call
            ]
            Stock = stock
        }

    let riskReversal stock expiry = 
        {
            Name = "Risk Reversal"
            Legs = [ 
                    //out of the money call
                    buildOptionLeg 1.0 (stock.CurrentPrice*1.1) expiry Call
                    //out of the money put
                    buildOptionLeg -1.0 (stock.CurrentPrice*0.9) expiry Put
            ]
            Stock = stock
        }

    let collar stock expiry =  {
            Name = "Collar"
            Legs = [ 
                    //sell 1 out of money call
                    buildOptionLeg -1.0 (stock.CurrentPrice*1.2) expiry Call
                    //buy 1 out of money put
                    buildOptionLeg 1.0 (stock.CurrentPrice*0.8) expiry Put
                    //long some shares
                    {
                        Definition = Cash { 
                            Direction = 1.0
                            Price = stock.CurrentPrice
                        }
                        Pricing = None
                    }
            ]
            Stock = stock
        }

    let coveredCall stock expiry =  
        {
            Name = "Covered Call"
            Legs = [ 
                    //sell 1 out of money call
                    buildOptionLeg -1.0 (stock.CurrentPrice*1.2) expiry Call
                    //long some shares
                    {
                        Definition = Cash { 
                            Direction = 1.0
                            Price = stock.CurrentPrice
                        }
                        Pricing = None
                    }
            ]
            Stock = stock
        }

    let cashPayOff strike ref = ref - strike

    let condor stock expiry = 
        let strike1 = floor(stock.CurrentPrice*0.6)
        let strike2 = floor(stock.CurrentPrice*0.9)
        let strike3 = floor stock.CurrentPrice*1.1
        let strike4 = floor stock.CurrentPrice*1.4
        {
            Name = "Condor"
            Legs = [ 
                    buildOptionLeg -1.0 strike2 expiry Call
                    buildOptionLeg 1.0 strike1 expiry Call
                    buildOptionLeg -1.0 strike3 expiry Call
                    buildOptionLeg 1.0 strike4 expiry Call
            ]
            Stock = stock
        }

    let boxOption stock expiry = 
        let strike1, strike2 = testStrikes stock
        {
            Name = "Box Option"
            Legs = [ 
                    buildOptionLeg 1.0 strike1 expiry Call
                    buildOptionLeg -1.0 strike2 expiry Call
                    buildOptionLeg 1.0 strike2 expiry Call
                    buildOptionLeg -1.0 strike1 expiry Call
            ]
            Stock = stock
    }

    let longCall stock expiry =  {
        Name = "Long Call - Out Of Money"
        Legs = [
                buildOptionLeg 1.0 (stock.CurrentPrice*1.05) expiry Call
        ]
        Stock = stock
    }

    let shortCall stock expiry =  {
        Name = "Short Call - Out Of Money"
        Legs = [
                buildOptionLeg -1.0 (stock.CurrentPrice*1.05) expiry Call
        ]
        Stock = stock
    }

    let callSpread stock expiry =  
        let strike1,strike2 = testStrikes stock
        {
            Name = "Bull Call Spread"
            Legs = [
                    buildOptionLeg -1.0 strike2 expiry Call
                    buildOptionLeg 1.0 strike1 expiry Call
            ]
            Stock = stock
        }

    let putSpread stock expiry = 
        let strike1,strike2 = testStrikes stock
        {
            Name = "Bull Put Spread"
            Legs = [
                    buildOptionLeg -1.0 strike2 expiry Put
                    buildOptionLeg 1.0 strike1 expiry Put
            ]
            Stock = stock
        }

    let strategiesForStock stock expiry = [
        longCall stock expiry
        shortCall stock expiry
        callSpread stock expiry
        putSpread stock expiry
        straddle stock expiry
        strangle stock expiry
        butterfly stock expiry
        riskReversal stock expiry
        collar stock expiry
        condor stock expiry
        boxOption stock expiry
        coveredCall stock expiry
    ]

    let exampleStock = {
        CurrentPrice = 100.0
        Volatility = 0.05
        Rate = 0.03
    }
    
    let exampleStrategies = strategiesForStock exampleStock