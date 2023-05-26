import { cy, describe, it } from 'local-cypress';
import Log from 'consola';
import CheckServerConnectionsProgress from '@components/Progress/CheckServerConnectionsProgress.vue';
import { generatePlexServers } from '@mock';

describe('<InspectServerProgress />', () => {
	it('renders', () => {
		// see: https://test-utils.vuejs.org/guide/
		cy.mount(CheckServerConnectionsProgress).then(({ wrapper, component }) => {
			const config = {
				seed: 267398,
				plexAccountCount: 2,
				plexServerCount: 5,
			};

			const generatedPlexServers = generatePlexServers(config);
			Log.info('servers', generatedPlexServers);
			wrapper.setData({ visible: true });
			component.$data.plexServers = generatedPlexServers;
			// setData({ plexServers: generatedPlexServers });
		});
	});
});
