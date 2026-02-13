import { randRecentDate } from '@ngneat/falso';
import {	LibrarySyncJobStatus } from '@dto';

import type {
	LibrarySyncJobQueueDTO,
	LibrarySyncProgressDTO,
	LibrarySyncProgressItemDTO,
	PlexMediaType } from '@dto';

export function generateLibrarySyncProgress({
	libraryId,
	received,
	total,
	items,
}: {
	libraryId: number;
	type: PlexMediaType;
	received: number;
	total: number;
	items: LibrarySyncProgressItemDTO[];
}): LibrarySyncProgressDTO {
	const percentage = Math.round((received / total) * 100);
	const remainingPercentage = 100 - percentage;
	const timeRemainingInSeconds = Math.round(remainingPercentage / 10);

	return {
		plexLibraryId: libraryId,
		received,
		total,
		isComplete: received === total,
		timeStamp: randRecentDate().toISOString(),
		percentage,
		timeRemaining: generateTimeRemaining(timeRemainingInSeconds * 1000),
		items: items,
		errors: [],
	};
}

export function generateTimeRemaining(remainingMs: number): string {
	const remainingSecs = Math.floor(remainingMs / 1000);
	const hours = Math.floor(remainingSecs / 3600).toString().padStart(2, '0');
	const minutes = Math.floor((remainingSecs % 3600) / 60).toString().padStart(2, '0');
	const seconds = (remainingSecs % 60).toString().padStart(2, '0');
	return `${hours}:${minutes}:${seconds}`;
}

export function generateLibrarySyncProgressItem(mediaType: PlexMediaType, partial: Partial<LibrarySyncProgressItemDTO> = {}): LibrarySyncProgressItemDTO {
	const dto: LibrarySyncProgressItemDTO = {
		mediaType,
		isComplete: false,
		received: 0,
		percentage: 0,
		timeRemaining: '00:00:00',
		total: 0,
		...partial,
	};

	dto.percentage = Math.round((dto.received / dto.total) * 100);
	dto.isComplete = dto.received === dto.total;

	return dto;
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
