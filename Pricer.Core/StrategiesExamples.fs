namespace Pricer.Core

open System

module StrategiesExamples = 

    let testStrikes stock =
        floor(stock.CurrentPrice*1.1),floor(stock.CurrentPrice*1.4)

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

    let expiry = DateTime.Now.AddDays(60.0)
    let strangle stock = 
        let strike1, strike2 = testStrikes stock
        {
            Name = "Long Strangle"
            Legs = [ 
                    buildOptionLeg 1.0 strike1 expiry Call
                    buildOptionLeg 1.0 strike2 expiry Put
            ]
            Stock = stock
        }

    let straddle stock = 
        let strike,_ = testStrikes stock
        {
            Name = "Straddle"
            Legs = [ 
                    buildOptionLeg 1.0 strike expiry Call
                    buildOptionLeg 1.0 strike expiry Put
            ]
            Stock = stock
        }

    let butterfly stock = 
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

    let riskReversal stock = 
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

    let collar stock = {
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

    let coveredCall stock = 
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

    let condor stock = 
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

    let boxOption stock = 
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

    let longCall stock = {
        Name = "Long Call - Out Of Money"
        Legs = [
                buildOptionLeg 1.0 (stock.CurrentPrice*1.2) expiry Call
        ]
        Stock = stock
    }

    let shortCall stock = {
        Name = "Short Call - Out Of Money"
        Legs = [
                buildOptionLeg -1.0 (stock.CurrentPrice*1.2) expiry Call
        ]
        Stock = stock
    }

    let callSpread stock = 
        let strike1,strike2 = testStrikes stock
        {
            Name = "Bull Call Spread"
            Legs = [
                    buildOptionLeg -1.0 strike2 expiry Call
                    buildOptionLeg 1.0 strike1 expiry Call
            ]
            Stock = stock
        }

    let putSpread stock = 
        let strike1,strike2 = testStrikes stock
        {
            Name = "Bull Put Spread"
            Legs = [
                    buildOptionLeg -1.0 strike2 expiry Put
                    buildOptionLeg 1.0 strike1 expiry Put
            ]
            Stock = stock
        }

    let strategiesForStock stock = [
        longCall stock
        shortCall stock
        callSpread stock
        putSpread stock
        straddle stock
        strangle stock
        butterfly stock
        riskReversal stock
        collar stock
        condor stock
        boxOption stock
        coveredCall stock
    ]

    let exampleStock = {
        CurrentPrice = 100.0
        Volatility = 0.05
        Rate = 0.03
    }
    
    let exampleStrategies = strategiesForStock exampleStock