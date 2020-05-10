export interface IPlexAccount {
	id: number;
	uuid: string;
	email: string;
	joinedAt: string;
	username: string;
	title: string;
	hasPassword: boolean;
	authToken: string;
	authenticationToken: string;
	confirmedAt: string;
	forumId: number;
	accountId: number;
}
