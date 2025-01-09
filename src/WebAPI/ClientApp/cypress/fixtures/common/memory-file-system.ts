import Log from 'consola';

interface FolderNode {
	[key: string]: FolderNode | string; // A directory can have other directories or files
}

export class MemoryFileSystem {
	private fileSystem: FolderNode = {}; // Root is a FolderNode

	// Create a directory
	createDirectory(path: string): void {
		if (!path) {
			throw new Error('Path is required to create a directory');
		}
		const parts = path.split('/').filter(Boolean);
		let current = this.fileSystem;

		for (const part of parts) {
			if (!current[part]) {
				current[part] = {}; // Create a new directory
			} else if (typeof current[part] !== 'object') {
				throw new Error(`${part} is a file, not a directory`);
			}
			current = current[part] as FolderNode; // Assert it's a FolderNode
		}
	}

	// Delete a directory or file
	delete(path: string): void {
		const parts = path.split('/').filter(Boolean);
		if (parts.length === 0) {
			throw new Error('Cannot delete root directory');
		}

		const name = parts.pop()!;
		const parent = this.getNode(parts);

		if (parent && name in parent) {
			delete parent[name];
		} else {
			throw new Error(`Path "${path}" does not exist`);
		}
	}

	// Create a file
	createFile(path: string, content: string = ''): void {
		const parts = path.split('/').filter(Boolean);
		const name = parts.pop()!;
		const parent = this.getNode(parts);

		if (parent && name in parent) {
			throw new Error(`File or directory "${name}" already exists`);
		}

		if (parent) {
			parent[name] = content; // Create a file with content
		}
	}

	// Read a file
	readFile(path: string): string {
		const parts = path.split('/').filter(Boolean);
		const name = parts.pop()!;
		const parent = this.getNode(parts);

		if (!parent || typeof parent[name] !== 'string') {
			throw new Error(`File "${path}" does not exist`);
		}

		return parent[name] as string; // Return the file content
	}

	// List contents of a directory
	listDirectory(path: string = '/'): string[] {
		const node = this.getNode(path.split('/').filter(Boolean));

		if (node && typeof node !== 'object') {
			throw new Error(`"${path}" is not a directory`);
		}

		return node ? Object.keys(node) : [];
	}

	directoryExists(path: string): boolean {
		try {
			const node = this.getNode(path.split('/').filter(Boolean));
			return !!node && typeof node === 'object';
		} catch {
			return false;
		}
	}

	// Helper: Get a node (file or directory) from the filesystem
	private getNode(parts: string[]): FolderNode | null {
		let current: FolderNode | null = this.fileSystem;

		for (const part of parts) {
			if (!current || !current[part] || typeof current[part] !== 'object') {
				Log.warn(`Path "${parts.join('/')}" does not exist`);
				return null;
			}
			current = current[part] as FolderNode;
		}

		return current;
	}
}
