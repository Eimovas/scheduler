namespace Scheduler.Core.Infrastructure
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


module Json =
    open Newtonsoft.Json
    
    let serialize obj = JsonConvert.SerializeObject obj
    let deserialize<'a> str =
        try
            JsonConvert.DeserializeObject<'a> str
            |> Result.Ok
        with
        | ex -> Result.Error ex
