import {
	randBoolean,
	randCompanyName,
	randEmail,
	randFullName,
	randNumber,
	randPassword,
	randRecentDate,
	randUuid,
} from '@ngneat/falso';
import { MockConfig } from '@mock/interfaces/MockConfig';
import { PlexAccountDTO, PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { checkConfig } from '@mock/mock-base';

let plexAccountIdIndex = 1;

export function generatePlexAccount({
	id,
	plexServers = [],
	plexLibraries = [],
	partialData = {},
	config = {},
}: {
	id: number;
	plexServers: PlexServerDTO[];
	plexLibraries: PlexLibraryDTO[];
	partialData?: Partial<PlexAccountDTO>;
	config?: Partial<MockConfig>;
}): PlexAccountDTO {
	const validConfig = checkConfig(config);

	return {
		id,
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
		plexId: randNumber({ min: 1, max: 10000 }),
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
		...partialData,
	};
}

export function generatePlexAccounts({
	plexServers = [],
	plexLibraries = [],
	config = {},
	partialData = {},
}: {
	plexServers: PlexServerDTO[];
	plexLibraries: PlexLibraryDTO[];
	config?: Partial<MockConfig>;
	partialData?: Partial<PlexAccountDTO>;
}): PlexAccountDTO[] {
	const validConfig = checkConfig(config);
	return Array(validConfig.plexAccountCount)
		.fill(null)
		.map(() => generatePlexAccount({ id: plexAccountIdIndex++, plexServers, plexLibraries, partialData, config }));
}
