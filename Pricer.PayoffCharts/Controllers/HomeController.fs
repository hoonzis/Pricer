namespace Pricer.PayoffCharts.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax
open FSharp.Interop.Dynamic

type HomeController() =
    inherit Controller()
    member this.Index () = 
        this.View("OptionPrices")

    member this.PayoffCharts () = this.View()
    member this.OptionPrices () = this.View()
    member this.StockAnalysis() = this.View()
    member this.About () = this.View()


