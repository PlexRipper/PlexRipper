import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, toRefs } from 'vue';
import { type Observable, of, Subject } from 'rxjs';
import { DialogType } from '@enums';
import { StoreNames, type IAccountDialog, type IAlert, type IConnectionDialog, type IDialogState, type IHelp, type ISetupResult } from '@interfaces';
import type {
	CheckAllConnectionStatusUpdateDTO,
	DownloadMediaDTO,
	FolderPathDTO,
	RefreshPlexAccountAccessRapportDTO,
} from '@dto';
import { cloneDeep } from 'lodash-es';

interface IDialogStoreState {
	dialogUpdate: Subject<IDialogState>;
}

export const useDialogStore = defineStore(StoreNames.DialogStore, () => {
	const defaultState: IDialogStoreState = {
		dialogUpdate: new Subject<IDialogState>(),
	};

	const state = reactive<IDialogStoreState>(cloneDeep(defaultState));

	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: StoreNames.DialogStore, isSuccess: true });
		},
		closeDialog(name: DialogType): void {
			state.dialogUpdate.next({ name, state: false, data: {} as unknown });
		},
		openDialog(name: DialogType): void {
			state.dialogUpdate.next({ name, state: true, data: {} as unknown });
		},
		openCheckServerConnectionsDialog(data: CheckAllConnectionStatusUpdateDTO): void {
			state.dialogUpdate.next({ name: DialogType.CheckServerConnectionDialogName, state: true, data });
		},
		openAccountDialog(data: IAccountDialog): void {
			state.dialogUpdate.next({ name: DialogType.AccountDialog, state: true, data });
		},
		openDirectoryBrowserDialog(data: FolderPathDTO): void {
			state.dialogUpdate.next({ name: DialogType.DirectoryBrowserDialog, state: true, data });
		},
		openServerSettingsDialog(plexServerId: number): void {
			state.dialogUpdate.next({ name: DialogType.ServerSettingsDialog, state: true, data: plexServerId });
		},
		openDownloadTaskDetailsDialog(downloadTaskId: string): void {
			state.dialogUpdate.next({ name: DialogType.DownloadDetailsDialog, state: true, data: downloadTaskId });
		},
		openMediaConfirmationDownloadDialog(data: DownloadMediaDTO[]): void {
			state.dialogUpdate.next({ name: DialogType.MediaDownloadConfirmationDialog, state: true, data });
		},
		openAddConnectionDialog(data: IConnectionDialog): void {
			state.dialogUpdate.next({ name: DialogType.AddConnectionDialog, state: true, data });
		},
		openRefreshPlexAccountAccessDialog(data: RefreshPlexAccountAccessRapportDTO[]): void {
			state.dialogUpdate.next({ name: DialogType.RefreshAccountAccessDialog, state: true, data });
		},
		openHelpInfoDialog(data: IHelp): void {
			state.dialogUpdate.next({ name: DialogType.HelpInfoDialog, state: true, data });
		},
		openAlertInfoDialog(alert: IAlert): void {
			state.dialogUpdate.next({ name: `${DialogType.AlertInfoDialog}-${alert.id}`, state: true, data: alert });
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};
	const getters = {
		getDialogState: (): Observable<IDialogState> => {
			return state.dialogUpdate.asObservable();
		},
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useDialogStore, import.meta.hot));
}
