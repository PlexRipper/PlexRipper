export default class GateButton {
	category: string = '';

	name: string = '';

	public constructor(init?: Partial<GateButton>) {
		Object.assign(this, init);
	}
}
