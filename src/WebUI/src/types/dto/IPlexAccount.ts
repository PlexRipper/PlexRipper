import IPlexServer from './IPlexServer';

export interface IPlexAccount {
	id: number;
	uuid: string;
	email: string;
	joinedAt: Date;
	username: string;
	title: string;
	hasPassword: boolean;
	authToken: string;
	authenticationToken: string;
	confirmedAt: Date;
	forumId: number;
	plexServers: IPlexServer[];
}
