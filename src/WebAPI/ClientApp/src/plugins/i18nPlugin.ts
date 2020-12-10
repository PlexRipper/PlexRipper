import { Context } from '@nuxt/types';
import VueI18n from 'vue-i18n';
import { Inject } from '@nuxt/types/app';

export default (ctx: Context, inject: Inject): void => {
	// Doc: https://nuxtjs.org/guide/plugins/
	// Create a simple function to retrieve the entire translation messages object;
	inject('messages', (): VueI18n.LocaleMessageObject => messages(ctx));

	inject('ts', (path: string): string => ctx.app.i18n.t(path).toString());

	const objectPath = require('object-path');
	inject('getMessage', (path: string): any => objectPath.get(messages(ctx), path));
};

function messages(ctx: Context) {
	return ctx.app.i18n.messages[ctx.app.i18n.locale];
}
