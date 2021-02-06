module.exports = {
	preset: 'ts-jest',
	testEnvironment: 'node',
	moduleNameMapper: {
		'^@/(.*)$': '<rootDir>/src/$1',
		'^~/(.*)$': '<rootDir>/src/$1',
		'^vue$': 'vue/dist/vue.common.js',
	},
	moduleFileExtensions: ['ts', 'js', 'vue', 'json'],
	transform: {
		'^.+\\.ts$': 'ts-jest',
		'^.+\\.js$': 'babel-jest',
		'.*\\.(vue)$': 'vue-jest',
	},
	collectCoverage: true,
	collectCoverageFrom: ['<rootDir>/src/components/**/*.vue', '<rootDir>/src/pages/**/*.vue'],
};
