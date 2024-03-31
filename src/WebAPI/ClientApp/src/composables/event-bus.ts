import { useEventBus, type UseEventBusReturn } from '@vueuse/core';
import type { DownloadMediaDTO } from '@dto';

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

export interface IMediaOverviewSort {
	field: string;
	sort: 'asc' | 'desc' | 'no-sort';
}

export function useMediaOverviewSortBus(): UseEventBusReturn<IMediaOverviewSort, any> {
	return useEventBus<IMediaOverviewSort>('mediaOverviewSort');
}

// region General
export interface IMediaOverviewCommands {
	command: 'scrollTo' | 'download' | 'open-details';
	scrollToLetter?: string;
	downloadMediaCommands?: DownloadMediaDTO[];
	mediaId?: number;
}

export function resetMediaOverviewCommandsBus(): void {
	return useMediaOverviewCommandsBus().reset();
}

export function useMediaOverviewCommandsBus(): UseEventBusReturn<IMediaOverviewCommands, any> {
	// Do not set this to a constant, it will cause issues with the event bus.
	return useEventBus<IMediaOverviewCommands>('mediaOverViewCommands');
}

// endregion

// region  ScrollTo command

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

// endregion

/**
 * This is used to send a command to from the MediaOverviewBar to trigger a download command.
 */
export function useMediaOverviewBarDownloadCommandBus(): UseEventBusReturn<string, any> {
	return useEventBus<string>('downloadCommand');
}

// region Download command

export function sendMediaOverviewDownloadCommand(downloadMediaCommands: DownloadMediaDTO[]): void {
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

// region Open Details

export function sendMediaOverviewOpenDetailsCommand(mediaId: number): void {
	useMediaOverviewCommandsBus().emit({
		command: 'open-details',
		mediaId,
	});
}

export function listenMediaOverviewOpenDetailsCommand(action: (mediaId: number) => void): void {
	useMediaOverviewCommandsBus().on(({ command, mediaId }) => {
		if (command === 'open-details') {
			action(mediaId ?? -1);
		}
	});
}

// endregion

// endregion
