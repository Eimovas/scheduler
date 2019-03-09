open System

#r @"c:\code\FSharp\Scheduler\packages\Newtonsoft.Json\lib\netstandard2.0\Newtonsoft.Json.dll"
#r @"c:\code\FSharp\Scheduler\packages\NETStandard.Library\build\netstandard2.0\ref\netstandard.dll"

open Newtonsoft.Json

type TimeRange = {
    From : DateTime
    To : DateTime
}

type PairedTime<'a> = {
    Pair : 'a
    TimeRange : TimeRange
}

type Surgery = {
    Name : string
    WorkTimes : TimeRange list
}

let paired = 
    {
        TimeRange = { From = DateTime.Now; To = DateTime.Now.AddDays(1.0) }
        Pair = {
            Name = "Surgery 1"
            WorkTimes = [
                { From = DateTime.Now; To = DateTime.Now.AddDays(2.0) }
                { From = DateTime.Now; To = DateTime.Now.AddDays(3.0) }
            ]
        }
    }

let result = JsonConvert.SerializeObject paired
