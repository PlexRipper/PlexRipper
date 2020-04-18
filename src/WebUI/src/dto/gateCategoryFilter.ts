export default class GateCategoryFilter {
	name: string = '';

	selected: boolean = true;

	public constructor(init?: Partial<GateCategoryFilter>) {
		Object.assign(this, init);
	}
}
