open System

#r @"c:\code\FSharp\Scheduler\server\packages\Newtonsoft.Json\lib\netstandard2.0\Newtonsoft.Json.dll"
#r @"c:\code\FSharp\Scheduler\server\packages\NETStandard.Library\build\netstandard2.0\ref\netstandard.dll"
#r @"c:\code\FSharp\Scheduler\Server\packages\NodaTime\lib\netstandard2.0\NodaTime.dll"

open NodaTime

type TimeRange = {
    From : DateTime
    To : DateTime
}

let testFirst = { 
    From = DateTime(2018, 1, 1, 8, 0, 0)
    To = DateTime(2018, 1, 1, 17, 0, 0)
}

let testSecond = { 
    From = DateTime(2018, 1, 1, 10, 0, 0)
    To = DateTime(2018, 1, 1, 12, 0, 0)
}

let expected = [
    { 
        From = DateTime(2018, 1, 1, 8, 0, 0)
        To = DateTime(2018, 1, 1, 9, 59, 0)
    }
    { 
        From = DateTime(2018, 1, 1, 12, 1, 0)
        To = DateTime(2018, 1, 1, 17, 0, 0)
    }
]

// start with from hour
// see if employee can work during that hour datetime.hour
    

let getTimeRangeRemainder first second : TimeRange list = 
    let resultFirstFrom = first.From
    let f = first.From.Add(second.From - first.From)
    []    


let first = Period.Between(LocalDateTime.FromDateTime(testFirst.From), LocalDateTime.FromDateTime(testSecond.From))
let second = Period.Between(LocalDateTime.FromDateTime(testSecond.To), LocalDateTime.FromDateTime(testFirst.To))
let result = first - second
result.ToDuration()

//let interval1 = DateInterval(LocalDate.FromDateTime(testFirst.From), LocalDate.FromDateTime(testFirst.To))
//let interval2 = DateInterval(LocalDate.FromDateTime(testSecond.From), LocalDate.FromDateTime(testSecond.To))


//let interval3 = Interval(Instant.FromDateTimeOffset(testFirst.From), Instant.FromDateTimeOffset(testFirst.To))
//let interval4 = Interval(Instant.FromDateTimeOffset(testSecond.From), Instant.FromDateTimeOffset(testSecond.To))
//interval3.

//let interval5 = interval3.


let a = [1;2;3]
let b = [5]
5::b
