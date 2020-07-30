export default interface IFolderPath {
	id: number;
	displayName?: string;
	type: string;
	directory: string;
	freespace?: number;
	unmappedfolders?: number;
	size?: number;
}
