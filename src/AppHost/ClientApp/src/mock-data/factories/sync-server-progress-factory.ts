import { randRecentDate } from '@ngneat/falso';
import type { LibraryProgress, SyncServerMediaProgress } from '@dto';
import { times, mean } from 'lodash-es';

export function generateSyncServerMediaProgress({
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
}): SyncServerMediaProgress {
	const progress = times(plexLibraryIds.length, (i) => generateLibraryProgress({
		libraryId: plexLibraryIds[i],
		received: progressIndex * 100,
		total: 1000,
	}));
	return {
		serverId: plexServerId,
		libraryProgresses: progress,
		percentage: mean(progress.map((x) => x.percentage)),
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
	const percentage = Math.round((received / total) * 100);
	const remainingPercentage = 100 - percentage;
	const timeRemainingInSeconds = Math.round(remainingPercentage / 10);

	// Convert the remaining seconds into HH:MM:SS format
	const hours = Math.floor(timeRemainingInSeconds / 3600);
	const minutes = Math.floor((timeRemainingInSeconds % 3600) / 60);
	const seconds = timeRemainingInSeconds % 60;

	// Format the time as HH:MM:SS
	const formattedTimeRemaining = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

	return {
		id: libraryId,
		received,
		total,
		isComplete: received === total,
		isRefreshing: received !== total,
		timeStamp: randRecentDate().toISOString(),
		percentage,
		timeRemaining: formattedTimeRemaining,
		step: 1,
		totalSteps: 1,
	};
}
