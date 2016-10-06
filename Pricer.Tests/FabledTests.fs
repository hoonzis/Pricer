namespace Pricer.Tests

open Pricer
open System
open FsUnit
open NUnit.Framework
open Pricer.Core
open Pricer.Fabled


[<TestFixture>]
type FabledTests() = 
        
    [<Test>]
    member this.``parse date`` () =
        let value  = "2016-01-20"
        let date = value |> Tools.parseDate 
        date.Year |> should equal 2016
        date.Month |> should equal 1
        date.Day |> should equal 20

    [<Test>]
    member this.``date to string`` () =
        let date = new DateTime(2016, 1, 2)
        let value = date |> Tools.toDate
        value |> should equal "2016-01-02"

    [<Test>]
    member this.``test simple pricer - european call BS which is not using CDF but ERF`` () =
        let estimationPricer = new BlackScholesPricer(new SimpleMathProvider())      
        let bsPricer = new BlackScholesPricer(new MathNetProvider())
        let price = bsPricer.blackScholes TestData.stock TestData.europeanCall
        let estimationPrice = estimationPricer.blackScholes TestData.stock TestData.europeanCall
        let diff = abs (estimationPrice.Premium - price.Premium)
        diff |> should be (lessThan 0.00001)

    [<Test>]
    member this.``compare CDF and ERF`` () =
        let simpleProvider = new SimpleMathProvider() :> IMathProvider
        let realProvider = new MathNetProvider() :> IMathProvider

        [1.0..10.0] |> List.map (fun v-> 
            let erf = simpleProvider.cdf v
            let cdf = realProvider.cdf v
            abs (erf - cdf) |> should be (lessThan 0.00002)
        ) |> ignore