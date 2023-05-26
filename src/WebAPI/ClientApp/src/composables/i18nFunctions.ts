import { get as objGet } from 'object-path';
import { useI18n } from '#imports';

export function getMessages() {
	const i18n = useI18n();

	// @ts-ignore
	return i18n.messages.value[i18n.locale.value];
}

export function getMessage(path: string): any {
	return objGet(getMessages(), path);
}
