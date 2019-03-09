namespace Scheduler.Console.API
       

module ScheduleProvider = 
    open Scheduler.Core.Domain
    open Scheduler.Core.Domain.DTO

    let CalculateDistribution (setupDTO: SetupDTO) : DistributionDTO =
        let setup = setupDTO |> Mappers.mapSetupToDomain
        
        Operations.distributionWorkflow setup Operations.calculateDistribution Operations.calculateTimeFrame
        |> Mappers.mapDistributionToDTO
        