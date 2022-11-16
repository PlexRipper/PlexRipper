module.exports = {
	env: {
		test: {
			presets: [
				['@vue/cli-plugin-babel/preset'],
				['@babel/preset-typescript'],
				[
					'@babel/preset-env',
					{
						targets: {
							node: 'current',
						},
					},
				],
			],
		},
	},
};
