open System

#r @"c:\code\FSharp\Scheduler\server\packages\Newtonsoft.Json\lib\netstandard2.0\Newtonsoft.Json.dll"
#r @"c:\code\FSharp\Scheduler\server\packages\NETStandard.Library\build\netstandard2.0\ref\netstandard.dll"
#r @"c:\code\FSharp\Scheduler\Server\packages\NodaTime\lib\netstandard2.0\NodaTime.dll"

open NodaTime


let a = [1;2;3;4]
let b = [2;3]

a |> List.except(b)