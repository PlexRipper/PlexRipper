import { PlexMediaType } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';

export default abstract class Convert {
	public static buttonTypeToIcon(type: ButtonType): string {
		switch (type) {
			case ButtonType.Download:
				return 'mdi-download';
			case ButtonType.Error:
				return 'mdi-alert-circle';
			case ButtonType.ExternalLink:
				return 'mdi-open-in-new';
			case ButtonType.Start:
			case ButtonType.Resume:
				return 'mdi-play';
			case ButtonType.Restart:
				return 'mdi-refresh';
			case ButtonType.Pause:
				return 'mdi-pause';
			case ButtonType.Stop:
				return 'mdi-stop';
			case ButtonType.Clear:
				return 'mdi-notification-clear-all';
			case ButtonType.Details:
				return 'mdi-chart-box-outline';
			case ButtonType.Delete:
				return 'mdi-delete';
			default:
				return '';
		}
	}

	public static mediaTypeToIcon(mediaType: PlexMediaType): string {
		switch (mediaType) {
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
	}
}
