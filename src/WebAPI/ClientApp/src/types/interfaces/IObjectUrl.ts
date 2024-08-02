import type { PlexMediaType } from '@dto';

export interface IObjectUrl {
	id: number;
	type: PlexMediaType;
	url: string;
}
