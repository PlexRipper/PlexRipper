import { rand, seed } from '@ngneat/falso';
import { PlexMediaType } from '@dto/mainApi';

export function resetSeed() {
	seed();
}

export function randPlexMediaType(): PlexMediaType {
	return rand([PlexMediaType.Movie, PlexMediaType.TvShow]);
}
