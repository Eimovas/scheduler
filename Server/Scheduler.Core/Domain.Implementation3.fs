namespace Scheduler.Core.Domain.Implementation3

open System

type TimeRange = {
    From : DateTime
    To : DateTime
}

type Position = 
| Nurse
| Dentist

type EmployeeWorkSlot = {
    Name : string
    Position : Position
    TimeSlot : TimeRange
}
type SurgeryWorkSlot = {
    Name : string
    TimeSlot : TimeRange
}

type Employee = {
    Name : string
    WorkHours : TimeRange list
    TimeOff : TimeRange list
    Position : Position
}

type Surgery = {
    Name : string
    WorkHours : TimeRange list
}

type Setup = {
    Employees : Employee list
    Surgeries : Surgery list
}

type SurgeryStaffAssignment = {
    Surgery : SurgeryWorkSlot
    Staff : EmployeeWorkSlot list
}

type MatchType = 
| Full
| PartialMore
| PartialLess


type SurgeryStaffAssignmentDTO = {
    Surgery : SurgeryWorkSlotDTO
    Staff : EmployeeWorkSlotDTO[]
}
and SurgeryWorkSlotDTO = {
    Name : string
    TimeSlot : TimeRange
}
and EmployeeWorkSlotDTO = {
    Name : string
    TimeSlot : TimeRange
    Position : string
}

type SetupDTO = {
    Surgeries : SurgeryDTO[]
    Employees : EmployeeDTO[]
}
and SurgeryDTO = {
    Name : string
    WorkHours : TimeRange[]
}
and EmployeeDTO = {
    Name : string
    WorkHours : TimeRange[]
    TimeOff : TimeRange[]
    Position : string
}

type MatchForType = EmployeeWorkSlot -> TimeRange -> MatchType -> (EmployeeWorkSlot * EmployeeWorkSlot * EmployeeWorkSlot list) option 
type FindNextBest = MatchForType -> MatchType -> TimeRange -> EmployeeWorkSlot list -> (EmployeeWorkSlot list * EmployeeWorkSlot list)
type GetIntervalDiff = TimeRange -> TimeRange list -> TimeRange list
type FillSurgeries = FindNextBest -> MatchForType -> GetIntervalDiff -> SurgeryWorkSlot list -> EmployeeWorkSlot list -> EmployeeWorkSlot list -> SurgeryStaffAssignment list

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

module Implementation3 = 
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

module internal Setup3 =
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

module Mappings3 = 
    let surgeryWorkSlotToDTO (surgery : SurgeryWorkSlot) : SurgeryWorkSlotDTO = 
        { Name = surgery.Name ; TimeSlot = surgery.TimeSlot }
    
    let employeeWorkSlotToDTO (employee : EmployeeWorkSlot) : EmployeeWorkSlotDTO = 
        { Name = employee.Name; Position = employee.Position.ToString(); TimeSlot = employee.TimeSlot}

    let private staffAssignmentToDTOInternal surgeryToDTO employeeToDTO (assignment : SurgeryStaffAssignment) : SurgeryStaffAssignmentDTO = 
        { 
            Surgery = surgeryToDTO assignment.Surgery
            Staff = assignment.Staff |> List.map (fun s -> employeeToDTO s) |> List.toArray
        }
    
    let staffAssignmentToDTO = staffAssignmentToDTOInternal surgeryWorkSlotToDTO employeeWorkSlotToDTO

    let employeeToDomain (employee : EmployeeDTO) : Employee = 
        { 
            Name = employee.Name
            Position = 
                match employee.Position with
                | "Nurse" -> Nurse
                | "Dentist" -> Dentist
                | position -> failwith (sprintf "Given employee position '%s' is unsuported" position)
            TimeOff = employee.TimeOff |> Array.toList
            WorkHours = employee.WorkHours |> Array.toList
        }

    let surgeryToDomain (surgery : SurgeryDTO) : Surgery = 
        { Name = surgery.Name; WorkHours = surgery.WorkHours |> Array.toList }

    let private setupToDomainInternal employeeToDomain surgeryToDomain (setup : SetupDTO) : Setup = 
        {
            Employees = setup.Employees |> Array.map (fun e -> employeeToDomain e) |> Array.toList
            Surgeries = setup.Surgeries |> Array.map (fun s -> surgeryToDomain s) |> Array.toList
        }

    let setupToDomain = setupToDomainInternal employeeToDomain surgeryToDomain
    // TODO: finished here. Do the setup DTO mappings and validation, commit everything and refactor everything.

    
module Test = 
    let setupDTO : SetupDTO = {
        Employees = [|
            {   Name = "Algis"
                Position = "Nurse"
                WorkHours = [|
                    { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
                    { From = DateTime(2018, 1, 2, 8, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
                    { From = DateTime(2018, 1, 3, 8, 0, 0); To = DateTime(2018, 1, 3, 17, 0, 0)}
                |]
                TimeOff = [|
                    { From = DateTime(2018, 1, 2, 12, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
                |]   
            }
            {   Name = "Jonas"
                Position = "Dentist"
                WorkHours = [|
                    { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
                    { From = DateTime(2018, 1, 2, 8, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
                    { From = DateTime(2018, 1, 4, 8, 0, 0); To = DateTime(2018, 1, 4, 17, 0, 0) }
                |]
                TimeOff = [|
                    { From = DateTime(2018, 1, 2, 12, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
                |]   
            }
        |]
        Surgeries = [|
            { Name = "Surgery 1"; WorkHours = [|
                { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
                { From = DateTime(2018, 1, 2, 8, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
            |]} 
            { Name = "Surgery 2"; WorkHours = [|
                { From = DateTime(2018, 1, 3, 8, 0, 0); To = DateTime(2018, 1, 3, 17, 0, 0) }
                { From = DateTime(2018, 1, 4, 8, 0, 0); To = DateTime(2018, 1, 4, 17, 0, 0) }
            |]}
        |]
    }

    let setup = Mappings3.setupToDomain setupDTO
    let prepedSurgeries = Setup3.prepSurgeries setup.Surgeries
    let prepedNurses = Setup3.prepEmployees (setup.Employees |> List.filter (fun e -> e.Position = Nurse))
    let preppedDoctors = Setup3.prepEmployees (setup.Employees |> List.filter (fun e -> e.Position = Dentist))
    let result = Implementation3.fillSurgeries prepedSurgeries preppedDoctors prepedNurses
    let resultDTO = result |> List.map (fun r -> Mappings3.staffAssignmentToDTO r)

    let printReport (report : SurgeryStaffAssignment list) = 
        for staffAssignment in report do
            printfn "Surgery : %s (%s - %s)" staffAssignment.Surgery.Name (staffAssignment.Surgery.TimeSlot.From.ToString()) (staffAssignment.Surgery.TimeSlot.To.ToString())
            for employee in staffAssignment.Staff do
                printfn "\t Employee: %s(%A), Times: %s - %s" employee.Name employee.Position (employee.TimeSlot.From.ToString()) (employee.TimeSlot.To.ToString())

    printReport result