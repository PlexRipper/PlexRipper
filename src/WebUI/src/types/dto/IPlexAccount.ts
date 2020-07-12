import IPlexServer from './IPlexServer';

export default interface IPlexAccount {
	id: number;
	displayName: string;
	username: string;
	password: string;
	isEnabled: boolean;
	isValidated: boolean;
	validatedAt: Date;
	plexAccount: IPlexAccount;
	uuid: string;
	email: string;
	joinedAt: Date;
	title: string;
	hasPassword: boolean;
	authToken: string;
	authenticationToken: string;
	confirmedAt: Date;
	forumId: number;
	plexServers: IPlexServer[];
}
