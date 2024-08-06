import { rand, randNumber } from '@ngneat/falso';
import { PlexMediaType } from '@dto';

export function randPlexMediaType(): PlexMediaType {
	return rand([PlexMediaType.Movie, PlexMediaType.TvShow]);
}

export function randId(): number {
	return randNumber({
		min: 1,
		max: 100000000,
	});
}
