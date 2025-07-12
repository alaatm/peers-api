export enum DeviceGroup {
    None = 0,
    Customers = 1 << 0,
    Dispatchers = 1 << 1,
    Drivers = 1 << 2,
    ManagedDrivers = 1 << 3,
    Partners = Dispatchers | Drivers,
    All = Customers | Dispatchers | Drivers,
}

export type CancelTripCommand = {
    cancellationReasonId: number;
    applyCancellationCharge: boolean;
}

export type DispatchMessageCommand = {
    deviceToken?: string | null;
    deviceGroup: DeviceGroup;
    title: Record<string, string>;
    body: Record<string, string>;
}