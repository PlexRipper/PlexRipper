import { faker } from '@faker-js/faker';
import { checkConfig, incrementSeed } from './mock-base';
import { MockConfig } from '@mock/interfaces';
import { FolderPathDTO, PlexLibraryDTO, PlexMediaType } from '@dto/mainApi';

export function generatePlexLibraries(config: Partial<MockConfig> = {}): PlexLibraryDTO[] {
	const configValid = checkConfig(config);

	const plexLibraries: PlexLibraryDTO[] = [];

	for (let i = 0; i < configValid.plexLibraryCount; i++) {
		incrementSeed(i);
		const type = faker.helpers.arrayElement<PlexMediaType>(configValid.plexLibraryTypes);

		const library: PlexLibraryDTO = {
			id: i + 1,
			type,
			title: faker.company.name(),
			key: faker.datatype.number(99999).toString(),
			libraryLocationPath: faker.system.directoryPath(),
			defaultDestination: {} as FolderPathDTO,
			createdAt: faker.date.past().toUTCString(),
			updatedAt: faker.date.recent().toUTCString(),
			scannedAt: faker.date.recent().toUTCString(),
			syncedAt: faker.date.recent().toUTCString(),
			uuid: faker.datatype.uuid(),
			count: 0,
			libraryLocationId: 0,
			defaultDestinationId: 0,
			downloadTasks: [],
			episodeCount: 0,
			mediaSize: 0,
			movies: [],
			outdated: false,
			plexServerId: 0,
			seasonCount: 0,
			tvShows: [],
		};

		if (type === PlexMediaType.Movie) {
			library.movies = [];
		}
		if (type === PlexMediaType.TvShow) {
			library.tvShows = [];
		}

		plexLibraries.push(library);
	}

	return plexLibraries;
}
