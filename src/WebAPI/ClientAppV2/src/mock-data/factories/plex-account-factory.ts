import * as Factory from 'factory.ts';
import { randBoolean, randCompanyName, randEmail, randFullName, randPassword, randRecentDate, randUuid } from '@ngneat/falso';
import { MockConfig } from '@mock/interfaces/MockConfig';
import { PlexAccountDTO, PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { checkConfig } from '@mock/mock-base';

export function generatePlexAccounts({
	config,
	plexServers,
	plexLibraries,
}: {
	plexServers: PlexServerDTO[];
	plexLibraries: PlexLibraryDTO[];
	config: Partial<MockConfig>;
}): PlexAccountDTO[] {
	const configValid = checkConfig(config);

	if (configValid.plexAccountCount === 0) {
		return [];
	}

	const plexAccountDTOFactory = Factory.Sync.makeFactory<PlexAccountDTO>(() => {
		return {
			id: Factory.each((i) => i),
			authenticationToken: randUuid(),
			clientId: randUuid(),
			displayName: randFullName(),
			email: randEmail(),
			hasPassword: true,
			is2Fa: randBoolean(),
			isEnabled: true,
			isMain: randBoolean(),
			isValidated: randBoolean(),
			password: randPassword(),
			plexId: 0,
			title: randCompanyName(),
			username: randCompanyName(),
			uuid: randUuid(),
			validatedAt: randRecentDate({ days: 60 }).toUTCString(),
			verificationCode: '',
			plexServerAccess: plexServers.map((x) => {
				return {
					plexServerId: x.id,
					plexLibraryIds: plexLibraries.filter((y) => y.plexServerId === x.id).map((y) => y.id),
				};
			}),
		};
	});

	return plexAccountDTOFactory.buildList(configValid.plexAccountCount);
}
