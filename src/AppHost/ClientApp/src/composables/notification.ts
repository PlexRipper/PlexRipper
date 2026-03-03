import { Notify } from 'quasar';

export function showErrorNotification(message: string) {
	Notify.create({
		type: 'negative',
		message,
		progress: true,
		timeout: 0,
		actions: [
			{ icon: 'mdi-close', color: 'white', round: true, handler: () => { } },
		],
	});
}

export function showSuccessNotification(message: string) {
	Notify.create({
		type: 'positive',
		message,
		progress: true,
		timeout: 2000,
	});
}
