import { defineNuxtPlugin } from '#app';
import { Composer, LocaleMessageObject, VueI18n } from 'vue-i18n';
import objectPath from 'object-path';

export default defineNuxtPlugin((nuxtApp) => {
	// Doc: https://i18n.nuxtjs.org/

	const ctx = nuxtApp.$i18n as VueI18n | Composer;

	function messages(ctx: VueI18n | Composer) {
		return ctx.messages[ctx.locale.toString()];
	}

	return {
		provide: {
			messages: (): LocaleMessageObject => messages(ctx),
			getMessage: (path: string): any => objectPath.get(messages(ctx), path),
		},
	};
});
