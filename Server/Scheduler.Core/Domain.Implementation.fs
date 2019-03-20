namespace Scheduler.Core.Domain.Implementation

open System
open Scheduler.Core.Domain.Models
open Scheduler.Core.SimpleTypes

module Processing = 
    let private findNextBest : FindNextBest = 
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

    let private matchForType : MatchForType = 
        fun employee timeRange matchType -> 
            match matchType with
            | Full -> 
                match employee.TimeSlot.From - timeRange.From, employee.TimeSlot.To - timeRange.To with
                | (x,y) when x = TimeSpan.Zero && y = TimeSpan.Zero -> 
                    Some(employee, employee, []) 
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

    let private fillSurgeriesInternal : FillSurgeries =
        fun findNextBest matchForType getRemainder surgeries doctors nurses ->
            let rec fillSurgeries matchType surgeries aggr = 
                // TODO: for partial less, add recursive approach - probably extract to a new function
                let (employees, result) = aggr
                match surgeries with
                | [] -> aggr
                | surgery::tail -> 
                    let (employeesForDay, employees) = findNextBest matchForType matchType surgery.TimeSlot employees
                    fillSurgeries matchType tail (employees, (surgery, employeesForDay)::result)

            let getRemainingSurgeryTimes getRemainder (surgeryWithEmployees : (SurgeryWorkSlot*EmployeeWorkSlot list) list) : SurgeryWorkSlot list = 
                surgeryWithEmployees
                |> List.map (fun (s, e) -> 
                    let employeeTimes = e |> List.map (fun e -> e.TimeSlot)
                    getRemainder s.TimeSlot employeeTimes |> List.map(fun r -> { s with TimeSlot = r })
                )
                |> List.concat

            // full matching
            let (doctors, doctorResult1) = fillSurgeries Full surgeries (doctors, [])
            let (nurses, nurseResult1) = fillSurgeries Full surgeries (nurses, [])

            let remainingTimesForDoctors1 = getRemainingSurgeryTimes getRemainder doctorResult1
            let remainingTimesForNurses1 = getRemainingSurgeryTimes getRemainder nurseResult1

            // partial more matching
            let (doctors, doctorResult2) = fillSurgeries PartialMore remainingTimesForDoctors1 (doctors, [])
            let (nurses, nurseResult2) = fillSurgeries PartialMore remainingTimesForNurses1 (nurses, [])

            let remainingTimesForDoctors2 = getRemainingSurgeryTimes getRemainder doctorResult2
            let remainingTimesForNurses2 = getRemainingSurgeryTimes getRemainder nurseResult2

            // partial less matching
            let (_, doctorResult3) = fillSurgeries PartialLess remainingTimesForDoctors2 (doctors, [])
            let (_, nurseResult3) = fillSurgeries PartialLess remainingTimesForNurses2 (nurses, [])

            let finalAll = 
                doctorResult1 @ doctorResult2 @ doctorResult3 @ nurseResult1 @ nurseResult2 @ nurseResult3
                |> List.groupBy (fun (surgery, _) -> surgery)
                |> List.map (fun e -> 
                    let (surgery, surgeryEmployees) = e
                    let employees = surgeryEmployees |> List.map (fun (_,e) -> e) |> List.concat
                    { Surgery = surgery; Staff = employees }
                )
            finalAll

    let fillSurgeries = fillSurgeriesInternal findNextBest matchForType TimeRange.getRemainder

module PreProcessing =
    let private prepEmployeeInternal getRemainder (employee: Employee) : EmployeeWorkSlot list = 
       employee.WorkHours
       |> List.map(fun wh -> 
           let timeOffsForDay = employee.TimeOff |> List.filter (fun tmOff -> wh.From.Date = tmOff.From.Date)
           (wh, timeOffsForDay)
       )
       |> List.map (fun (workTime, timeOffs) -> getRemainder workTime timeOffs)
       |> List.concat
       |> List.map (fun tr -> { Name = employee.Name; Position = employee.Position; TimeSlot = tr })

    let private prepEmployee = prepEmployeeInternal TimeRange.getRemainder 

    let private prepEmployeesInternal prepEmployee (employees : Employee list) : EmployeeWorkSlot list = 
        employees
        |> List.map(fun employee -> prepEmployee employee)
        |> List.concat

    let prepEmployees = prepEmployeesInternal prepEmployee

    let prepSurgeries (surgeries : Surgery list) : SurgeryWorkSlot list = 
        surgeries 
        |> List.collect (fun s -> s.WorkHours |> List.map (fun w -> { Name = s.Name; TimeSlot = w }))

