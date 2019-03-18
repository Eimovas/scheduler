namespace Scheduler.Core.Domain.Implementation3

open System

type TimeRange = {
    From : DateTime
    To : DateTime
}

type EmployeeWorkSlot = {
    Name : string
    //Position : Position
    TimeSlot : TimeRange
}
type MatchType = 
| Full
| PartialMore
| PartialLess

type MatchForType = EmployeeWorkSlot -> TimeRange -> MatchType -> (EmployeeWorkSlot * EmployeeWorkSlot * EmployeeWorkSlot list) option 
type FindNextBest = MatchForType -> MatchType -> TimeRange -> EmployeeWorkSlot list -> (EmployeeWorkSlot list * EmployeeWorkSlot list)

module Implementation3 = 
    let findNextBest : FindNextBest = 
        fun matchForType matchType timeRange employeeList -> 

            let rec findNextBest' employees timeRange matchType =           
                match employees with
                | [] -> None
                | head::tail ->
                    let singleMatch = matchForType head timeRange matchType
                    match singleMatch with
                    | None -> findNextBest' tail timeRange matchType
                    | Some (x,y,z) -> Some (x,y,z)
        
            let resultOption = findNextBest' employeeList timeRange matchType
            let (result, newEmployeeList) = 
                match resultOption with
                | None -> [], employeeList
                | Some (x,y,z) -> 
                    let updatedList = employeeList |> List.filter (fun row -> row <> y)
                    let updatedList = updatedList @ z
                    [x],updatedList

            result, newEmployeeList

    let matchForType : MatchForType = 
        fun employee timeRange matchType -> 
            match matchType with
            | Full -> 
                match employee.TimeSlot.From - timeRange.From, employee.TimeSlot.To - timeRange.To with
                | (x,y) when x = TimeSpan.Zero && y = TimeSpan.Zero -> 
                    Some(employee, employee, []) // result * what to remove * what to add
                | _ -> None
            | PartialMore -> 
                match employee.TimeSlot.From - timeRange.From, employee.TimeSlot.To - timeRange.To with
                | (x,y) when x < TimeSpan.Zero && y = TimeSpan.Zero -> 
                    let remainder = { From = employee.TimeSlot.From; To = timeRange.From }
                    Some({ employee with TimeSlot = timeRange }, employee, [{ employee with TimeSlot = remainder }])
                | (x,y) when x = TimeSpan.Zero && y > TimeSpan.Zero -> 
                    let remainder = { From = timeRange.To; To = employee.TimeSlot.To }
                    Some({ employee with TimeSlot = timeRange }, employee, [{ employee with TimeSlot = remainder }])
                | (x,y) when x < TimeSpan.Zero && y > TimeSpan.Zero -> 
                    let remainder1 = { From = employee.TimeSlot.From; To = timeRange.From }
                    let remainder2 = { From = timeRange.To; To = employee.TimeSlot.To }
                    Some({ employee with TimeSlot = timeRange }, employee, [{ employee with TimeSlot = remainder1}; { employee with TimeSlot = remainder2 }])
                | _ -> None
            | PartialLess -> 
                match employee.TimeSlot.From - timeRange.From, employee.TimeSlot.To - timeRange.To with
                | (x,y) when x > TimeSpan.Zero && y = TimeSpan.Zero -> 
                    let actual = { From = employee.TimeSlot.From ; To = timeRange.To }
                    Some({ employee with TimeSlot = actual }, employee, [])
                | (x,y) when x = TimeSpan.Zero && y < TimeSpan.Zero -> 
                    let actual = { From = timeRange.From ; To = employee.TimeSlot.To }
                    Some({ employee with TimeSlot = actual }, employee, [])
                | (x,y) when x > TimeSpan.Zero && y < TimeSpan.Zero -> 
                    Some(employee, employee, [])
                | _ -> None

    let surgeryTimeRange = { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
    let employees : EmployeeWorkSlot list= [
        //{ Name = "Algis"; TimeSlot = { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }}
        { Name = "Algis1"; TimeSlot = { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 12, 0, 0) }}
        { Name = "Algis2"; TimeSlot = { From = DateTime(2018, 1, 1, 12, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }}
        { Name = "Algis3"; TimeSlot = { From = DateTime(2018, 1, 1, 7, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }}
    ]


    let result = findNextBest matchForType Full surgeryTimeRange employees 

    // this seems to work now - gives a best matching employee for the surgery time slot and gives back the updated employee list
    // need to ->
    // iterate surgeries
    // on first iteration, match full, second - match partialMore, and after ->
    // match partialLess and recurse until returns anything
    // 