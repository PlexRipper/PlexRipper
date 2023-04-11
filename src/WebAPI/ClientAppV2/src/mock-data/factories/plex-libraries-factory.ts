import { faker } from '@faker-js/faker';
import * as Factory from 'factory.ts';
import { randCompanyName, randNumber, randRecentDate, randUuid } from '@ngneat/falso';
import { randPlexMediaType } from './';
import { checkConfig, MockConfig } from '@mock';
import { FolderPathDTO, PlexLibraryDTO } from '@dto/mainApi';

export function generatePlexLibraries(plexServerId: number, config: Partial<MockConfig> = {}): PlexLibraryDTO[] {
	const configValid = checkConfig(config);

	const plexLibraryDTOFactory = Factory.Sync.makeFactory<PlexLibraryDTO>(() => {
		const libraryType = randPlexMediaType();
		return {
			id: Factory.each((i) => i),
			type: libraryType,
			title: randCompanyName(),
			key: '' + randNumber({ max: 999999 }),
			libraryLocationPath: faker.system.directoryPath(),
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
			plexServerId: -1,
			seasonCount: 0,
			tvShows: [],
		};
	});

	return plexLibraryDTOFactory.buildList(configValid.plexLibraryCount, { plexServerId });
}
