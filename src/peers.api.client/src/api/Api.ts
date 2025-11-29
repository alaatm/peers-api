import type { Key } from 'react';
import RestClient from './RestClient';
import type { TableParams } from '@/components/Tables';
import type { DispatchMessageCommand } from './commands';
import {
    type ClientTaskAcknowledgeResponse, type ClientTaskRequestResponse, ClientTaskRequestTaskType, type Customer, type CustomerSummary, type DeviceSummary, UserStatus,
    type DeviceError, type Lang, type PushNotificationProblem, type ClientApp,
    type AppUsageHistorySummary,
    type SysInfo
} from './types';

class Api extends RestClient {
    getCustomer(id: string | number) {
        return this.get<Customer>(`customers/${id}`);
    }

    listCustomers(params: TableParams) {
        return this.getPaged<CustomerSummary>('customers', this.extractTableParams(params));
    }

    setUserStatus(userId: number, status: UserStatus, changeReason: string) {
        return this.put(`users/${userId}/status`, {
            'newStatus': status,
            'changeReason': changeReason,
        });
    }

    listUserDevices(userId: number, params: TableParams) {
        return this.getPaged<DeviceSummary>(`users/${userId}/devices`, this.extractTableParams(params));
    }

    listUserAppUsageHistory(userId: number, params: TableParams) {
        return this.getPaged<AppUsageHistorySummary>(`users/${userId}/app-usage`, this.extractTableParams(params));
    }

    listPushNotificationProblems(params: TableParams) {
        return this.getPaged<PushNotificationProblem>('devices/push-notification-problems', this.extractTableParams(params));
    }

    listCrashReports(params: TableParams) {
        return this.getPaged<DeviceError>('devices/crash-reports', this.extractTableParams(params));
    }

    listSupportedLanguages() {
        return this.getPaged<Lang>('system/langs');
    }

    sendPushNotification(cmd: DispatchMessageCommand) {
        return this.post('devices/message-dispatch', cmd);
    }

    sendClientTaskRequest(deviceId: string, task: ClientTaskRequestTaskType) {
        return this.post<ClientTaskRequestResponse>(`devices/${deviceId}/client-task`, { task });
    }

    checkClientTaskRequest(deviceId: string, requestId: string) {
        return this.post<ClientTaskAcknowledgeResponse>(`devices/${deviceId}/client-task`, { requestId });
    }

    /* System Info */

    getClientApp() {
        return this.get<ClientApp>('system/client-app');
    }

    getSystemInfo() {
        return this.get<SysInfo>('system/info');
    }

    private extractTableParams(params: TableParams) {
        const { pagination, sortField, sortOrder, filters } = params;
        const { current: page, pageSize } = pagination || {};

        return {
            page,
            pageSize,
            sortField: sortField as Key | undefined,
            sortOrder,
            filters: filters ? JSON.stringify(filters) : undefined,
        };
    }

    // private createFormData(data: object, file?: File) {
    //     const formData = new FormData();
    //     formData.append('data', JSON.stringify(data));
    //     if (file) {
    //         formData.append('file', file, file.name);
    //     }
    //     return formData;
    // }
}

export default new Api();
