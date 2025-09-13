import type { Composer, UseI18nOptions } from 'vue-i18n';

export type I18nObjectType = Composer<
	NonNullable<UseI18nOptions['messages']>,
	NonNullable<UseI18nOptions['datetimeFormats']>,
	NonNullable<UseI18nOptions['numberFormats']>,
	UseI18nOptions['locale'] extends unknown ? string : UseI18nOptions['locale']
>;
