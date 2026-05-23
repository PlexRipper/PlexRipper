export enum LibraryAccessTimelineState {
	Accessible = 'Accessible',
	LostAccess = 'LostAccess',
}

export interface ILibraryAccessTimelineEvent {
	plexAccountId: number;
	plexAccountName: string;
	plexServerId: number | null;
	plexServerName: string;
	plexLibraryId: number | null;
	plexLibraryName: string;
	state: 'Granted' | 'Revoked' | 'Updated' | 'Unknown' | number;
	occurredAtUtc: string;
	refreshRunId: string;
}

export interface ILibraryAccessCurrentState {
	plexAccountId: number;
	plexAccountName: string;
	plexServerId: number | null;
	plexServerName: string;
	plexLibraryId: number | null;
	plexLibraryName: string;
	currentState: LibraryAccessTimelineState | string;
	grantedAt: string | null;
	revokedAt: string | null;
	lastChangedAt: string;
}

export interface ILibraryAccessTimelineResponse {
	events: ILibraryAccessTimelineEvent[];
	currentState: ILibraryAccessCurrentState[];
}

export interface ILibraryAccessTimelineInterval {
	id: string;
	rowId: string;
	accountName: string;
	serverName: string;
	libraryName: string;
	grantedAt: string;
	revokedAt: string | null;
	start: Date;
	end: Date;
	durationLabel: string;
}

export interface ILibraryAccessTimelineServerGroup {
	id: string;
	name: string;
	collapsed: boolean;
	intervals: ILibraryAccessTimelineInterval[];
}
