import { Context } from '@nuxt/types';
import VueI18n from 'vue-i18n';

export default (ctx: Context): void => {
	// Create a simple function to retrieve the entire translation messages object;
	ctx.$messages = function (): VueI18n.LocaleMessageObject {
		return ctx.app.i18n.messages[ctx.app.i18n.locale];
	};
};
