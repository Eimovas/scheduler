namespace Scheduler.Core

module List = 
    let flatten (listOfLists: 'a list list ) : 'a list = 
        listOfLists |> List.fold (fun aggr current -> List.append current aggr) []