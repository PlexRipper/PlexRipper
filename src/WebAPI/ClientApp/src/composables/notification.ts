import { Notify } from 'quasar';

export function showErrorNotification(message: string) {
	Notify.create({
		type: 'negative',
		message,
	});
}
