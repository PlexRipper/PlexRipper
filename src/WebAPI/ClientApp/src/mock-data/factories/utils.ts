import { rand } from '@ngneat/falso';
import type { PlexMediaType } from '@dto/mainApi';

export function randPlexMediaType(): PlexMediaType {
	return rand([PlexMediaType.Movie, PlexMediaType.TvShow]);
}
