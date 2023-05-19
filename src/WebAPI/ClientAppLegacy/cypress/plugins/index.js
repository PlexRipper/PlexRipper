module.exports = (on, config) => {
	on('before:browser:launch', (browser = {}, args) => {
		if (browser.name === 'chromium') {
			const newArgs = args.filter((arg) => arg !== '--disable-gpu');
			newArgs.push('--ignore-gpu-blacklist');
			return newArgs;
		}
	});
};
