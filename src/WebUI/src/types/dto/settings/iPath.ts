export default interface IPath {
	id: number;
	displayName?: string;
	type: string;
	directory: string;
	freespace?: number;
	unmappedfolders?: number;
	size?: number;
}
