import Vue from 'vue';
import Log from 'consola';
import { PlexMediaType } from '@dto/mainApi';

export default (): void => {
	Log.debug('Setup Vue filters');

	/*
	 * String filters
	 */
	Vue.filter('substr', (value: string, limit: number) => {
		if (value?.length > limit) {
			return `${value.substring(0, limit)}...`;
		}
		return value;
	});

	Vue.filter('mediaTypeIcon', (type: PlexMediaType): string => {
		switch (type) {
			case PlexMediaType.TvShow:
				return 'mdi-television-classic';
			case PlexMediaType.Season:
				return 'mdi-play-box-multiple';
			case PlexMediaType.Episode:
				return 'mdi-movie-open';
			case PlexMediaType.Movie:
				return 'mdi-filmstrip';
			case PlexMediaType.Music:
				return 'mdi-music';
			default:
				return 'mdi-help-circle-outline';
		}
	});
};
