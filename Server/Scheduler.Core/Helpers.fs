namespace Scheduler.Core.Helpers
module Logging = 
    open System
  
    type LoggingValue<'a> = 
    | Log of 'a * list<string>

    // Listing 12.23 Computation builder that adds logging support 
               
    type LoggingBuilder() =
        member x.Bind(Log(v, logs1), f) =  // Unwrap the value and log buffer
            // Run the rest of the computation
            let (Log(nv, logs2)) = f(v)
            // Wrap the value and merge log buffers
            Log(nv, logs1 @ logs2)
    
    // Augment value with an empty log
        member x.Return(v) = Log(v, [])
        // No value with an empty log
        member x.Zero() = Log((), [])
    
    let log = new LoggingBuilder()
    let logMessage(s) = Log((), [s])

    // Listing 12.24 Logging using computation expressions    
  
    // Writes string to console and to the log
    let write(s) = log { 
        do! logMessage("writing: " + s)
        Console.Write(s) 
    }

    let read() = log { 
        do! logMessage("reading...")
        return Console.ReadLine() }
      
    let testIt () = log { 
    // Call the primitive logging function
        do! logMessage("starting...")
        // Call function written using computation expressions
        do! write("Enter your name:")
        // Using customized value binding
        let! name = read()
        return "Hello " + name + "!" }

module List = 
    let flatten (listOfLists: 'a list list ) : 'a list = 
        listOfLists |> List.fold (fun aggr current -> List.append current aggr) []

module Json =
    open Newtonsoft.Json
    
    let serialize obj = JsonConvert.SerializeObject obj
    let deserialize<'a> str =
        try
            JsonConvert.DeserializeObject<'a> str
            |> Result.Ok
        with
        | ex -> Result.Error ex

module Helpers =
    open Scheduler.Core.Domain
    open System

    let printDistribution (distribution : Distribution) : unit =
        distribution.Schedules
        |> List.iter (fun s -> 
            let surgeryName = s.Surgery.Name
            let time = sprintf "%s - %s" (s.TimeSlot.From.ToString()) (s.TimeSlot.To.ToString()) 
            let employees = s.Employees |> List.map (fun e -> 
                match e with
                | Some em -> sprintf "%s (%s)" em.Pair.Name (em.Pair.Specialization.ToString())
                | None -> "NA"
            )
            printf "\n%s: \n \t %s \n \t %s" surgeryName time (String.Join(", ", employees)
        ))

module SetupInput =
    open Scheduler.Core.Domain
    open System

    let load() : Setup = 
        {
            OperationTimes = [
                { From = new DateTime(2018, 1, 1, 8, 0, 0); To = new DateTime(2018, 1, 1, 17, 0, 0) }
            ]
            SurgerySetup = [
                { 
                    Name = "Surgery 1"
                    WorkTimes = [
                        { From = new DateTime(2018, 1, 1, 8, 0, 0); To = new DateTime(2018, 1, 1, 12, 0, 0) }
                        { From = new DateTime(2018, 1, 1, 13, 0, 0); To = new DateTime(2018, 1, 1, 17, 0, 0) }
                    ]
                }
                //{ 
                //    Name = "Surgery 2"
                //    WorkTimes = [
                //        { From = new DateTime(2018, 1, 1, 8, 0, 0); To = new DateTime(2018, 1, 1, 17, 0, 0) }
                //    ]
                //}
                //{ 
                //    Name = "Surgery 3"
                //    WorkTimes = [
                //        { From = new DateTime(2018, 1, 1, 8, 0, 0); To = new DateTime(2018, 1, 1, 17, 0, 0) }
                //    ]
                //}
            ]
            EmployeeSetup = [
                {
                    Name = "Algis"
                    Specialization = Position.Nurse
                    WorkTimes = [
                        { From = new DateTime(2018, 1, 1, 8, 0, 0); To = new DateTime(2018, 1, 1, 17, 0, 0) }
                    ]
                    HourLimitPerWeek = 0M
                    RequestedOvertime = []
                }
                {
                    Name = "Juozas"
                    Specialization = Position.Nurse
                    WorkTimes = [
                        { From = new DateTime(2018, 1, 1, 8, 0, 0); To = new DateTime(2018, 1, 1, 11, 0, 0) }
                    ]
                    HourLimitPerWeek = 0M
                    RequestedOvertime = []
                }
                {
                    Name = "Zigmas"
                    Specialization = Position.Dentist
                    WorkTimes = [
                        { From = new DateTime(2018, 1, 1, 8, 0, 0); To = new DateTime(2018, 1, 1, 17, 0, 0) }
                    ]
                    HourLimitPerWeek = 0M
                    RequestedOvertime = []
                }
            ]
        }