export interface SurgeryDTO {
    name: string;
    workHours: TimeRange[];
}

export interface EmployeeDTO {
    name: string;
    position: string;
    workHours: TimeRange[];
    timeOff: TimeRange[];
}

export interface SetupDTO {
    employees: EmployeeDTO[];
    surgeries: SurgeryDTO[];
}

export interface TimeRange {
    to: Date;
    from: Date;
}

export const sampleSetup: SetupDTO = {
    surgeries: [
        {
            name: "Surgery 1",
            workHours: [
                { from: new Date(2018, 1, 1, 8, 0, 0), to: new Date(2018, 1, 1, 12, 0, 0) },
                { from: new Date(2018, 1, 1, 13, 0, 0), to: new Date(2018, 1, 1, 17, 0, 0) }
            ]
        }
    ],
    employees: [
        {
            name: "Algis",
            timeOff: [],
            position: "Nurse",
            workHours: [
                { from: new Date(2018, 1, 1, 8, 0, 0), to: new Date(2018, 1, 1, 17, 0, 0) }
            ]
        },
        {
            name: "Jonas",
            timeOff: [],
            position: "Dentist",
            workHours: [
                { from: new Date(2018, 1, 1, 8, 0, 0), to: new Date(2018, 1, 1, 17, 0, 0) }
            ]
        }
    ]
}