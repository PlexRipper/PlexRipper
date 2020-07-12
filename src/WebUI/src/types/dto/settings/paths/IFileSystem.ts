export interface IFileSystem {
	parent: string;
	directories: IDirectory[];
	files: IFile[];
}

export interface IDirectory {
	type: number;
	name: string;
	path: string;
	extension?: string;
	size?: number;
	lastModified?: Date;
}

export interface IFile {
	type: number;
	name: string;
	path: string;
	extension: string;
	size: number;
	lastModified: Date;
}
