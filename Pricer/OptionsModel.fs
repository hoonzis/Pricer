namespace Pricer.Core

open System

type OptionKind =
    | Call
    | Put
    member this.Name = match this with | Call -> "Call" | Put -> "Put"

type OptionStyle =
    | American
    | European

type OptionLeg =
    {
        Direction : float
        Strike : float
        Expiry : DateTime
        Kind : OptionKind
        Style: OptionStyle
        PurchaseDate: DateTime
    }
    member this.BuyVsSell = if this.Direction < 0.0 then "Sell" else "Buy"
    member this.TimeToExpiry = (float (this.Expiry - this.PurchaseDate).Days)/365.0
    member this.Name = sprintf "%s %s %.2f" this.BuyVsSell this.Kind.Name this.Strike

type CashLeg = {
    Direction: float
    Price:float
}

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

type LegData = {
    Leg:Leg
    LegData: float*float list
}

type SingleLine =  (float*float) list

type LegsData = Leg * SingleLine

type StrategyData = 
    | SingleYear of SingleLine * ((Leg * SingleLine) seq)
    | MultiYear of SingleLine seq

module BasicOptions = 
    let optionValue option stockPrice =
        match option.Kind with
                | Call -> max 0.0 (stockPrice - option.Strike)
                | Put -> max 0.0 (option.Strike - stockPrice)

    let buildLeg kind strike direction style expiry buyingDate =
        {
            Strike=strike
            Kind = Call
            Direction = direction
            Expiry = expiry
            Style = European
            PurchaseDate = buyingDate
        }


type IMathProvider = 
    abstract member cdf: float -> float

type IPricer =
    abstract member priceOption: StockInfo -> OptionLeg -> Pricing
    abstract member priceCash: CashLeg -> Pricing
    abstract member priceConvert: StockInfo -> ConvertibleLeg -> Pricing