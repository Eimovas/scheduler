namespace Scheduler.Core.Domain.Implementation2

open Scheduler.Core.Helpers
open System
open Scheduler.Core.Domain

// this represents a day single day range
type TimeRange = {
    From : DateTime
    To : DateTime
}

type Surgery = {
    Name : string
    WorkTimes : TimeRange list
}

type Position = 
| Dentist
| Nurse

type UnvalidatedEmployee = {
    Name : string
    Position : Position
    WorkHours : TimeRange list // time off range cannot be larger than work time range for the same day
    TimeOff: TimeRange list
    //HourLimitPerWeek : decimal
}

type ValidatedEmployee = {
    Name : string
    Position : Position
    WorkTimes : TimeRange list
    TimeOff: TimeRange list
}

type EmployeeWorkSlot = {
    Name : string
    Position : Position
    TimeSlot : TimeRange
}

type SurgeryWorkSlot = {
    Name : string
    TimeSlot : TimeRange
}

type TimedWorkSlot = {
    Surgery : SurgeryWorkSlot
    Employees : EmployeeWorkSlot option list
}

module Implementation2 = 
    open System.Threading

    let some = 1

    // there's s surgeries
    // there's n nurses
    // there's d doctors

    // surgery is the one that needs to be filled with doc*nurse
    // i could do the matching in two or three phases
    // #00- vaildate inputs -> validate integrity of data -> 1. employee hour limit; 2. employee work times matching time offs; 3.
    // #0- flatten all lists -> there should be 3 lists in total -> nurse*time list; doctor*time list; surgery*time list;
    // #0- prepare employee lists and use time off requests to filter out times they can't work.
    // #1- match employees who can work full time the practice needs and ignore all partials -> just leave them blanks -> i still need to update employee times if they could work more
    // #1.1- repeat the step until no matches against full employees is found
    // #2- filter unfilled surgery days and create a new list of it
    // #2.1- iterate this list and do a partial matching on employees - include the ones who can work less now
    // #2.2- repeat #2.1 until no matches left
    // #3- merge lists produced in #1 and #2 -> this is the result
    // #3.1- keep the lists of remaining times for surgeries and remaining times for employees -> this is part of the result
    
    let setup = SetupInput.load()
        

    let getTimeRangeRemainder first second : TimeRange list = 
        match first=second with
        | true -> [first]
        | false -> 
            [
                { From = first.From; To = second.From}
                { From = second.To; To = first.To}
            ]

    let calculateTimeRangeListDiff getTimeRangeRemainder first second : TimeRange list = 
        let relevantDays = 
            second
            |> List.fold (fun sum s -> 
                let matched = 
                    first 
                    |> List.filter (fun f -> s.From.Date >= f.From.Date  && s.To.Date <= f.To.Date)
                    |> List.map(fun m -> (m,s))
                sum@matched
                ) []
        let result = 
            relevantDays 
            |> List.map (fun (m,s) -> getTimeRangeRemainder m s)
            |> List.collect (fun m -> m)
        result

    let buildEmployeeWorkTimes getTimeRangeRemainder calculateTimeRangeListDiff (employee: ValidatedEmployee) : EmployeeWorkSlot list = 
        calculateTimeRangeListDiff getTimeRangeRemainder employee.WorkTimes employee.TimeOff
        |> List.map (fun a -> { Name = employee.Name; Position = employee.Position; TimeSlot = a})        

    let prepEmployees buildEmployeeWorkTimes (employees : ValidatedEmployee list) : EmployeeWorkSlot list = 
        employees 
        |> List.map (fun e -> buildEmployeeWorkTimes e)
        |> List.collect (fun e -> e)

    let prepSurgeries (surgeries : Surgery list) : SurgeryWorkSlot list = 
        surgeries
        |> List.collect (fun s -> s.WorkTimes |> List.map (fun w -> { Name = s.Name; TimeSlot = w }))



    //let employees : ValidatedEmployee list = [
    //    {   Name = "Algis"
    //        Position = Nurse
    //        WorkTimes = [
    //            { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
    //            { From = DateTime(2018, 1, 2, 8, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
    //            { From = DateTime(2018, 1, 3, 8, 0, 0); To = DateTime(2018, 1, 3, 17, 0, 0)}
    //        ]
    //        TimeOff = [
    //            { From = DateTime(2018, 1, 2, 12, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
    //        ]   
    //    }
    //    {   Name = "Jonas"
    //        Position = Dentist
    //        WorkTimes = [
    //            { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
    //            { From = DateTime(2018, 1, 2, 8, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
    //            { From = DateTime(2018, 1, 4, 8, 0, 0); To = DateTime(2018, 1, 4, 17, 0, 0) }
    //        ]
    //        TimeOff = [
    //            { From = DateTime(2018, 1, 2, 12, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
    //        ]   
    //    }
    //]

    //let result = prepEmployees (buildEmployeeWorkTimes getTimeRangeRemainder calculateTimeRangeListDiff) employees


    type InterimState = {
        TimedSlots : TimedWorkSlot list
        Nurses : EmployeeWorkSlot list
        Doctors : EmployeeWorkSlot list
    }

    // #1
    let rec matchFullTimers (doctors: EmployeeWorkSlot list) (nurses: EmployeeWorkSlot list) (surgeries : SurgeryWorkSlot list) init : InterimState = 
        let tryMatchEmployee (surgery : SurgeryWorkSlot) (list : EmployeeWorkSlot list) = 
            list |> List.tryFind(fun d -> d.TimeSlot.From = surgery.TimeSlot.From && d.TimeSlot.To = surgery.TimeSlot.To)

        let excludeEmployee (employee : EmployeeWorkSlot option) (list : EmployeeWorkSlot list) = 
            match employee with
            | Some x -> list |> List.filter(fun d -> d.TimeSlot <> x.TimeSlot)
            | None -> list

        match surgeries with
            | [] -> init
            | head::[] -> 
                let doctor = doctors |> tryMatchEmployee head
                let nurse = nurses |> tryMatchEmployee head
                { Nurses = nurses; Doctors = doctors; TimedSlots =  { Surgery = head; Employees = [nurse;doctor] }::init.TimedSlots}
            | head::tail -> 
                let doctor = doctors |> tryMatchEmployee head
                let nurse = nurses |> tryMatchEmployee head
                let doctors = doctors |> excludeEmployee doctor 
                let nurses = nurses |> excludeEmployee nurse 
                matchFullTimers doctors nurses tail ({ Nurses = nurses; Doctors = doctors; TimedSlots =  { Surgery = head; Employees = [nurse;doctor] }::init.TimedSlots})
    
    // #2
    let getRemainingSurgeries interimState = interimState.TimedSlots |> List.filter (fun t -> t.Employees |> List.choose (fun e -> e) |> List.length < 2 )
    
    // #2.1
    //let splitTimesIntoRange first second : TimeRange list = 
        // first is the master
        // second is trying to match items

        // first : 8:00 - 17:00
        // second : 8:00 - 12:00
        // result : [ 8:00 - 12:00 ; 12:00 - 17:00]

        // first : 8:00 - 17:00
        // second : 9:00 - 12:00
        // result : [ 8:00 - 9:00 ; 9:00 - 12:00 ; 12:00 - 17:00]

        // first : 8:00 - 17:00
        // second : 7:00 - 12:00
        // result : [ 7:00 - 8:00 ; 8:00 - 12:00 ; 12:00 - 17:00]

        // 


    // PROTOTYPE
    // create a day map -> build from employee work times
    // each day has a method to ask for best employee times and returns a list of times + new instance of day
    // 
    // 

    type MatchType = 
    | Full
    | PartialMore
    | PartialLess

    type MatchForType = EmployeeWorkSlot -> TimeRange -> MatchType -> (EmployeeWorkSlot * EmployeeWorkSlot * EmployeeWorkSlot list) option 
    type FindNextBest = MatchForType -> MatchType -> TimeRange -> EmployeeWorkSlot list -> (EmployeeWorkSlot list * EmployeeWorkSlot list)

    let findNextBest : FindNextBest = 
        fun matchForType matchType timeRange employeeList -> 

            let rec iterateEmployeesForResult employees timeRange matchType =           
                match employees with
                | [] -> None
                | head::tail ->
                    let singleMatch = matchForType head timeRange matchType
                    match singleMatch with
                    | None -> iterateEmployeesForResult tail timeRange matchType
                    | Some (x,y,z) -> Some (x,y,z)
        
            let resultOption = iterateEmployeesForResult employeeList timeRange matchType
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



        //match employee.TimeSlot.From - timeRange.From, employee.TimeSlot.To - timeRange.To with
        //    | (x,y) when x = TimeSpan.Zero && y = TimeSpan.Zero -> 
        //        Some(employee, employee, []) // result * what to remove * what to add
        //    | (x,y) when x < TimeSpan.Zero && y = TimeSpan.Zero -> 
        //        let remainder = { From = employee.TimeSlot.From; To = timeRange.From }
        //        Some({ employee with TimeSlot = timeRange }, employee, [{ employee with TimeSlot = remainder }])
        //    | (x,y) when x = TimeSpan.Zero && y > TimeSpan.Zero -> 
        //        let remainder = { From = timeRange.To; To = employee.TimeSlot.To }
        //        Some({ employee with TimeSlot = timeRange }, employee, [{ employee with TimeSlot = remainder }])
        //    | (x,y) when x < TimeSpan.Zero && y > TimeSpan.Zero -> 
        //        let remainder1 = { From = employee.TimeSlot.From; To = timeRange.From }
        //        let remainder2 = { From = timeRange.To; To = employee.TimeSlot.To }
        //        Some({ employee with TimeSlot = timeRange }, employee, [{ employee with TimeSlot = remainder1}; { employee with TimeSlot = remainder2 }])
        //    | (x,y) when x > TimeSpan.Zero && y = TimeSpan.Zero -> 
        //       findNextBestual = { From = employee.TimeSlot.From ; To = timeRange.To }
        //        Some({ employee with TimeSlot = actual }, employee, [])
        //    | (x,y) when x = TimeSpan.Zero && y < TimeSpan.Zero -> 
        //        let actual = { From = timeRange.From ; To = employee.TimeSlot.To }
        //        Some({ employee with TimeSlot = actual }, employee, [])
        //    | (x,y) when x > TimeSpan.Zero && y < TimeSpan.Zero -> 
        //        Some(employee, employee, [])
        //    | _ -> None
        // match full timers first
        // otherwise
        // if anyone can work more -> use them
        // leave their remaining available bit in the list
        // if no one can work more, find someone who can work most (rec) until list is filled
        // otherwise return empty and same list
        

    type Slot = 
    | OneHour of TimeRange list
    | TwoHour of TimeRange list
    | Half of TimeRange list
    | Full of TimeRange list

    type EmployeePrototypeProcessed = {
        Name : string
        Slots : Slot list
        WorkTime : TimeRange list
        // other props
    }
    
    let rec fillSlot (hoursToAdd : float) (currentTime : DateTime) (endTime : DateTime) sum : TimeRange list= 
        match currentTime >= endTime with
        | true -> sum
        | false -> 
            let nextTo = currentTime.AddHours(hoursToAdd)
            let sum = { From = currentTime; To = nextTo } :: sum
            fillSlot hoursToAdd nextTo endTime sum

    let buildSlot fillSlot (slot : Slot) (timeRange: TimeRange) : Slot = 
        match slot with
        | OneHour _ -> OneHour (fillSlot 1.0 timeRange.From timeRange.To [])
        | TwoHour _ -> TwoHour (fillSlot 2.0 timeRange.From timeRange.To [])
        | Half _ -> 
            let diff = (timeRange.To - timeRange.From).TotalHours
            Half (fillSlot (diff / 2.0) timeRange.From timeRange.To [])
        | Full _ -> 
            let diff = (timeRange.To - timeRange.From).TotalHours
            Full (fillSlot diff timeRange.From timeRange.To [])

    let timeRange = { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0)}
    let result = buildSlot fillSlot (OneHour([])) timeRange 
    let result1 = buildSlot fillSlot (TwoHour([])) timeRange 
    let result2 = buildSlot fillSlot (Half([])) timeRange 
    let result3 = buildSlot fillSlot (Full([])) timeRange 

    

    // for each item in surgeries
    // ask for full items
    // if full not found, ask for half
    // if half not found, ask for two hour
    // if two hour not found, ask for one hour

    let getMatches timeSlot employees : EmployeeWorkSlot list = 
        let rec getMatchesInternal employees aggr = 
            match employees with
            | [] -> aggr
            | head::tail ->
                head.

    let fill getMatches updateEmployees nurses doctors surgeries = 
        let rec fillInternal nurses doctors surgeries aggr = 
            match surgeries with
            | [] -> aggr
            | surgery::tail -> 
                let nurseMatches = nurses |> getMatches surgery.TimeSlot // gets best available time slots to fill the day
                let doctorMatches = doctors |> getMatches surgery.TimeSlot

                let nurses = nurses |> updateEmployees nurseMatches // filters out the returned items so they get removed from list
                let doctors = doctors |> updateEmployees doctorMatches

                fillInternal nurses doctors tail ({ Surgery = surgery; Employees = nurseMatches@doctorMatches } :: aggr)

        fillInternal nurses doctors surgeries []


            

            