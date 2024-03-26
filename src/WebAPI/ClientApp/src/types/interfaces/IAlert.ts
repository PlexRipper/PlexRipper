import type ResultDTO from '@dto/ResultDTO';

export default interface IAlert {
	id: number;
	title: string;
	text: string;
	result?: ResultDTO;
}
