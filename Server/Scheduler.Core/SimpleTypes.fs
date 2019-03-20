namespace Scheduler.Core.SimpleTypes

open System

type TimeRange = {
    From : DateTime
    To : DateTime
}

type GetIntervalDiff = TimeRange -> TimeRange list -> TimeRange list
module TimeRange = 
    let getRemainder : GetIntervalDiff = 
        fun a b -> 
            let getHourListBetween (a : DateTime) (b : DateTime) = 
                seq { for i in a.Hour .. b.Hour -> i } |> Seq.toList

            let busyHours = 
                b
                |> List.map (fun item -> getHourListBetween item.From item.To)
                |> List.concat

            let availableHours = 
                getHourListBetween a.From a.To 
                |> List.except(busyHours)
                |> List.map(fun i -> [i+1; i; i-1])
                |> List.concat
                |> List.filter (fun i -> (i >= a.From.Hour) && (i <= a.To.Hour))
                |> List.distinct
                |> List.sort

            let buildResult (initDate : DateTime) (sortedHours:int list): TimeRange list = 
                let rec buildResult' hours aggr = 
                    match hours with
                    | [] -> aggr
                    | head::[] -> 
                        let (currentLatestDate:DateTime, previousHour, resultList) = aggr
                        let newTo = currentLatestDate.AddHours(float(head - currentLatestDate.Hour))
                        let result = { From = currentLatestDate; To = newTo } :: resultList
                        (newTo, head, result)
                    | head::tail -> 
                        let (currentLatestDate:DateTime, previousHour, resultList) = aggr
                        match head - previousHour with
                        | 1 -> buildResult' tail (currentLatestDate, head, resultList)
                        | diff -> 
                            let newTo = currentLatestDate.AddHours(float(previousHour - currentLatestDate.Hour))
                            let result = { From = currentLatestDate; To = newTo } :: resultList
                            buildResult' tail (newTo.AddHours(float(diff)), head, result)

                let head = sortedHours |> List.head
                let tail = sortedHours |> List.tail
                let startDate head (initDate : DateTime) = 
                    if head = initDate.Hour then
                        initDate
                    else
                        initDate.AddHours(float(head - initDate.Hour))

                let (_, _, result) = buildResult' tail (startDate head initDate, head, [])
                result
        
            match availableHours with
            | [] -> []
            | _ -> buildResult a.From availableHours


