import type { ResultDTO } from '@interfaces';

export default interface IAlert {
	id: number;
	title: string;
	text: string;
	result?: ResultDTO;
}
