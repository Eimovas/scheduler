namespace Scheduler.Core.Domain.DTO

open Scheduler.Core.SimpleTypes

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