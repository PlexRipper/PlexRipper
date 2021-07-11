import { ResultDTO } from '@dto/mainApi';

export default interface IAlert {
	id: number;
	title: string;
	text: string;
	result?: ResultDTO;
}
