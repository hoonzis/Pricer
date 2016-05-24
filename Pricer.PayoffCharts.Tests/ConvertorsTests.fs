namespace Pricer.PayoffCharts.Tests


open System
open System.Reflection
open System.IO
open Pricer
open FsUnit
open NUnit.Framework
open Pricer.PayoffCharts.Convertors
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open FSharp.Reflection

type Result = 
        | Error
        | Success of String
        | StrangeError of String
        | SuperSuccess of String*String

[<TestFixture>]
type ConvertorsTests() = 

    [<Test>]
    member this.``test du coversion in a list`` () =
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
        let test = 4.0

        let legs = [
            StrategiesExamples.buildOptionLeg 1.0 100.0 DateTime.Now Call
            StrategiesExamples.buildOptionLeg 1.0 110.0 DateTime.Now Call 
        ]
        let json = JsonConvert.SerializeObject(legs, listserializer,duserializer,optionConverter)
        json |> should contain """"Kind":"Call"""

    [<Test>]
    member this.``test du only serialization of simple DU`` () =
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
        let test = 4.0

        let data = StrategiesExamples.buildOptionLeg 1.0 100.0 DateTime.Now Call
        let json = JsonConvert.SerializeObject(data, listserializer,duserializer,optionConverter)
        json |> should contain """"Kind":"Call"""

    [<Test>]
    member this.``other test`` () =
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
        let test = 4.0

        let optionLeg = StrategiesExamples.buildOptionLeg 1.0 100.0 DateTime.Now Call
        let cashLeg = {
            Definition = Cash {
                Price = 1.0
                Direction = 2.0
            }
            Pricing = None
        }


        let data : Leg list = [optionLeg;cashLeg]
        let json = JsonConvert.SerializeObject(data, listserializer,duserializer,optionConverter)
        json |> should contain """"Kind":"Call"""
        json |> should not' (contain "Item0")

        
    [<Test>]
    member this.``test DU in a list single type`` () =
        let stringData = """{"Stock":{"Volatility":0.5,"CurrentPrice":235,"Name":"VODAFONE","Rate":0.03},"Legs":[{Definition:{"Expiry":"2015-10-12T09:08:58.065Z","Strike":250,"Kind":"Call","Style":"European","Direction":-1},Pricing:null}],"Rate":0.03}"""
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
        let test = 4.0

        let data = StrategiesExamples.buildOptionLeg 1.0 100.0 DateTime.Now Call
        let strat = JsonConvert.DeserializeObject<Strategy>(stringData, duserializer,listserializer,optionConverter)
        strat.Legs |> should haveLength 1
        let firstLeg = strat.Legs |> Seq.head
        match firstLeg.Definition with
            | Option op -> op.Kind |> should equal Call
            | Cash cl -> failwith "should be serialized as option leg"

    [<Test>]
    member this.``two du in the list`` () =
        let stringData = """{"Stock":{"Volatility":0.5,"CurrentPrice":235,"Name":"VODAFONE","Rate":0.03},"Legs":[{Definition:{"Strike":250,"Direction":-1}},{Definition:{"Expiry":"2015-10-12T09:08:58.065Z","Strike":250,"Kind":"Call","Style":"European","Direction":-1}}],"Rate":0.03}"""
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
        let test = 4.0

        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
        let data = StrategiesExamples.buildOptionLeg 1.0 100.0 DateTime.Now Call
        let strat = JsonConvert.DeserializeObject<Strategy>(stringData,duserializer,listserializer,optionConverter)
        strat.Legs |> should haveLength 2
        let firstLeg = strat.Legs |> Seq.head
        let secondLeg = strat.Legs |> Seq.last
        match firstLeg.Definition with
            | Option op -> failwith "Should be cash leg..."
            | Cash cl -> cl.Price |> should equal 250

        match secondLeg.Definition with
            | Option op -> op.Kind |> should equal Call
            | Cash cl -> failwith "should be serialized as option leg"


    

    [<Test>]
    member this.``classic DU example`` () =
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
        
        let data = SuperSuccess ("All","IsOK")
        let json = JsonConvert.SerializeObject(data, duserializer,listserializer,optionConverter)
        json |> should not' (be Null)
        json |> should equal """{"Case":"SuperSuccess","Item1":"All","Item2":"IsOK"}"""

        let data = Success "Allright"
        let json = JsonConvert.SerializeObject(data, duserializer,listserializer,optionConverter)
        json |> should not' (be Null)
        json |> should equal """{"Case":"Success","Item1":"Allright"}"""

        let data = Error
        let json = JsonConvert.SerializeObject(data, duserializer,listserializer,optionConverter)
        json |> should not' (be Null)
        json |> should equal "\"Error\""


    [<Test>]
    member this.``classic DU multiple cases deserializatoin - into a case with tuple`` () =
        let stringData = """{"Case":"SuperSuccess","Item1":"All","Item2":"IsOK"}"""
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
 
        let superSuc = JsonConvert.DeserializeObject<Result>(stringData,duserializer,listserializer,optionConverter)
        superSuc |> should not' (be Null)
        match superSuc with
            | SuperSuccess (s1,s2) -> 
                s1|> should equal "All" 
                s2 |> should equal "IsOK"
            | _ -> failwith "Incorrect case"

    [<Test>]
    member this.``classic DU multiple cases deserializatoin - into a case with single string`` () =
        let stringData = """{"Case":"Success","Item1":"Fine"}"""
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
 
        let superSuc = JsonConvert.DeserializeObject<Result>(stringData,duserializer,listserializer,optionConverter)
        superSuc |> should not' (be Null)
        match superSuc with
            | Success s1 -> 
                s1|> should equal "Fine" 
            | _ -> failwith "Incorrect case"

    [<Test>]
    member this.``classic DU multiple cases deserializatoin - only case without any type`` () =
        let stringData = "\"Error\""
        let duserializer = new IdiomaticDuConverter()
        let listserializer = new ListConverter()
        let optionConverter = new OptionConverter()
 
        let superSuc = JsonConvert.DeserializeObject<Result>(stringData,duserializer,listserializer,optionConverter)
        superSuc |> should not' (be Null)
        superSuc |> should equal Error
       