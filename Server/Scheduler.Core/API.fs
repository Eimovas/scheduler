namespace Scheduler.Core.API

open Scheduler.Core.Domain.DTO
open Scheduler.Core.Domain.Models
open Scheduler.Core.Mappings
open Scheduler.Core.Domain.Implementation
      
type IScheduleCalculator = 
    abstract Calculate : SetupDTO -> SurgeryStaffAssignmentDTO[]

type ScheduleCalculator() = 
    interface IScheduleCalculator with
        member x.Calculate(setupDto) = 
            let setup = Mappings.setupToDomain setupDto
            let prepedSurgeries = PreProcessing.prepSurgeries setup.Surgeries
            let prepedNurses = PreProcessing.prepEmployees (setup.Employees |> List.filter (fun e -> e.Position = Nurse))
            let preppedDoctors = PreProcessing.prepEmployees (setup.Employees |> List.filter (fun e -> e.Position = Dentist))
            let result = Processing.fillSurgeries prepedSurgeries preppedDoctors prepedNurses
            result |> List.map (fun r -> Mappings.staffAssignmentToDTO r) |> List.toArray