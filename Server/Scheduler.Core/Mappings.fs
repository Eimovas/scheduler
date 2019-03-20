namespace Scheduler.Core.Mappings

module Mappings = 
    open Scheduler.Core.Domain.DTO
    open Scheduler.Core.Domain.Models

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