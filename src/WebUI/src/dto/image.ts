export default class Image {
	name: string = '';

	src: string = '';

	public constructor(init?: Partial<Image>) {
		Object.assign(this, init);
	}
}
