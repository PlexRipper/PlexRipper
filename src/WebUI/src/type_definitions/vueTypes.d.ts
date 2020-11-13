import VueI18n from 'vue-i18n';

declare module 'vue/types/vue' {
	interface Vue {
		$messages(): VueI18n.LocaleMessageObject;
		$ts(path: string): string;
	}
}
