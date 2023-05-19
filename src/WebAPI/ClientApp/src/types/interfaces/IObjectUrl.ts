import { PlexMediaType } from '@dto/mainApi';

export default interface IObjectUrl {
	id: number;
	type: PlexMediaType;
	url: string;
}
