namespace Scheduler.Core.Domain.Models

open Scheduler.Core.SimpleTypes

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

type MatchForType = EmployeeWorkSlot -> TimeRange -> MatchType -> (EmployeeWorkSlot * EmployeeWorkSlot * EmployeeWorkSlot list) option 
type FindNextBest = MatchForType -> MatchType -> TimeRange -> EmployeeWorkSlot list -> (EmployeeWorkSlot list * EmployeeWorkSlot list)
type FillSurgeries = FindNextBest -> MatchForType -> GetIntervalDiff -> SurgeryWorkSlot list -> EmployeeWorkSlot list -> EmployeeWorkSlot list -> SurgeryStaffAssignment list
