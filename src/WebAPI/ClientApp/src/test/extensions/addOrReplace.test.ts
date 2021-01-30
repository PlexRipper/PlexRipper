test('add element to array', () => {
	let testArray: string[] = [];

	testArray = testArray.addOrReplace((x) => x === 'unknown', 'unknown');

	expect(testArray.length).toBe(1);
});
