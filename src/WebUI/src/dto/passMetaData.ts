export default class PassMetaData {
	id: number = 0;

	created: Date = new Date();

	createdBy?: string;

	lastChanged?: Date;

	lastChangedBy?: string;
}
