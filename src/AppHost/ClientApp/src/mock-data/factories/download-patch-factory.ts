import { randNumber, randUuid } from '@ngneat/falso';
import type { DownloadPatchDTO, DownloadPatchMessagePackDTO } from '@dto';
import { DownloadStatus } from '@dto';
import type { DownloadPatchEntryMessagePackTuple, DownloadPatchMessagePackTuple } from '@interfaces';

function toDecimalString(value: number): string {
	return value.toFixed(2);
}

export function generateDownloadPatchMessagePackDTO({
	serverId = randNumber({ min: 1, max: 9999 }),
	sequence = randNumber({ min: 1, max: 100_000 }),
	upsertCount = 2,
	deletedCount = 1,
}: {
	serverId?: number;
	sequence?: number;
	upsertCount?: number;
	deletedCount?: number;
} = {}): DownloadPatchMessagePackDTO {
	const upserts = Array.from({ length: upsertCount }, (): DownloadPatchDTO => ({
		id: randUuid(),
		parentId: randUuid(),
		status: DownloadStatus.Downloading,
		percentage: Number(toDecimalString(randNumber({ min: 1, max: 99.99, fraction: 2 }))),
		dataReceived: randNumber({ min: 1, max: 2_000_000_000 }),
		dataTotal: randNumber({ min: 2_000_000_001, max: 4_000_000_000 }),
		downloadSpeed: randNumber({ min: 1, max: 100_000_000 }),
		timeRemaining: randNumber({ min: 1, max: 1_000_000 }),
	}));

	const deletedIds = Array.from({ length: deletedCount }, () => randUuid());

	return {
		serverId,
		sequence,
		upserts,
		deletedIds,
	};
}

export function toDownloadPatchMessagePackTuple(
	patch: DownloadPatchMessagePackDTO,
	options: { percentageAsString?: boolean } = {},
): DownloadPatchMessagePackTuple {
	const upserts: DownloadPatchEntryMessagePackTuple[] = patch.upserts.map((upsert) => [
		upsert.id,
		upsert.parentId,
		upsert.status,
		options.percentageAsString ? String(upsert.percentage) : upsert.percentage,
		upsert.dataReceived,
		upsert.dataTotal,
		upsert.downloadSpeed,
		upsert.timeRemaining,
	]);

	return [
		patch.serverId,
		patch.sequence,
		upserts,
		patch.deletedIds,
	];
}
