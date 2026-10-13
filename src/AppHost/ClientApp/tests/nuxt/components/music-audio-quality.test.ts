import { afterEach, beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { defineComponent } from 'vue';
import { createI18n } from 'vue-i18n';
import messages from '@/lang/en-US.json';
import { baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { AudioQuality, PlexMediaType, VideoQuality, type PlexMediaQualityDTO } from '@dto';
import MediaAudioQuality from '~/components/MediaOverview/MediaAudioQuality.vue';
import MediaQuality from '~/components/MediaOverview/MediaQuality.vue';

const chip = defineComponent({
	props: ['value'],
	emits: ['click'],
	template: '<button @click="$emit(\'click\')">{{ value }}</button>',
});

describe('Music audio quality and explicit version selection', () => {
	let { mock } = baseVars();

	beforeAll(() => baseSetup());
	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	afterEach(() => mock.restore());
	test.each<[AudioQuality, string]>([
		[AudioQuality.FLAC16Bit441KHz, 'FLAC 16-bit 44.1 kHz'],
		[AudioQuality.FLAC24Bit96KHz, 'FLAC 24-bit 96 kHz'],
		[AudioQuality.MP3320KbpsPlus, 'MP3 320 kbps+'],
		[AudioQuality.AACBelow96Kbps, 'AAC <96 kbps'],
		[AudioQuality.Unknown, '"Unknown"'],
	])('Should show enum-derived audio labels for %s', async (quality, label) => {
		// Arrange / Act
		const wrapper = await mountSuspended(MediaAudioQuality, {
			props: { quality },
			global: {
				plugins: [createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QGlowChip: chip },
			},
		});
		// Assert
		expect(wrapper.text()).toBe(label);
		wrapper.unmount();
	});

	test('Should preserve the selected DataId when two originals share an audio classification', async () => {
		// Arrange
		const first: PlexMediaQualityDTO = {
			mediaId: 41, dataId: 401, mediaDataType: PlexMediaType.MusicTrack,
			quality: VideoQuality.None, audioQuality: AudioQuality.FLAC16Bit441KHz,
		};
		const second: PlexMediaQualityDTO = { ...first, dataId: 402 };
		const wrapper = await mountSuspended(MediaQuality, {
			props: { qualities: [first, second], clickable: true },
			global: {
				plugins: [createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QGlowChip: chip },
			},
		});
		// Act
		await wrapper.findAll('button')[1]!.trigger('click');
		// Assert
		expect(wrapper.emitted('download')).toEqual([[[second]]]);
		expect(wrapper.findAll('button').map((button) => button.text())).toEqual([
			'FLAC 16-bit 44.1 kHz', 'FLAC 16-bit 44.1 kHz',
		]);
		wrapper.unmount();
	});

	test.each([PlexMediaType.MusicArtist, PlexMediaType.MusicAlbum])('Should render distinct audio chips for parent type %s without ranking enum values', async (mediaDataType) => {
		// Arrange
		const repeated: PlexMediaQualityDTO = {
			mediaId: 41, dataId: -1, mediaDataType,
			quality: VideoQuality.None, audioQuality: AudioQuality.FLAC16Bit441KHz,
		};
		const alternate: PlexMediaQualityDTO = {
			...repeated,
			audioQuality: AudioQuality.MP3320KbpsPlus,
		};

		// Act
		const wrapper = await mountSuspended(MediaQuality, {
			props: { qualities: [repeated, { ...repeated }, alternate] },
			global: {
				plugins: [createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QGlowChip: chip },
			},
		});

		// Assert
		expect(wrapper.findAll('button').map((button) => button.text())).toEqual([
			'FLAC 16-bit 44.1 kHz', 'MP3 320 kbps+',
		]);
		wrapper.unmount();
	});

	test('Should keep video quality rows on the existing video chip presentation', async () => {
		// Arrange
		const video: PlexMediaQualityDTO = {
			mediaId: 41,
			dataId: 401,
			mediaDataType: PlexMediaType.Movie,
			quality: VideoQuality.FullHD,
			audioQuality: AudioQuality.None,
		};

		// Act
		const wrapper = await mountSuspended(MediaQuality, {
			props: { qualities: [video] },
			global: {
				plugins: [createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QGlowChip: chip },
			},
		});

		// Assert
		expect(wrapper.text()).toContain('Full HD (1080p)');
		expect(wrapper.text()).not.toContain('Unknown');
		wrapper.unmount();
	});
});
