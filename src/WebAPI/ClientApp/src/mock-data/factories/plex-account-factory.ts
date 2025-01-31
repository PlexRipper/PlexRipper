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
import { times } from 'lodash-es';
import type { MockConfig } from '@mock';
import type { PlexAccountDTO, PlexLibraryDTO, PlexServerDTO } from '@dto';
import { checkConfig, incrementSeed } from '@mock/mock-base';

let plexAccountIdIndex = 1;

export function generatePlexAccount({
	id,
	plexServers = [],
	plexLibraries = [],
	partialData = {},
	config = {},
}: {
	id: number;
	plexServers?: PlexServerDTO[];
	plexLibraries?: PlexLibraryDTO[];
	partialData?: Partial<PlexAccountDTO>;
	config?: Partial<MockConfig>;
}): PlexAccountDTO {
	checkConfig(config);
	incrementSeed(id);

	const plexServerIds = plexServers.map((x) => x.id);
	const plexLibraryIds = plexLibraries.filter((x) => plexServerIds.includes(x.plexServerId)).map((x) => x.id);

	const value: PlexAccountDTO = {
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
		password: randPassword()[0],
		plexId: randNumber({ min: 1, max: 10000 }),
		title: randCompanyName(),
		username: randCompanyName(),
		uuid: randUuid(),
		validatedAt: randRecentDate({ days: 60 }).toUTCString(),
		verificationCode: '',
		plexLibraryAccess: plexLibraryIds,
		plexServerAccess: plexServerIds,
	};

	return Object.assign(value, partialData);
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
	return times(validConfig.plexAccountCount, () =>
		generatePlexAccount({ id: plexAccountIdIndex++, plexServers, plexLibraries, partialData, config }),
	);
}
