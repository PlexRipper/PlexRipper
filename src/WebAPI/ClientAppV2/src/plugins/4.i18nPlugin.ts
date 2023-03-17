import { Context } from '@nuxt/types';
import { Options } from '@nuxtjs/i18n';
import { Inject } from '@nuxt/types/app';
import VueI18n from 'vue-i18n';

export default (ctx: Context, inject: Inject): void => {
	// Doc: https://i18n.nuxtjs.org/

	// Create a simple function to retrieve the entire translation messages object;
	inject('messages', (): VueI18n.LocaleMessageObject => messages(ctx));

	inject('ts', (path: string, values?: VueI18n.Values): string => ctx.app.i18n.t(path, values).toString());

	const objectPath = require('object-path');
	inject('getMessage', (path: string): any => objectPath.get(messages(ctx), path));
};

function messages(ctx: Context) {
	return ctx.app.i18n.messages[ctx.app.i18n.locale];
}

export const NuxtI18nConfigOptions: Options = {
	lazy: true,
	langDir: './lang/',
	defaultLocale: 'en-US',
	locales: [
		{ text: 'English', code: 'en-US', iso: 'en-US', file: 'en-US.json' },
		{ text: 'Français', code: 'fr-FR', iso: 'fr-FR', file: 'fr-FR.json' },
		{ text: 'Deutsch', code: 'de-DE', iso: 'de-DE', file: 'de-DE.json' },
	],
	vueI18n: {
		fallbackLocale: 'en-US',
	},
	strategy: 'no_prefix',
};
