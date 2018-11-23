namespace Pricer.Core

open System

module Transforms = 
    let directionToString direction = if direction < 0.0 then "Sell" else "Buy"
    let stringToDirection direction = if direction = "Sell" then -1.0 else 1.0

type OptionKind =
    | Call
    | Put
    override x.ToString() = match x with | Call -> "Call" | _ -> "Put"

type OptionStyle =
    | American
    | European
    override x.ToString() = match x with | American -> "American" | _ -> "European"



type OptionLeg =
    {
        Direction : float
        Strike : float
        Expiry : DateTime
        Kind : OptionKind
        Style: OptionStyle
        PurchaseDate: DateTime
    }
    member this.BuyVsSell = this.Direction |> Transforms.directionToString
    member this.TimeToExpiry = (float (this.Expiry - this.PurchaseDate).Days)/365.0
    member this.Name = sprintf "%s %s %.2f" this.BuyVsSell (this.Kind.ToString()) this.Strike

type CashLeg = 
    {
        Direction: float
        Price:float
    }
    member this.BuyVsSell = this.Direction |> Transforms.directionToString

type ConvertibleLeg = {
    Direction: float
    Coupon: float
    ConversionRatio: float
    Maturity: DateTime
    FaceValue: float
    ReferencePrice: float
}

type LegInfo =
    | Cash of CashLeg
    | Option of OptionLeg
    | Convertible of ConvertibleLeg
    member this.Name = match this with
                            | Cash cl -> "Cash"
                            | Option ol -> ol.Name
                            | Convertible convert -> sprintf "Convert %f" convert.FaceValue
type Pricing = {
    Delta: float
    Premium: float
}

type Leg = {
    Definition:LegInfo
    Pricing:Pricing option
}

type Strategy = {
    Stock : StockInfo
    Name : String
    Legs: Leg list
}

module BasicOptions = 
    let optionValue option stockPrice =
        match option.Kind with
                | Call -> max 0.0 (stockPrice - option.Strike)
                | Put -> max 0.0 (option.Strike - stockPrice)

    let buildLeg kind strike direction style expiry buyingDate =
        {
            Strike=strike
            Kind = kind
            Direction = direction
            Expiry = expiry
            Style = style
            PurchaseDate = buyingDate
        }


type IMathProvider = 
    abstract member cdf: float -> float

type IPricer =
    abstract member priceOption: StockInfo -> OptionLeg -> Pricing
    abstract member priceCash: CashLeg -> Pricing
    abstract member priceConvert: StockInfo -> ConvertibleLeg -> Pricing
