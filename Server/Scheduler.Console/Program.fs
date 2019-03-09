// Learn more about F# at http://fsharp.org

open System
open Scheduler.Core.Helpers
open Scheduler.Core.Domain

[<EntryPoint>]
let main argv =
    let setup = SetupInput.load()
    let result = Operations.distributionWorkflow setup Operations.calculateDistribution Operations.calculateTimeFrame

    Helpers.printDistribution result
    
    Console.ReadLine() |> ignore

    0 // return an integer exit code
