open System
open System.IO

#r @"c:\code\FSharp\Scheduler\packages\Newtonsoft.Json\lib\netstandard2.0\Newtonsoft.Json.dll"
#r @"c:\code\FSharp\Scheduler\packages\NETStandard.Library\build\netstandard2.0\ref\netstandard.dll"

module Json =
    open Newtonsoft.Json
    
    let serialize obj = JsonConvert.SerializeObject obj
    let deserialize<'a> str =
        try
            JsonConvert.DeserializeObject<'a> str
            |> Result.Ok
        with
        | ex -> Result.Error ex

type NonEmptyString = NonEmptyString of string
type PositiveInt = PositiveInt of int

type TimeRange = {
    From : DateTime
    To : DateTime
}

type Surgery = {
    Name : string
}

type SurgerySetup = {
    Surgery : Surgery
    OperationalTimes : TimeRange list
}

type PracticeSetup = {
    OperationTimes : TimeRange list
    SurgerySetup : SurgerySetup list
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
    //TimeOff : TimeRange list
    WorkTimes : TimeRange list
    HourLimitPerWeek : decimal
    RequestedOvertime : TimeRange list // TODO: have a separate type, only relevant for nurses
}

type EmployeeSetup = {
    Employees : Employee list
}

type Setup = {
    EmployeeSetup : EmployeeSetup
    PracticeSetup : PracticeSetup
}

type SurgerySchedule = {
    Surgery : Surgery
    TimeSlot : TimeRange
    Employees : Employee list
}

type EmployeeSchedule = {
    Employee : Employee
    WorkingTimes : TimeRange list
}

type Distribution = {
    Schedules: SurgerySchedule list
    TimeRange : TimeRange
}

type CalculateDistribution = Setup -> Distribution
type SimpleWorkflow = Setup -> CalculateDistribution -> Distribution

//let calculateDistribution : CalculateDistribution = 
//    fun setup -> 
       
let setup = {
    PracticeSetup = { 
        OperationTimes = [
            {
                From = new DateTime(2018, 1, 1, 8, 0, 0)
                To = new DateTime(2018, 1, 1, 17, 0, 0)
            };
            {
                From = new DateTime(2018, 1, 2, 8, 0, 0)
                To = new DateTime(2018, 1, 2, 17, 0, 0)
            };
            {
                From = new DateTime(2018, 1, 3, 8, 0, 0)
                To = new DateTime(2018, 1, 3, 17, 0, 0)
            }
        ]
        SurgerySetup = [
            {
                Surgery = { Name = "Surgery 1"}
                OperationalTimes = [
                    {
                        From = new DateTime(2018, 1, 1, 8, 0, 0)
                        To = new DateTime(2018, 1, 1, 17, 0, 0)
                    };
                    {
                        From = new DateTime(2018, 1, 2, 8, 0, 0)
                        To = new DateTime(2018, 1, 2, 17, 0, 0)
                    };
                    {
                        From = new DateTime(2018, 1, 3, 8, 0, 0)
                        To = new DateTime(2018, 1, 3, 17, 0, 0)
                    }
                ]
            };
            {
                Surgery = { Name = "Surgery 2"}
                OperationalTimes = [
                    {
                        From = new DateTime(2018, 1, 1, 8, 0, 0)
                        To = new DateTime(2018, 1, 1, 17, 0, 0)
                    };
                    {
                        From = new DateTime(2018, 1, 2, 8, 0, 0)
                        To = new DateTime(2018, 1, 2, 17, 0, 0)
                    }
                ]
            }
        ]
    }
    EmployeeSetup = {
        Employees = [
            {
                Name = "Algis"
                Specialization = Position.Nurse
                HourLimitPerWeek = 0M
                RequestedOvertime = []
                WorkTimes = [
                    {
                        From = new DateTime(2018, 1, 1, 8, 0, 0)
                        To = new DateTime(2018, 1, 1, 17, 0, 0)
                    };
                    {
                        From = new DateTime(2018, 1, 2, 8, 0, 0)
                        To = new DateTime(2018, 1, 2, 17, 0, 0)
                    };
                    {
                        From = new DateTime(2018, 1, 3, 8, 0, 0)
                        To = new DateTime(2018, 1, 3, 17, 0, 0)
                    }
                ]
            };
            {
                Name = "Zigmas"
                Specialization = Position.Dentist
                HourLimitPerWeek = 0M
                RequestedOvertime = []
                WorkTimes = [
                    {
                        From = new DateTime(2018, 1, 1, 8, 0, 0)
                        To = new DateTime(2018, 1, 1, 17, 0, 0)
                    };
                    {
                        From = new DateTime(2018, 1, 2, 8, 0, 0)
                        To = new DateTime(2018, 1, 2, 17, 0, 0)
                    };
                    {
                        From = new DateTime(2018, 1, 3, 8, 0, 0)
                        To = new DateTime(2018, 1, 3, 17, 0, 0)
                    }
                ]
            }
        ]
    }
}

let setupStr = Json.serialize setup
let path = Path.Combine(@"c:\code\FSharp\Scheduler\Scheduler.Core", "setup.json")
File.WriteAllText(path, setupStr)


// employees
let employeeSetup = setup.EmployeeSetup
let nurses = employeeSetup.Employees |> List.filter (fun e -> e.Specialization = Nurse)
let doctors = employeeSetup.Employees |> List.filter (fun e -> e.Specialization <> Nurse)

let doctorsTimeMap = 
    doctors
    |> List.map (fun current -> current.WorkTimes |> List.map(fun w -> (current.Name, w)))
    |> List.fold (fun aggr current -> List.append current aggr) []

let nursesTimeMap = 
    nurses
    |> List.map (fun current -> current.WorkTimes |> List.map(fun w -> (current.Name, w)))
    |> List.fold (fun aggr current -> List.append current aggr) []


// practice
let practiceSetup = setup.PracticeSetup
let surgeries = practiceSetup.SurgerySetup

let surgerySlotMap = 
    surgeries 
    |> List.map (fun surgery -> 
        surgery.OperationalTimes
        |> List.map(fun op -> (surgery.Surgery, op))
    )
    |> List.fold (fun aggr curr -> List.append curr aggr) []

let pairedSlotsAggregation = 
    surgerySlotMap
    |> List.fold (fun aggr currentSurgery -> 
        let (currentState, docsInternal, nursesInternal) = aggr
        let currentTimeSlot = snd currentSurgery 

        let doctor = 
            docsInternal
            |> List.tryFind(fun r -> (snd r).From = currentTimeSlot.From && (snd r).To = currentTimeSlot.To)

        let nurse = 
            nursesInternal 
            |> List.tryFind (fun r -> 
                let (_, times) = r
                times.From = currentTimeSlot.From && times.To = currentTimeSlot.To
            )

        let nurseNewList = 
            match nurse with
            | Some n -> nursesInternal |> List.filter (fun x -> x <> n)
            | None -> nursesInternal

        let docNewList = 
            match doctor with
            | Some d -> docsInternal |> List.filter (fun x -> x <> d)
            | None -> docsInternal

        //docsInternal |> List.iter (fun i -> printf "%s \n" ((snd i).From.ToString()))
        //let nurseNewList = nursesInternal |> List.filter (fun n -> n <> nurse)
        //let docNewList = docsInternal |> List.filter (fun n -> n <> doctor)
                
        (List.append currentState [(currentSurgery, doctor, nurse)], docNewList, nurseNewList)
    ) ([], doctorsTimeMap, nursesTimeMap)

let (pairedSlots, _, _) = pairedSlotsAggregation

let result = 
    pairedSlots
    |> List.iter (fun row -> 
        let (surgery, doctor, nurse) = row
        let nurseText = 
            match nurse with
            | Some (n,_) -> sprintf "%s" n
            | None -> "None"
        let docText = 
            match doctor with
            | Some (n,_) -> sprintf "%s" n
            | None -> "None"

        let (surgeryName, times) = surgery
        //let surgeryName = (fst surgery).Name

        let times = sprintf "%s - %s" (times.From.ToString()) (times.To.ToString())
        printf "Surgery: %s -- %s, Doctor: %s, Nurse: %s \n" surgeryName.Name times docText nurseText)


        
// take each of surgery working times and create time slots that need to be filled -> replaced with dentist + nurse pair and a timerange?
// go through each of survery working times and pair it with corresponding doctor+nurse time pairs
// 
// 


