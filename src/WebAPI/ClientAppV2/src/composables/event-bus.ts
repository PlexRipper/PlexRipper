import { useEventBus, UseEventBusReturn } from '@vueuse/core';
import { DownloadMediaDTO, PlexMediaSlimDTO } from '@dto/mainApi';

export interface IMediaOverviewBarBus {
	downloadButtonVisible: boolean;
}

export interface IProcessDownloadCommandBus {
	command: DownloadMediaDTO[];
	items: PlexMediaSlimDTO[];
}

export function mediaOverviewBarBus(): UseEventBusReturn<IMediaOverviewBarBus, any> {
	return useEventBus<IMediaOverviewBarBus>('mediaOverViewBarBus');
}

export function useMediaOverviewCommandBus(): UseEventBusReturn<string, any> {
	return useEventBus<string>('downloadCommand');
}

export function useProcessDownloadCommandBus(): UseEventBusReturn<IProcessDownloadCommandBus, any> {
	return useEventBus<IProcessDownloadCommandBus>('processDownloadCommand');
}
