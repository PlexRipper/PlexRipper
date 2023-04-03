import { useEventBus, UseEventBusReturn } from '@vueuse/core';
import { DownloadMediaDTO } from '@dto/mainApi';

export interface IMediaOverviewBarBus {
	downloadButtonVisible: boolean;
}

export function useMediaOverviewBarBus(): UseEventBusReturn<IMediaOverviewBarBus, any> {
	return useEventBus<IMediaOverviewBarBus>('mediaOverViewBarBus');
}

/**
 * This is used to send a command to from the MediaOverviewBar to trigger a download command.
 */
export function useMediaOverviewBarDownloadCommandBus(): UseEventBusReturn<string, any> {
	return useEventBus<string>('downloadCommand');
}

export function useProcessDownloadCommandBus(): UseEventBusReturn<DownloadMediaDTO[], any> {
	return useEventBus<DownloadMediaDTO[]>('processDownloadCommand');
}

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
