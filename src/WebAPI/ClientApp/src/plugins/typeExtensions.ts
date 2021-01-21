export default (): void => {
	Array.prototype.addOrReplace = function (index: number, object: any) {
		if (this.length > index) {
			this.push(object);
		} else {
			this.splice(index, 1, object);
		}

		return this;
	};
};
