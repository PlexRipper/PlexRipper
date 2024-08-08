import { randRecentDate } from '@ngneat/falso';
import type { LibraryProgress, SyncServerProgress } from '@dto';
import { times, mean } from 'lodash-es';

export function generateSyncServerProgress({
	progressIndex = 0,
	plexServerId,
	plexLibraryIds,
}: {
	/**
	 * The progress of the server
	 */
	progressIndex: number;
	plexServerId: number;
	plexLibraryIds: number[];
}): SyncServerProgress {
	const progress = times(plexLibraryIds.length, (i) => generateLibraryProgress({ libraryId: plexLibraryIds[i], received: progressIndex * 100, total: 1000 }));
	return {
		id: plexServerId,
		libraryProgresses: progress,
		percentage: mean(progress.map((x) => x.percentage)) * 100,
	};
}

export function generateLibraryProgress({
	libraryId,
	received,
	total,
}: {
	libraryId: number;
	received: number;
	total: number;
}): LibraryProgress {
	return {
		id: libraryId,
		received,
		total,
		isComplete: received === total,
		isRefreshing: received !== total,
		timeStamp: randRecentDate().toISOString(),
		percentage: Math.round((received / total) * 100) / 100,
	};
}