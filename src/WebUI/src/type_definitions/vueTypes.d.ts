// This is meant to hide false typescript errors
declare module 'vanta/dist/vanta.waves.min' {
	interface Waves {
		constructor(userOptions: any): any;
	}
}
