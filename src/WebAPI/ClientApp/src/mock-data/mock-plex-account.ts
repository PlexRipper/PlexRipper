import { faker } from '@faker-js/faker';
import { MockConfig } from '@mock/interfaces/MockConfig';
import ResultDTO from '@dto/ResultDTO';
import { PlexAccountDTO } from '@dto/mainApi';
import { checkConfig } from '@mock/mock-base';
import { generatePlexServers } from '@mock/mock-plex-server';

export function generatePlexAccounts(config: MockConfig | null = null): PlexAccountDTO[] {
	config = checkConfig(config);

	const plexAccounts: PlexAccountDTO[] = [];
	// @ts-ignore
	for (let i = 0; i < config.plexAccountCount; i++) {
		plexAccounts.push({
			id: i + 1,
			displayName: faker.internet.email(),
			username: faker.internet.email(),
			password: faker.internet.password(),
			isEnabled: true,
			isValidated: true,
			validatedAt: faker.date.recent().toUTCString(),
			plexId: faker.datatype.number(99999),
			uuid: faker.datatype.uuid(),
			clientId: faker.datatype.uuid(),
			title: faker.internet.userName(),
			email: faker.internet.email(),
			hasPassword: true,
			authenticationToken: faker.datatype.uuid(),
			isMain: true,
			is2Fa: false,
			verificationCode: '',
			plexServers: generatePlexServers(config),
		});
	}

	return plexAccounts;
}
