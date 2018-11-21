namespace Pricer.Tests

open Pricer
open System
open System.Reflection
open System.IO
open FsUnit
open NUnit.Framework
open Pricer.Core

[<TestFixture>]
type OptionsTests() = 
    
    let mathProvider = new MathNetProvider()
    let bsPricer = new BlackScholesPricer(mathProvider)
    let fullPricer = new FullPricer()
    let payoffGenerator = new PayoffsGenerator(fullPricer)
    
    [<Test>]
    member this.``black sholes euroean call`` () =        
        let price = bsPricer.blackScholes TestData.stock TestData.europeanCall
        price.Premium |> should equal 2.8237329844670001
        price.Delta |> should equal 0.56214071717802927
        
    [<Test>]
    member this.``black sholes euroean call - volatility 0`` () =      
        let updatedStock = { TestData.stock with Volatility = 0.0 }  
        let price = bsPricer.blackScholes updatedStock TestData.europeanCall
        price.Premium |> should equal 0.85318400656242943
        // if the volatitlity is 0, then the price depends only on rate and
        price.Delta |> should equal 1.0

    [<Test>]
    member this.``black sholes euroean put`` () =        
        let price = bsPricer.blackScholes TestData.stock TestData.europeanPut
        price.Premium |> should equal 1.9705489779045706
        price.Delta |> should equal -0.43785928282197073

    [<Test>]
    member this.``black sholes quick price put from now with expiry in 30 days from now``() =
        let buyingDate = DateTime.Now.Date
        let exp = buyingDate.AddDays(30.0)
        let quickPrice = fullPricer.europeanPrice 0.03m 1.0 230m 0.05m 231m exp Put buyingDate
        quickPrice.Premium |> should equal 1.5432874726200509
        quickPrice.Delta |> should equal -0.54913311568046086

    [<Test>]
    member this.``black sholes quick price put from now with fixed dates``() =
        let buyingDate = new DateTime(2014, 1, 1)
        let exp = new DateTime(2014, 4, 1)
        let quickPrice = fullPricer.europeanPrice 0.0125m 1.0 10m 0.01m 10m exp Put buyingDate
        quickPrice.Premium |> should equal 0.0080847607807861266
        quickPrice.Delta |> should equal -0.26658074485026539
