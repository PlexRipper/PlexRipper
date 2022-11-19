module.exports = {
	env: {
		test: {
			presets: [
				['@vue/cli-plugin-babel/preset'],
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
