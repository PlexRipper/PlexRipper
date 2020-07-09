import { Module, Mutation, VuexModule } from 'vuex-module-decorators';

// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
@Module({ name: 'signalrStore', namespaced: true, stateFactory: true })
export default class SignalrStore extends VuexModule {
	signalRConnections: signalR.HubConnection[] = [];

	get getDownloadProgressConnection(): signalR.HubConnection {
		return this.signalRConnections[0];
	}

	@Mutation
	setConnections(connections: signalR.HubConnection[]): void {
		this.signalRConnections = connections;
	}
}
