import { randCompanyName, randDirectoryPath, randNumber, randRecentDate, randUuid } from '@ngneat/falso';
import { randPlexMediaType } from './';
import { checkConfig, incrementSeed, MockConfig } from '@mock';
import { FolderPathDTO, PlexLibraryDTO } from '@dto/mainApi';

let plexLibraryIdIndex = 1;

export function generatePlexLibrary({
	id,
	plexServerId,
	config = {},
	partialData = {},
}: {
	id: number;
	plexServerId: number;
	partialData?: Partial<PlexLibraryDTO>;
	config?: Partial<MockConfig>;
}): PlexLibraryDTO {
	checkConfig(config);
	incrementSeed(id);
	const libraryType = randPlexMediaType();
	return {
		id,
		type: libraryType,
		title: randCompanyName(),
		key: '' + randNumber({ max: 999999 }),
		libraryLocationPath: randDirectoryPath(),
		defaultDestination: {} as FolderPathDTO,
		createdAt: randRecentDate({ days: 60 }).toUTCString(),
		updatedAt: randRecentDate({ days: 10 }).toUTCString(),
		scannedAt: randRecentDate({ days: 10 }).toUTCString(),
		syncedAt: randRecentDate({ days: 10 }).toUTCString(),
		uuid: randUuid(),
		count: 0,
		libraryLocationId: 0,
		defaultDestinationId: 0,
		downloadTasks: [],
		episodeCount: 0,
		mediaSize: 0,
		movies: [],
		outdated: false,
		plexServerId,
		seasonCount: 0,
		tvShows: [],
		...partialData,
	};
}

export function generatePlexLibraries({
	plexServerId,
	config = {},
	partialData = {},
}: {
	plexServerId: number;
	partialData?: Partial<PlexLibraryDTO>;
	config?: Partial<MockConfig>;
}): PlexLibraryDTO[] {
	const validConfig = checkConfig(config);
	return Array(validConfig.plexLibraryCount)
		.fill(null)
		.map(() => generatePlexLibrary({ id: plexLibraryIdIndex++, plexServerId, partialData }));
}
