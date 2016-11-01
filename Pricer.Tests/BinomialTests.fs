namespace Pricer.Tests

open Pricer
open System
open System.Reflection
open System.IO
open FsUnit
open NUnit.Framework
open Pricer.Core

[<TestFixture>]
type BinomialTests() = 
    
    let mathProvider = new MathNetProvider()
    let bsPricer = new BlackScholesPricer(mathProvider)
    let fullPricer = new FullPricer()
    let payoffGenerator = new PayoffsGenerator(fullPricer)
    
    let callPrice = 2.7172467445106512
    let callDelta = 0.54373433554702966
    let europeanPutPrice = 2.0542675521718747
    let europeanDeltaPrice = -0.44565561894046612


    [<Test>]
    member this.``test simple binomial`` () =
        let price = Binomial.binomialPrice 120.0 130.0 0.03 1.15
        price |> should equal 2.792773459184565

        
    [<Test>]
    member this.``binomial pricing european call in`` () =
        let price = Binomial.binomial TestData.stock TestData.europeanCall 1000 Imperative
        price.Premium |> should equal callPrice
        price.Delta |> should equal callDelta

    // American and European call have the same price
    [<Test>]
    member this.``binomial american call`` () = 
        let price = Binomial.binomial TestData.stock TestData.americanCall 1000 Imperative
        price.Premium |> should equal callPrice
        price.Delta |> should equal callDelta
        
    
    [<Test>]
    member this.``binomial euroean put`` () =        
        let price = Binomial.binomial TestData.stock TestData.europeanPut 1000 Imperative
        price.Premium |> should equal europeanPutPrice
        price.Delta |> should equal europeanDeltaPrice

    // American put has higher premium then european put and as well higher delta
    [<Test>]
    member this.``binomial american put`` () =        
        let price = Binomial.binomial TestData.stock TestData.americanPut 1000 Imperative
        price.Premium |> should equal 2.3156625779008477
        price.Delta |> should equal -0.52646992694089489

    [<Test>]
    member this.``binomial european put - functional`` () =        
        let price = Binomial.binomial TestData.stock TestData.europeanPut 1000 Functional
        price.Premium |> should equal europeanPutPrice
        price.Delta |> should equal europeanDeltaPrice

    [<Test>]
    member this.``binomial american put - functional`` () =        
        let price = Binomial.binomial TestData.stock TestData.americanPut 1000 Functional
        price.Premium |> should equal 2.3156625779008477
        price.Delta |> should equal -0.52646992694089489

    [<Test>]
    member this.``binomial european call - functional`` () =        
        let price = Binomial.binomial TestData.stock TestData.europeanCall 1000 Functional
        price.Premium |> should equal callPrice
        price.Delta |> should equal callDelta

    [<Test>]
    member this.``binomial american call - functional`` () =        
        let price = Binomial.binomial TestData.stock TestData.americanCall 1000 Functional
        price.Premium |> should equal callPrice
        price.Delta |> should equal callDelta

    [<Test>]
    member this.``end node price generation tests`` () =
        let periods = 100000
        let up = 1.25
        let uu = 1.25**2.0
        let lowest = (100.0*(0.8**(float periods)))
        let highest = lowest * ((1.25*1.25)**(float periods-1.0))

        // let's say options value is the same as the value of the stock
        let optionVal = id
        
        let prices = Binomial.generateEndNodePrices 100.0 1.25 periods optionVal
        prices.[0] |> should equal { 
            Binomial.emptyNode with
                Stock = lowest
                Option = lowest
            }

        prices.[1] |> should equal {
            Binomial.emptyNode with
                Stock = lowest*1.25*1.25
                Option = lowest*1.25*1.25
            }
     
    [<Test>]
    member this.``single binomial step test`` () =
        let prices = [|
            { Binomial.emptyNode with
                Stock = 1.0
                Option = 1.0
            }
            { Binomial.emptyNode with
                Stock = 2.0
                Option = 2.0
            }
            { Binomial.emptyNode with
                Stock = 3.0
                Option = 2.0
            }
            { Binomial.emptyNode with
                Stock = 4.0
                Option = 4.0
            }
        |]

        //let's test with european call, so there is premature execution
        let pricing = {
            Periods = 100
            Down = 0.6
            Up = 1.4
            PUp = 0.6
            PDown = 0.4
            Option = TestData.europeanCall
            Rate = 1.0
            Ref = 100.0
        }

        //let's say the derivative price is the same as the TestData.stock price
        let optionVal = id
        
        // in real world the R will be => exp (stock.Rate*deltaT)
        // the computation of the continuation of the derivative price
        // dPrice <- (downPrice*pDown + upPrice*pUp)*exp(-r*deltaT)
        // in the model Rate<-exp(r*deltaT)
        let newPrices = Binomial.step pricing optionVal prices

        newPrices |> should haveLength 3
        // for derivative price:
        // first element => (1*0.4 + 2*0.6)1/1
        // second element => (2*0.4 + 2*0.6)1/1
        // third element => (2*0.4 + 4*0.6)1/1

        // for the stock price:
        // 2*0.6 - (upStock*down) = 1.2
        // 3*0.6 - 1.8
        // 4*0.6 - 2.4
        
        newPrices.[0] |> should equal {
            Stock = 1.2
            Option = 1.6
            PreviousOption = 2.0
        }

        let secondNode = newPrices.[1]
        secondNode.Stock |> should (equalWithin 0.0000001) 1.8
        secondNode.Option |> should equal 2.0

        let thirdNode = newPrices.[2]
        thirdNode.Stock |> should (equalWithin 0.0000001) 2.4