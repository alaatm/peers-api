interface HasName {
    translationKey: string;
}

export enum UserStatus {
    // None = 0,
    Active = 1,
    Suspended = 2,
    Banned = 3,
}

export enum ClientTaskRequestTaskType {
    Ping,
    SignOff,
}

export enum ClientTaskAcknowledgeStatus {
    Waiting,
    Ok,
    NoResponse,
}

export type IdName = {
    id: number;
    name: string;
}

export type ClientTaskRequestResponse = {
    requestId: string;
}

export type ClientTaskAcknowledgeResponse = {
    status: ClientTaskAcknowledgeStatus;
}

export type CustomerSummary = {
    id: number;
    username: string;
    name: string;
    status: UserStatus;
    bookingsCount: number;
    appLastUsed: string | null;
};

export type Customer = CustomerSummary & {};

export type ProfileImage = {
    id: number,
    description: string | null,
    link: string;
}

export type Profile = {
    avatarLink: string | null,
    bio: string | null,
    images: ProfileImage[];
}

export type AppUsageHistorySummary = {
    openedAt: string;
}

export type DeviceSummary = {
    id: number;
    deviceId: string;
    manufacturer: string;
    model: string;
    platform: string;
    osVersion: string;
    idiom: string;
    deviceType: string;
    handle: string;
    registeredOn: string;
    app: string;
    appVersion: string;
    isStalled: boolean;
    faults: number;
    lastPing: string | null;
}

export type DeviceError = {
    id: number;
    reportedOn: string;
    deviceId: string;
    username: string | null;
    locale: string | null;
    silent: boolean;
    source: string | null;
    app: string | null;
    appVersion: string | null;
    appState: string | null;
    exception: string | null;
    stackTrace: string[];
    info: string[];
    deviceInfo: string | null;
}

export enum FirebaseErrorCode {
    InvalidArgument,
    FailedPrecondition,
    OutOfRange,
    Unauthenticated,
    PermissionDenied,
    NotFound,
    Conflict,
    Aborted,
    AlreadyExists,
    ResourceExhausted,
    Cancelled,
    DataLoss,
    Unknown,
    Internal,
    Unavailable,
    DeadlineExceeded,
}

export enum FirebaseMessagingErrorCode {
    ThirdPartyAuthError,
    InvalidArgument,
    Internal,
    QuotaExceeded,
    SenderIdMismatch,
    Unavailable,
    Unregistered,
}

export type PushNotificationProblem = {
    id: number;
    reportedOn: string;
    token: string;
    errorCode: FirebaseErrorCode;
    messagingErrorCode: FirebaseMessagingErrorCode | null;
}

export type Lang = {
    name: string;
    code: string;
    dir: 'ltr' | 'rtl';
}

/*
 System Info
*/

export type AppInfo = {
    version: string;
    buildDate: string;
}

export type ServerInfo = {
    osVersion: string;
    runtimeVersion: string;
}

export type DatabaseInfo = {
    version: string | null;
    serviceLevelObjective: string | null;
    dtuLimit: number | null;
    cpuLimit: number | null;
    minCores: number | null;
    maxDop: number | null;
    maxSessions: number | null;
    maxDbMemory: number | null;
    maxDbMaxSizeInMb: number | null;
    checkpointRateIO: number | null;
    checkpointRateMbps: number | null;
    primaryGroupMaxWorkers: number | null;
    primaryMaxLogRate: number | null;
    primaryGroupMaxIO: number | null;
    primaryGroupMaxCpu: number | null;
    volumeTypeManagedXstoreIOPS: number | null;
}

export type SysInfo = {
    app: AppInfo;
    server: ServerInfo;
    database: DatabaseInfo;
}

export type ClientApp = {
    packageName: string;
    hashString: string;
    androidStoreLink: string;
    iosStoreLink: string;
    versionString: string;
}

/* App Settings */

export type LocalizedName = {
    language: string;
    value: string;
}

export enum DayOfWeek {
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
}

export enum PeriodKind {
    Year,
    Quarter,
    Month,
    Week,
    Day,
    Custom,
}

(UserStatus as unknown as HasName).translationKey = 'UserStatus';
(ClientTaskRequestTaskType as unknown as HasName).translationKey = 'ClientTaskRequestTaskType';
(ClientTaskAcknowledgeStatus as unknown as HasName).translationKey = 'ClientTaskAcknowledgeStatus';
(FirebaseErrorCode as unknown as HasName).translationKey = 'FirebaseErrorCode';
(FirebaseMessagingErrorCode as unknown as HasName).translationKey = 'FirebaseMessagingErrorCode';
(DayOfWeek as unknown as HasName).translationKey = 'DayOfWeek';
(PeriodKind as unknown as HasName).translationKey = 'PeriodKind';
