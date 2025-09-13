import {
	rand,
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
import {
	PlexAccessState,
	type PlexAccountDTO, type PlexLibraryAccessRapportDTO,
	type PlexLibraryDTO,
	type PlexServerAccessRapportDTO,
	type PlexServerDTO,
	type RefreshPlexAccountAccessRapportDTO,
} from '@dto';
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
		apiAuthenticationToken: '',
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

export function generateRefreshPlexAccountAccessRapportDTO({ plexAccounts, plexServers, plexLibraries }: { plexAccounts: PlexAccountDTO[]; plexServers: PlexServerDTO[]; plexLibraries: PlexLibraryDTO[] }) {
	return plexAccounts.map((account): RefreshPlexAccountAccessRapportDTO => ({
		plexAccountId: account.id,
		plexAccountName: account.displayName,
		access: plexServers.map((server): PlexServerAccessRapportDTO => {
			const isServerOffline = randBoolean();
			return ({
				plexServerId: server.id,
				isServerOffline,
				plexServerName: server.name,
				state: isServerOffline ? PlexAccessState.Unknown : rand([PlexAccessState.Granted, PlexAccessState.Updated, PlexAccessState.Revoked]),
				libraryAccess: isServerOffline
					? []
					: plexLibraries.filter((library) => library.plexServerId == server.id).map((library): PlexLibraryAccessRapportDTO => ({
							plexLibraryName: library.title,
							plexLibraryId: library.id,
							plexServerId: library.plexServerId,
							state: rand([PlexAccessState.Granted, PlexAccessState.Updated, PlexAccessState.Revoked]),
						})),
			});
		}),
	}));
}
