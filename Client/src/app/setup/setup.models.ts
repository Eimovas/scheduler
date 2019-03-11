export interface SurgeryDTO {
    name: string;
    workTimes: TimeRange[];
}

export interface EmployeeDTO {
    name: string;
    specialization: number;
    workTimes: TimeRange[];
    hourLimitPerWeek: number;
    requestedOvertime: TimeRange[];
}

export interface SurgeryScheduleDTO {
    surgery: SurgeryDTO;
    timeSlot: TimeRange;
    employees: PairedTime<EmployeeDTO>[];
}

export interface DistributionDTO {
    schedules: SurgeryScheduleDTO[];
    timeRange: TimeRange;
}

export interface SetupDTO {
    operationTimes: TimeRange[];
    employeeSetup: EmployeeDTO[];
    surgerySetup: SurgeryDTO[];
}

export interface TimeRange {
    to: Date;
    from: Date;
}

export interface PairedTime<T> {
    pair: T;
    timeRange: TimeRange;
}

export const sampleSetup: SetupDTO = {
    surgerySetup: [
        {
            name: "Surgery 1",
            workTimes: [
                { from: new Date(2018, 1, 1, 8, 0, 0), to: new Date(2018, 1, 1, 12, 0, 0) },
                { from: new Date(2018, 1, 1, 13, 0, 0), to: new Date(2018, 1, 1, 17, 0, 0) }
            ]
        }
    ],
    employeeSetup: [
        {
            name: "Algis",
            hourLimitPerWeek: 0.0,
            requestedOvertime: [],
            specialization: 1,
            workTimes: [
                { from: new Date(2018, 1, 1, 8, 0, 0), to: new Date(2018, 1, 1, 17, 0, 0) }
            ]
        },
        {
            name: "Jonas",
            hourLimitPerWeek: 0.0,
            requestedOvertime: [],
            specialization: 2,
            workTimes: [
                { from: new Date(2018, 1, 1, 8, 0, 0), to: new Date(2018, 1, 1, 17, 0, 0) }
            ]
        }
    ],
    operationTimes: [
        { from: new Date(2018, 1, 1, 8, 0, 0), to: new Date(2018, 1, 2, 17, 0, 0) }
    ]
}