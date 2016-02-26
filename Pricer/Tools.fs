namespace Pricer

open Microsoft.FSharp.Reflection

module Tools = 
    let caseString (x:'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
            | case, _ -> case.Name