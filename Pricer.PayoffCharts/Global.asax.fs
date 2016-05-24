namespace Pricer.PayoffCharts

open System
open System.Collections
open System.Collections.Generic
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Mvc
open System.Web.Routing
open System.Web.Optimization
open Newtonsoft.Json.Serialization
open Pricer
open Pricer.PayoffCharts.Convertors
open Pricer.MarketData

type BundleConfig() =
    static member RegisterBundles (bundles:BundleCollection) =
        bundles.Add(ScriptBundle("~/bundles/basics").Include(
                        [|
                            "~/Scripts/jquery-2.1.4.js"
                            "~/Scripts/bootstrap.js"
                            "~/Scripts/bootstrap-datepicker.js"
                        |]))

        bundles.Add(ScriptBundle("~/bundles/koext").Include(
                        [|
                            "~/Scripts/knockout-3.3.0.debug.js"
                            "~/Scripts/knockout.validation.js"
                            "~/Scripts/d3/d3.js"
                            "~/Scripts/KoExtensions.js"
                        |]))
                      
        bundles.Add(ScriptBundle("~/bundles/vms").Include(
                        [|
                            "~/ViewModels/Tools.js"
                            "~/ViewModels/ComboViewModel.js"
                            "~/ViewModels/LegViewModel.js"
                            "~/ViewModels/StockViewModel.js"
                            "~/ViewModels/StockInfoViewModel.js"
                            "~/ViewModels/StrategyViewModel.js"
                            "~/ViewModels/StrategyListViewModel.js"
                            "~/ViewModels/OptionAnalysisViewModel.js"
                            "~/ViewModels/StockAnalysisViewModel.js"
                        |]))

        bundles.Add(StyleBundle("~/Content/css").Include(
                       [|
                          "~/Content/bootstrap.css"
                          "~/Content/site.css"
                          "~/Content/kostyles.css"
                      |]))

/// Route for ASP.NET MVC applications
type Route = { 
    controller : string
    action : string
    id : UrlParameter }

type HttpRoute = {
    controller : string
    id : RouteParameter }

type Global() =
    inherit System.Web.HttpApplication() 

    static member RegisterWebApi(config: HttpConfiguration) =
        // Configure routing
        config.MapHttpAttributeRoutes()
        config.Routes.MapHttpRoute(
            "DefaultApi", // Route name
            "api/{controller}/{id}", // URL with parameters
            { controller = "{controller}"; id = RouteParameter.Optional } // Parameter defaults
        ) |> ignore

        // Configure serialization
        config.Formatters.XmlFormatter.UseXmlSerializer <- true
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()
        config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new OptionConverter())
        config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new ListConverter())
        config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new IdiomaticDuConverter())

        // Additional Web API settings

    static member RegisterFilters(filters: GlobalFilterCollection) =
        filters.Add(new HandleErrorAttribute())

    static member RegisterRoutes(routes:RouteCollection) =
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
        routes.MapRoute(
            "Default", // Route name
            "{controller}/{action}/{id}", // URL with parameters
            { controller = "Home"; action = "Index"; id = UrlParameter.Optional } // Parameter defaults
        ) |> ignore

    member x.Application_Start() =
        MarketProviders.authKey <- "&auth_token=VP8Yryjzdv-3kxtyH5Uc"
        MarketProviders.loadAllStocks [LSE;EURONEXT]
        let test = MarketProviders.stockLookup "TEST"
        AreaRegistration.RegisterAllAreas()
        GlobalConfiguration.Configure(Action<_> Global.RegisterWebApi)
        Global.RegisterFilters(GlobalFilters.Filters)
        Global.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles BundleTable.Bundles