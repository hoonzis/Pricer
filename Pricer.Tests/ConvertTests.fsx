#r "bin/Debug/Pricer.Core.dll"
#r "bin/Debug/Pricer.dll"
#r "bin/Debug/Pricer.Fabled.exe"
#load @"../packages/FSharp.Charting/lib/net45/FSharp.Charting.fsx"
#r "../packages/MathNet.Numerics/lib/net40/MathNet.Numerics.dll"

open System
open Pricer.Core
open Pricer
open Pricer.Fabled
open FSharp.Charting.ChartTypes
open FSharp.Charting
open System.Windows.Forms.DataVisualization
open System.Drawing
open MathNet.Numerics.Statistics

let stock = {
  CurrentPrice = 201.0
  Volatility = 0.25
  Rate = 0.03
}

let convert = { 
    Coupon = 0.4
    ReferencePrice = 200.0
    Direction = 1.0
    ConversionRatio = 1.0
    Maturity = DateTime.Now.AddDays(40.0)
    FaceValue = 10000.0
}

let convertStrategy = {
    Stock = stock
    Legs = [
            {
                Definition = Convertible convert
                Pricing = None
            }
    ]
    Name = "Convertible Example"
}
