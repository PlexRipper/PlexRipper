import * as signalR from '@aspnet/signalr';
import Log from 'consola';
import { signalRDownloadProgressUrl } from '@api/baseApi';
import { SignalrStore } from '@/store/';

export default async function addSignalR(): Promise<void> {
	try {
		let signalRConnections: signalR.HubConnection[] = [];

		Log.debug('Setting up SignalR connections...');

		// The connection for communicating the download progress
		signalRConnections.push(
			new signalR.HubConnectionBuilder()
				.withUrl(signalRDownloadProgressUrl)
				.configureLogging(signalR.LogLevel.Information) // TODO determine for environments
				.build(),
		);

		// Start all SignalR connections
		await Promise.all(
			signalRConnections.map((x) =>
				x.start().catch((err) => {
					Log.error(err);
				}),
			),
		);

		signalRConnections = signalRConnections.filter((x) => x.state.valueOf() === 1);

		// Add connections to the store
		SignalrStore.setConnections(signalRConnections);

		// Send out event that components can start using their signalR connections
		const event: Event = new Event('signalRSetup');
		dispatchEvent(event);

		Log.debug('SignalR connection done!');
	} catch (error) {
		Log.error(error);
	}
}
