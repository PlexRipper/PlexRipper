import type { ResultDTO } from '@interfaces';

export interface IAlert {
	id: number;
	title: string;
	text: string;
	result?: ResultDTO;
}
