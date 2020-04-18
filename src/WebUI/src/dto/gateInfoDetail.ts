export default class GateInfoDetail {
	name: string = '';

	value: string = '';

	editable: boolean = false;

	column: number = 0;

	order: number = 0;

	public constructor(init?: Partial<GateInfoDetail>) {
		Object.assign(this, init);
	}
}
