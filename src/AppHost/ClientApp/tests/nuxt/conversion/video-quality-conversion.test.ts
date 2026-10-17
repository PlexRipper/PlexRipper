import { describe, expect, test } from 'vitest';
import { AudioQuality, PlexMediaType, VideoQuality, type PlexMediaQualityDTO, type PlexMediaSlimDTO } from '@dto';
import { getHighestQuality, getHighestQualityRank } from '@composables/conversion/video-quality.conversion';
import { generatePlexMedia } from '@mock';

function media(qualities: PlexMediaQualityDTO[], type = PlexMediaType.Movie): PlexMediaSlimDTO {
	return generatePlexMedia({
		config: { seed: 593 },
		partialData: { plexServerId: 1, plexLibraryId: 1, type, qualities },
	});
}

function quality(videoQuality: VideoQuality, dataId: number, mediaDataType = PlexMediaType.Movie): PlexMediaQualityDTO {
	return {
		audioQuality: AudioQuality.None,
		dataId,
		mediaDataType,
		mediaId: 1,
		videoQuality,
	};
}

describe('video-quality.conversion', () => {
	test('Should rank the highest quality from canonical videoQuality fields', () => {
		// Arrange
		const item = media([
			quality(VideoQuality.HD, 1),
			quality(VideoQuality.UHD_4K, 2),
			quality(VideoQuality.FullHD, 3),
		]);

		// Act
		const highest = getHighestQuality(item);

		// Assert
		expect(highest).toBe(VideoQuality.UHD_4K);
		expect(getHighestQualityRank(item)).toBe(9);
	});

	test('Should return Unknown when a media item has no quality sources', () => {
		// Arrange
		const item = media([]);

		// Act
		const highest = getHighestQuality(item);

		// Assert
		expect(highest).toBe(VideoQuality.Unknown);
		expect(getHighestQualityRank(item)).toBe(0);
	});

	test('Should keep Unknown for music-only quality sources without video resolution', () => {
		// Arrange
		const item = media(
			[quality(VideoQuality.None, 1, PlexMediaType.MusicTrack)],
			PlexMediaType.MusicTrack,
		);

		// Act
		const highest = getHighestQuality(item);

		// Assert
		expect(highest).toBe(VideoQuality.Unknown);
		expect(getHighestQualityRank(item)).toBe(0);
	});
});
