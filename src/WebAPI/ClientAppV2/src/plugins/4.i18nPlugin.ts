import { defineNuxtPlugin } from '#app';
import { Composer, LocaleMessageObject, VueI18n } from 'vue-i18n';
import { get as objGet } from 'object-path';

export default defineNuxtPlugin((nuxtApp) => {
	// Doc: https://i18n.nuxtjs.org/

	const ctx = nuxtApp.$i18n as VueI18n | Composer;

	function messages(ctx: VueI18n | Composer) {
		// @ts-ignore
		return ctx.messages.value[ctx.locale.value];
	}

	return {
		provide: {
			messages: (): LocaleMessageObject => messages(ctx),
			getMessage: (path: string): any => objGet(messages(ctx), path),
		},
	};
});
