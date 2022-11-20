module.exports = {
	env: {
		test: {
			presets: [
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
