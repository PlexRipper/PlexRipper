import { PlexMediaType } from '@dto/mainApi';

export interface IObjectUrl {
	id: number;
	type: PlexMediaType;
	url: string;
}
