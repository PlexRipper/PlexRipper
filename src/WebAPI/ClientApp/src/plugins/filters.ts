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
			case PlexMediaType.Movie:
				return 'mdi-filmstrip';
			case PlexMediaType.Music:
				return 'mdi-music';
			default:
				return 'mdi-help-circle-outline';
		}
	});

	/*
	 * Filesize filters
	 * Source: https://github.com/sainf/vue-filter-pretty-bytes
	 */
	Vue.filter('prettyBytes', (bytes: number, decimals: number, kib: string | boolean) => {
		kib = kib || false;
		if (bytes === 0) return '0 Bytes';
		if (isNaN(bytes) && !isFinite(bytes)) return 'Unknown';
		const k = kib ? 1024 : 1000;
		const dm = decimals != null && !isNaN(decimals) && decimals >= 0 ? decimals : 2;
		const sizes = kib
			? ['Bytes', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB', 'BiB']
			: ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB', 'BB'];
		const i = Math.floor(Math.log(bytes) / Math.log(k));

		return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
	});
};
