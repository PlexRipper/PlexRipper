import { DownloadStatus } from '@dto';

export function determineDownloadStatus(statuses: DownloadStatus[]): DownloadStatus {
	for (const status in enumKeys(DownloadStatus)) {
		if (statuses.every((x) => x === DownloadStatus[status])) {
			return DownloadStatus[status];
		}
	}

	// Determine status based on children
	if (statuses.includes(DownloadStatus.Downloading)) {
		return DownloadStatus.Downloading;
	} else if (statuses.includes(DownloadStatus.Merging) && !statuses.includes(DownloadStatus.Downloading)) {
		return DownloadStatus.Merging;
	} else if (
		statuses.includes(DownloadStatus.Moving)
		&& !statuses.includes(DownloadStatus.Downloading)
		&& !statuses.includes(DownloadStatus.Merging)
	) {
		return DownloadStatus.Merging;
	} else if (statuses.includes(DownloadStatus.Error)) {
		return DownloadStatus.Error;
	} else {
		return DownloadStatus.Unknown;
	}
}

export function enumKeys<O extends object, K extends keyof O = keyof O>(obj: O): K[] {
	return Object.keys(obj).filter((k) => Number.isNaN(+k)) as K[];
}
