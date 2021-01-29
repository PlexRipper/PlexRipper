export default (): void => {
	Array.prototype.addOrReplace = function (searchFunction: Function, object: any) {
		const index = this.findIndex(() => searchFunction);
		if (this.length > index) {
			this.push(object);
		} else {
			this.splice(index, 1, object);
		}

		return this;
	};

	Array.prototype.sum = function (): number {
		let sum = 0;
		if (this && this.length > 0) {
			this.forEach((x) => (sum += x));
		}
		return sum;
	};
};
