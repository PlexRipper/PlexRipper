import { rand, randNumber, seed } from '@ngneat/falso';
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

export class Seed {
	private seed = 1;

	constructor(seed: number) {
		this.seed = seed;
	}

	next(): number {
		this.seed++;
		seed(this.seed + '');
		return this.seed;
	}
}
