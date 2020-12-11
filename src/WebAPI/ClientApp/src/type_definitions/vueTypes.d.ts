import VueI18n from 'vue-i18n';

declare module 'vue/types/vue' {
	interface Vue {
		$getThemeClass(): string;

		$messages(): VueI18n.LocaleMessageObject;
		$getMessage(path: string): object;
		$ts(path: string): string;
		$moment(path: string): string;
	}
}
