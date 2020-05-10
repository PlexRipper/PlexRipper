import { IPlexAccount } from './IPlexAccount';

export default interface IAccount {
	id: number;
	displayName: string;
	username: string;
	password: string;
	isEnabled: boolean;
	isValidated: boolean;
	validatedAt: Date;
	plexAccount: IPlexAccount;
}
