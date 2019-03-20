//let setupDTO : SetupDTO = {
//        Employees = [|
//            {   Name = "Algis"
//                Position = "Nurse"
//                WorkHours = [|
//                    { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
//                    { From = DateTime(2018, 1, 2, 8, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
//                    { From = DateTime(2018, 1, 3, 8, 0, 0); To = DateTime(2018, 1, 3, 17, 0, 0)}
//                |]
//                TimeOff = [|
//                    { From = DateTime(2018, 1, 2, 12, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
//                |]   
//            }
//            {   Name = "Jonas"
//                Position = "Dentist"
//                WorkHours = [|
//                    { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
//                    { From = DateTime(2018, 1, 2, 8, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
//                    { From = DateTime(2018, 1, 4, 8, 0, 0); To = DateTime(2018, 1, 4, 17, 0, 0) }
//                |]
//                TimeOff = [|
//                    { From = DateTime(2018, 1, 2, 12, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
//                |]   
//            }
//        |]
//        Surgeries = [|
//            { Name = "Surgery 1"; WorkHours = [|
//                { From = DateTime(2018, 1, 1, 8, 0, 0); To = DateTime(2018, 1, 1, 17, 0, 0) }
//                { From = DateTime(2018, 1, 2, 8, 0, 0); To = DateTime(2018, 1, 2, 17, 0, 0) }
//            |]} 
//            { Name = "Surgery 2"; WorkHours = [|
//                { From = DateTime(2018, 1, 3, 8, 0, 0); To = DateTime(2018, 1, 3, 17, 0, 0) }
//                { From = DateTime(2018, 1, 4, 8, 0, 0); To = DateTime(2018, 1, 4, 17, 0, 0) }
//            |]}
//        |]
//    }

//    let setup = Mappings3.setupToDomain setupDTO
//    let prepedSurgeries = Setup3.prepSurgeries setup.Surgeries
//    let prepedNurses = Setup3.prepEmployees (setup.Employees |> List.filter (fun e -> e.Position = Nurse))
//    let preppedDoctors = Setup3.prepEmployees (setup.Employees |> List.filter (fun e -> e.Position = Dentist))
//    let result = Implementation3.fillSurgeries prepedSurgeries preppedDoctors prepedNurses
//    let resultDTO = result |> List.map (fun r -> Mappings3.staffAssignmentToDTO r)

//    let printReport (report : SurgeryStaffAssignment list) = 
//        for staffAssignment in report do
//            printfn "Surgery : %s (%s - %s)" staffAssignment.Surgery.Name (staffAssignment.Surgery.TimeSlot.From.ToString()) (staffAssignment.Surgery.TimeSlot.To.ToString())
//            for employee in staffAssignment.Staff do
//                printfn "\t Employee: %s(%A), Times: %s - %s" employee.Name employee.Position (employee.TimeSlot.From.ToString()) (employee.TimeSlot.To.ToString())

//    printReport result   