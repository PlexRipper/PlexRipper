export default interface IPlexLibrary {
	key: string;
	title: string;
	type: string;
	updatedAt: Date;
	createdAt: Date;
	scannedAt: Date;
	contentChangedAt: Date;
	uuid: string;
	libraryLocationId: number;
	libraryLocationPath: string;
	plexServerId: number;
}
