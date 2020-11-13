import VueI18n from 'vue-i18n';
import Vue from 'vue';

declare module 'vue/types/vue' {
	interface Vue {
		$messages(): VueI18n.LocaleMessageObject;
	}
}
declare module 'vue/types/options' {
	interface ComponentOptions<V extends Vue> {
		$messages(): VueI18n.LocaleMessageObject;
	}
}

declare module '@nuxt/types' {
	interface Context {
		$messages(): VueI18n.LocaleMessageObject;
	}
}
