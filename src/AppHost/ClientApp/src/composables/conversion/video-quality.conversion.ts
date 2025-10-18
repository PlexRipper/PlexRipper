import { VideoQuality } from '@dto';

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
