namespace Scheduler.Core.Domain

open Scheduler.Core.Helpers
open System

module private Implementation =
    type MatchType = 
    | Empty
    | Partial of PairedTime<Employee>
    | Full of PairedTime<Employee>

    let tryFindForTimeSlot timeSlot (list : PairedTime<Employee> list) : MatchType = 
        let itemOption = list |> List.tryFind(fun r -> r.TimeRange.From = timeSlot.From && r.TimeRange.To = timeSlot.To)
        match itemOption with
        | Some x -> Full x
        | None -> 
            let partial = list |> List.tryFind (fun r -> 
                let time = r.TimeRange
                (time.From >= timeSlot.From && time.To <= timeSlot.To) ||
                (time.From >= timeSlot.From && time.To >= timeSlot.To) ||
                (time.From <= timeSlot.From && time.To <= timeSlot.To) ||
                (time.From <= timeSlot.From && time.To >= timeSlot.To)
            )
            match partial with
            | Some x -> Partial x
            | None -> Empty        

    let pairEmployeesWithTimes list : PairedTime<Employee> list= 
        list 
        |> List.map (fun current -> current.WorkTimes |> List.map(fun w -> PairedTime.create current w))
        |> List.flatten

    let pairSurgeriesWithTimes (list : Surgery list) : PairedTime<Surgery> list= 
        list 
        |> List.map (fun current -> current.WorkTimes |> List.map(fun w -> PairedTime.create current w))
        |> List.flatten

    let buildEmployeeFromMatch employeeMatch currentTimeSlot : PairedTime<Employee> option=
        let zero = TimeSpan.Zero
        match employeeMatch with 
        | Empty -> None
        | Full e -> Some e
        | Partial pairedEmployee  -> 
            let result = 
                match currentTimeSlot.From - pairedEmployee.TimeRange.From, currentTimeSlot.To - pairedEmployee.TimeRange.To with
                | f,t when f = zero && t < zero -> Some currentTimeSlot
                | f,t when f > zero && t < zero -> Some currentTimeSlot
                | f,t when f > zero && t = zero -> Some currentTimeSlot

                | f,t when f < zero && t = zero -> Some pairedEmployee.TimeRange
                | f,t when f = zero && t > zero -> Some pairedEmployee.TimeRange
                | f,t when f < zero && t > zero -> Some pairedEmployee.TimeRange
                | _ -> None

            match result with
            | Some r -> Some (PairedTime.create pairedEmployee.Pair r)
            | None -> None

    let buildUpdatedEmployeeList employeeMatch currentTimeSlot list : PairedTime<Employee> list = 
        match employeeMatch with
        | Full e -> list |> List.filter (fun x -> x <> e)
        | Empty -> list
        | Partial pairedEmployee -> 
            let updated = 
                let zero = TimeSpan.Zero
                match currentTimeSlot.From - pairedEmployee.TimeRange.From, currentTimeSlot.To - pairedEmployee.TimeRange.To with
                | f,t when f = zero && t < zero -> 
                    let newEmployeeFromTime = pairedEmployee.TimeRange.From.Add(currentTimeSlot.To - currentTimeSlot.From)
                    let remainingEmployeeTime = {From = newEmployeeFromTime; To = pairedEmployee.TimeRange.To}
                    Some (PairedTime.create pairedEmployee.Pair remainingEmployeeTime)
                | f,t when f > zero && t < zero -> 
                    let newEmployeeToTime = pairedEmployee.TimeRange.From.Add(-(currentTimeSlot.To - currentTimeSlot.From))
                    let newEmployeeFromTime = pairedEmployee.TimeRange.From.Add(currentTimeSlot.To - currentTimeSlot.From)
                    let remainingEmployeeTime = {From = newEmployeeFromTime; To = newEmployeeToTime}    
                    Some (PairedTime.create pairedEmployee.Pair remainingEmployeeTime)
                | f,t when f > zero && t = zero -> 
                    let newEmployeeToTime = pairedEmployee.TimeRange.From.Add(-(currentTimeSlot.To - currentTimeSlot.From))
                    let remainingEmployeeTime = {From = pairedEmployee.TimeRange.From; To = newEmployeeToTime}    
                    Some (PairedTime.create pairedEmployee.Pair remainingEmployeeTime)
                | f,t when f < zero && t = zero -> None
                | f,t when f = zero && t > zero -> None
                | f,t when f < zero && t > zero -> None

            let filteredList = list |> List.filter (fun x -> x <> pairedEmployee)

            match updated with
            | Some u -> u::filteredList
            | None -> filteredList

module Operations = 
    let calculateDistribution : CalculateDistribution = 
        fun employees surgeries -> 
            let nurses = employees |> List.filter (fun e -> e.Specialization = Nurse)
            let doctors = employees |> List.filter (fun e -> e.Specialization <> Nurse)

            let doctorsTimeMap = doctors |> Implementation.pairEmployeesWithTimes
            let nursesTimeMap = nurses |> Implementation.pairEmployeesWithTimes
            let surgeryTimeMap = surgeries |> Implementation.pairSurgeriesWithTimes

            let (pairedSlots, _, _) = 
                surgeryTimeMap
                |> List.fold (fun aggr currentSurgery -> 
                    let (currentState, docsInternal, nursesInternal) = aggr
                    let currentTimeSlot = currentSurgery.TimeRange 

                    let doctorMatch = Implementation.tryFindForTimeSlot currentTimeSlot docsInternal
                    let nurseMatch = Implementation.tryFindForTimeSlot currentTimeSlot nursesInternal

                    let doctor = Implementation.buildEmployeeFromMatch doctorMatch currentTimeSlot
                    let nurse = Implementation.buildEmployeeFromMatch nurseMatch currentTimeSlot
                    
                    let doctorNewList = Implementation.buildUpdatedEmployeeList doctorMatch currentTimeSlot docsInternal
                    let nurseNewList = Implementation.buildUpdatedEmployeeList nurseMatch currentTimeSlot nursesInternal

                    (List.append currentState [(currentSurgery, doctor, nurse)], doctorNewList, nurseNewList)
                ) ([], doctorsTimeMap, nursesTimeMap)

            let schedules : SurgerySchedule list = 
                pairedSlots
                |> List.map (fun row -> 
                    let (surgeryInfo, doctorInfo, nurseInfo) = row
                    { Surgery = surgeryInfo.Pair; TimeSlot = surgeryInfo.TimeRange; Employees = [doctorInfo; nurseInfo]}
                )

            schedules

    let calculateTimeFrame : CalculateTimeFrame = 
        fun operationTimes -> 
            let first = (operationTimes |> List.minBy (fun m -> m.From.Ticks)).From
            let last = (operationTimes |> List.maxBy (fun m -> m.To.Ticks)).To
            { From = first; To = last }
    
    let distributionWorkflow : DistributionWorkflow = 
        fun setup calculateDistribution calculateTimeframe -> 
            let timeFrame = setup.OperationTimes |> calculateTimeframe
            let schedule = calculateDistribution setup.EmployeeSetup setup.SurgerySetup
            { TimeRange = timeFrame; Schedules = schedule }

