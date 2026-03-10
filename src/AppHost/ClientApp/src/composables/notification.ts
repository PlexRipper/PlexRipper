import { Notify } from 'quasar';

export function showErrorNotification(message: string, timeout: number = 0) {
	Notify.create({
		type: 'negative',
		message,
		progress: true,
		timeout,
		actions: [
			{ icon: 'mdi-close', color: 'white', round: true, handler: () => { } },
		],
	});
}

export function showSuccessNotification(message: string, timeout: number = 0) {
	Notify.create({
		type: 'positive',
		message,
		progress: true,
		timeout,
		actions: [
			{ icon: 'mdi-close', color: 'white', round: true, handler: () => { } },
		],
	});
}
