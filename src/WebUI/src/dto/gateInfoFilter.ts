export default class GateInfoFilter {
	showOnlyErrors: boolean = true;

	public constructor(init?: Partial<GateInfoFilter>) {
		Object.assign(this, init);
	}
}
