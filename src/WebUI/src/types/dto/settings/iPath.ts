import IPathType from './iPathType';

export default interface IPath {
	id: number;
	path: string;
	type: IPathType;
	name?: string;
	freespace?: number;
	unmappedfolders?: number;
	size?: number;
}
