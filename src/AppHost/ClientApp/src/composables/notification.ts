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
