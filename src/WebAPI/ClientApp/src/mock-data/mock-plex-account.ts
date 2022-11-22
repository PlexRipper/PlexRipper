import { faker } from '@faker-js/faker';
import { MockConfig } from '@mock/interfaces/MockConfig';
import { PlexAccountDTO, PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { checkConfig } from '@mock/mock-base';

export function generatePlexAccounts(
	config: MockConfig | null = null,
	plexServers: PlexServerDTO[],
	plexLibraries: PlexLibraryDTO[],
): PlexAccountDTO[] {
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
			plexServerAccess: plexServers.map((x) => {
				return {
					plexServerId: x.id,
					plexLibraryIds: [], // x.plexLibraries.map((y) => y.id), TODO Fix this and give meaningful libraries back
				};
			}),
		});
	}

	return plexAccounts;
}
