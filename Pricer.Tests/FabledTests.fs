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
