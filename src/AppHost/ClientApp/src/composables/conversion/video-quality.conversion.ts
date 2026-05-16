import { VideoQuality } from '@dto';
import type { PlexMediaSlimDTO } from '@dto';

/**
 * Numeric rank for VideoQuality values — higher is better.
 * Used for sorting and navigation grouping.
 */
export const videoQualityRank: Record<VideoQuality, number> = {
	[VideoQuality.Unknown]: 0,
	[VideoQuality.None]: 0,
	[VideoQuality.SubSD144P]: 1,
	[VideoQuality.SubSDCIF]: 2,
	[VideoQuality.NHD]: 3,
	[VideoQuality.SD]: 4,
	[VideoQuality.DVD]: 5,
	[VideoQuality.HD]: 6,
	[VideoQuality.FullHD]: 7,
	[VideoQuality.QHD]: 8,
	[VideoQuality.UHD_4K]: 9,
	[VideoQuality.UHD_8K]: 10,
};

/** Returns the highest VideoQuality from a media item's qualities array. */
export function getHighestQuality(item: PlexMediaSlimDTO): VideoQuality {
	if (!item.qualities?.length) {
		return VideoQuality.Unknown;
	}
	let best: VideoQuality = VideoQuality.Unknown;
	for (const q of item.qualities) {
		if (videoQualityRank[q.quality] > videoQualityRank[best]) {
			best = q.quality;
		}
	}
	return best;
}

/** Returns the numeric rank of the highest quality in a media item. */
export function getHighestQualityRank(item: PlexMediaSlimDTO): number {
	return videoQualityRank[getHighestQuality(item)];
}

const videoQualityByValue = new Map<number, VideoQuality>([
	[-1, VideoQuality.None],
	[0, VideoQuality.Unknown],
	[144, VideoQuality.SubSD144P],
	[240, VideoQuality.SubSDCIF],
	[360, VideoQuality.NHD],
	[480, VideoQuality.SD],
	[576, VideoQuality.DVD],
	[720, VideoQuality.HD],
	[1080, VideoQuality.FullHD],
	[1440, VideoQuality.QHD],
	[2160, VideoQuality.UHD_4K],
	[4320, VideoQuality.UHD_8K],
]);

export function getVideoQualityFromValue(value: string): VideoQuality | undefined {
	return videoQualityByValue.get(Number(value));
}

const qualityColorMap: Record<VideoQuality, string> = {
	[VideoQuality.None]: 'black',
	[VideoQuality.SubSD144P]: 'brown-6',
	[VideoQuality.SubSDCIF]: 'deep-orange-6',
	[VideoQuality.NHD]: 'orange-7',
	[VideoQuality.SD]: 'amber-7',
	[VideoQuality.DVD]: 'yellow-7',
	[VideoQuality.HD]: 'light-green-13',
	[VideoQuality.FullHD]: 'light-blue-6',
	[VideoQuality.QHD]: 'cyan-6',
	[VideoQuality.UHD_4K]: 'red darken-4',
	[VideoQuality.UHD_8K]: 'purple-8',
	[VideoQuality.Unknown]: 'blue-grey-4',
};

export function getVideoQualityColor(quality?: VideoQuality): string {
	if (quality === undefined) {
		return qualityColorMap[VideoQuality.None];
	}
	return qualityColorMap[quality];
}

export function translateVideoQuality(quality?: VideoQuality): string {
	const { $i18n } = useNuxtApp();
	const { t } = $i18n;

	switch (quality) {
		case VideoQuality.None:
			return t('general.video-quality.quality-0-none');
		case VideoQuality.Unknown:
			return t('general.video-quality.quality-1-unknown');
		case VideoQuality.SubSD144P:
			return t('general.video-quality.quality-144-subsd-144p');
		case VideoQuality.SubSDCIF:
			return t('general.video-quality.quality-240-subsd-cif');
		case VideoQuality.NHD:
			return t('general.video-quality.quality-360-nhd');
		case VideoQuality.SD:
			return t('general.video-quality.quality-480-sd');
		case VideoQuality.DVD:
			return t('general.video-quality.quality-576-dvd');
		case VideoQuality.HD:
			return t('general.video-quality.quality-720-hd');
		case VideoQuality.FullHD:
			return t('general.video-quality.quality-1080-fullhd');
		case VideoQuality.QHD:
			return t('general.video-quality.quality-1440-qhd');
		case VideoQuality.UHD_4K:
			return t('general.video-quality.quality-2160-uhd-4k');
		case VideoQuality.UHD_8K:
			return t('general.video-quality.quality-4320-uhd-8k');
		default:
			return t('general.error.unknown');
	}
}
