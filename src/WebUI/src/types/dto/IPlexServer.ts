import IPlexLibrary from './IPlexLibrary';

export default interface IPlexServer {
	id: number;
	accessToken: string;
	name: string;
	address: string;
	port: number;
	version: string;
	scheme: string;
	host: string;
	localAddresses: string;
	machineIdentifier: string;
	createdAt: Date;
	updatedAt: Date;
	owned: boolean;
	synced: boolean;
	ownerId: number;
	home: boolean;
	plexLibraries: IPlexLibrary[];
}
