import { randRecentDate } from '@ngneat/falso';
import { type LibraryProgress, type LibrarySyncJobQueueDTO, LibrarySyncJobStatus } from '@dto';

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

export function generateLibrarySyncJobQueue({
	plexLibraryId,
	plexServerId,
	status = LibrarySyncJobStatus.Queued,
	priority = 1,
	isServerOffline = false,
	errorMessage = null,
	createdAt,
	startedAt = null,
	completedAt = null,
}: {
	plexLibraryId: number;
	plexServerId: number;
	status?: LibrarySyncJobStatus;
	priority?: number;
	isServerOffline?: boolean;
	errorMessage?: string | null;
	createdAt?: string;
	startedAt?: string | null;
	completedAt?: string | null;
}): LibrarySyncJobQueueDTO {
	return {
		plexLibraryId,
		plexServerId,
		status,
		priority,
		isServerOffline,
		errorMessage,
		createdAt: createdAt ?? new Date().toISOString(),
		startedAt,
		completedAt,
	};
}
