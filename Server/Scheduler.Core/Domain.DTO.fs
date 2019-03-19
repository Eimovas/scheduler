namespace Scheduler.Core.Domain.DTO

open Scheduler.Core.Domain

type SurgeryDTO = {
    Name : string
    WorkTimes : TimeRange list
}

type EmployeeDTO = {
    Name : string
    Specialization : int
    WorkTimes : TimeRange list
    HourLimitPerWeek : decimal
    RequestedOvertime : TimeRange list
}

type SurgeryScheduleDTO = {
    Surgery : SurgeryDTO
    TimeSlot : TimeRange
    Employees : PairedTime<EmployeeDTO> list
}

type DistributionDTO = {
    Schedules: SurgeryScheduleDTO list
    TimeRange : TimeRange
}

type SetupDTO = {
    OperationTimes : TimeRange list
    EmployeeSetup : EmployeeDTO list
    SurgerySetup : SurgeryDTO list
}

module Mappers = 
    let private mapEmployeeToDomain (employee : EmployeeDTO) : Employee = 
        {
            Name = employee.Name
            Position = 
                match employee.Specialization with
                | 1 -> Nurse
                | _ -> Dentist
            WorkHours = employee.WorkTimes
            HourLimitPerWeek = employee.HourLimitPerWeek
            RequestedOvertime = employee.RequestedOvertime
        }

    let private mapSurgeryToDomain (surgery : SurgeryDTO) : Surgery = 
        {
            Name = surgery.Name
            WorkTimes = surgery.WorkTimes
        }

    let private mapSurgeryToDTO (surgery: Surgery) : SurgeryDTO = 
        {
            Name = surgery.Name
            WorkTimes = surgery.WorkTimes
        }

    let private mapEmployeeToDTO (employee: Employee) : EmployeeDTO = 
        {
            Name = employee.Name
            Specialization = 
                match employee.Position with
                | Nurse -> 1
                | _ -> 2
            WorkTimes = employee.WorkHours
            HourLimitPerWeek = employee.HourLimitPerWeek
            RequestedOvertime = employee.RequestedOvertime
        }

    let private mapEmployeePairToDTO (paired : PairedTime<Employee>) : PairedTime<EmployeeDTO> = 
        {
            Pair = paired.Pair |> mapEmployeeToDTO
            TimeRange = paired.TimeRange
        }

    let private mapScheduleToDTO (schedule: SurgerySchedule) : SurgeryScheduleDTO = 
        {
            Surgery = schedule.Surgery |> mapSurgeryToDTO
            TimeSlot = schedule.TimeSlot
            Employees = schedule.Employees 
                |> List.choose (fun r -> r)
                |> List.map mapEmployeePairToDTO
        }

    let mapSetupToDomain (setup : SetupDTO) : Setup = 
        {
            OperationTimes = setup.OperationTimes
            EmployeeSetup = setup.EmployeeSetup |> List.map mapEmployeeToDomain
            SurgerySetup = setup.SurgerySetup |> List.map mapSurgeryToDomain
        }
        
    let mapDistributionToDTO (distribution: Distribution) : DistributionDTO =
        {
            Schedules = distribution.Schedules |> List.map mapScheduleToDTO
            TimeRange = distribution.TimeRange
        }