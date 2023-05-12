import { useEventBus, UseEventBusReturn } from '@vueuse/core';
import { DownloadMediaDTO } from '@dto/mainApi';

// region Dialog Controls

export function useControlDialog() {
	return useEventBus<{
		name: string;
		state: boolean;
		value?: any;
	}>('controlDialog');
}

export function useOpenControlDialog(name: string, value?: any) {
	useControlDialog().emit({
		name,
		state: true,
		value,
	});
}

export function useCloseControlDialog(name: string) {
	useControlDialog().emit({
		name,
		state: false,
	});
}

// endregion

// region MediaOverview

export interface IMediaOverviewBarBus {
	downloadButtonVisible: boolean;
}

export function useMediaOverviewBarBus(): UseEventBusReturn<IMediaOverviewBarBus, any> {
	return useEventBus<IMediaOverviewBarBus>('mediaOverViewBarBus');
}

export interface IMediaOverviewSort {
	field: string;
	sort: 'asc' | 'none' | 'desc';
}

export function useMediaOverviewSortBus(): UseEventBusReturn<IMediaOverviewSort, any> {
	return useEventBus<IMediaOverviewSort>('mediaOverviewSort');
}

export function setMediaOverviewSort(action: IMediaOverviewSort) {
	useMediaOverviewSortBus().emit(action);
}

export interface IMediaOverviewCommands {
	command: 'scrollTo' | 'download';
	scrollToLetter?: string;
	downloadMediaCommands?: DownloadMediaDTO[];
}

export function useMediaOverviewCommandsBus(): UseEventBusReturn<IMediaOverviewCommands, any> {
	return useEventBus<IMediaOverviewCommands>('mediaOverViewCommands');
}

export function sendMediaOverviewScrollToCommand(letter: string): void {
	useMediaOverviewCommandsBus().emit({
		command: 'scrollTo',
		scrollToLetter: letter,
	});
}

export function listenMediaOverviewScrollToCommand(action: (letter: string) => void): void {
	useMediaOverviewCommandsBus().on(({ command, scrollToLetter }) => {
		if (command === 'scrollTo') {
			action(scrollToLetter ?? '');
		}
	});
}

/**
 * This is used to send a command to from the MediaOverviewBar to trigger a download command.
 */
export function useMediaOverviewBarDownloadCommandBus(): UseEventBusReturn<string, any> {
	return useEventBus<string>('downloadCommand');
}

export function sendMediaOverviewBarDownloadCommand(downloadMediaCommands: DownloadMediaDTO[]): void {
	useMediaOverviewCommandsBus().emit({
		command: 'download',
		downloadMediaCommands,
	});
}

export function listenMediaOverviewDownloadCommand(action: (downloadMediaCommands: DownloadMediaDTO[]) => void): void {
	useMediaOverviewCommandsBus().on(({ command, downloadMediaCommands }) => {
		if (command === 'download') {
			action(downloadMediaCommands ?? []);
		}
	});
}

// endregion
