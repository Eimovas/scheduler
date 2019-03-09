namespace Scheduler.Core.Domain

open System

type NonEmptyString = NonEmptyString of string
type PositiveInt = PositiveInt of int

type TimeRange = {
    From : DateTime
    To : DateTime
}
type PairedTime<'a> = {
    Pair : 'a
    TimeRange : TimeRange
}
module PairedTime = 
    let create p t = { Pair = p; TimeRange = t }



type Surgery = {
    Name : string
    WorkTimes : TimeRange list
}

type Position = 
| Nurse
| Dentist
| Hygienist
| OrthodontistTherapeut
| Specialist

type Employee = {
    Name : string
    Specialization : Position
    WorkTimes : TimeRange list
    HourLimitPerWeek : decimal
    RequestedOvertime : TimeRange list // TODO: have a separate type, only relevant for nurses
}

type Setup = {
    OperationTimes : TimeRange list
    EmployeeSetup : Employee list
    SurgerySetup : Surgery list
}

type SurgerySchedule = {
    Surgery : Surgery
    TimeSlot : TimeRange
    Employees : PairedTime<Employee> option list
}

type EmployeeSchedule = {
    Employee : Employee
    WorkingTimes : TimeRange list
}

type Distribution = {
    Schedules: SurgerySchedule list
    TimeRange : TimeRange
}




type CalculateTimeFrame = TimeRange list -> TimeRange
type CalculateDistribution = Employee list -> Surgery list -> SurgerySchedule list

type DistributionWorkflow = Setup -> CalculateDistribution -> CalculateTimeFrame -> Distribution