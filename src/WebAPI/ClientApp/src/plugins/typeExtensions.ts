export default (): void => {
	Array.prototype.addOrReplace = function (index: number, object: any) {
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
