import { Context } from '@nuxt/types';

export default interface ISetup {
	setup(nuxtContext: Context, callBack: (name: string) => void): void;
}
