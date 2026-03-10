import type { DownloadStatus, PlexMediaType } from '@dto';

export type DownloadPatchEntryMessagePackTuple = [
	string,
	string,
	DownloadStatus,
	number | string,
	number,
	number,
	number,
	number,
];

export type DownloadPatchMessagePackTuple = [
	number,
	number,
	DownloadPatchEntryMessagePackTuple[],
	string[],
];

export type ServerDownloadEntryMessagePackTuple = [
	string,
	string,
	PlexMediaType,
	DownloadStatus,
	number | string,
	number,
	number,
	number,
	number,
	Array<ServerDownloadEntryMessagePackTuple | unknown>,
];

export type ServerDownloadProgressMessagePackTuple = [
	number,
	number,
	Array<ServerDownloadEntryMessagePackTuple | unknown>,
];
