import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { defineComponent, h } from 'vue';
import { createI18n } from 'vue-i18n';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { baseSetup } from '@services-test-base';
import { PhotoAssetKind, PhotoItemKind, ViewMode, type PhotoAssetBrowseDTO, type PhotoFolderBrowseDTO } from '@dto';
import { useSettingsStore } from '@store';
import type { PhotoOverviewItem } from '@/store/photoBrowsingStore';
import PhotoOverviewList from '@/components/Photos/PhotoOverviewList.vue';
import PhotoOverviewTableRow from '@/components/Photos/PhotoOverviewTableRow.vue';
import messages from '@/lang/en-US.json';

const asset = (id: number, title = `Asset ${id}`): PhotoAssetBrowseDTO => ({
	id,
	plexLibraryId: 4,
	plexServerId: 3,
	title,
	plexServerName: 'Camera server',
	plexLibraryTitle: 'Travel photos',
	assetKind: PhotoAssetKind.Image,
	hasThumbnail: false,
	plexApiRatingKey: id,
	plexApiMetaDataKey: id,
	originalFilename: `${title}.jpg`,
	container: 'jpg',
	size: 2048,
	width: 1920,
	height: 1080,
	duration: null,
	relativeFolderPath: '',
	sourceRootTitle: 'Camera',
});

const folder: PhotoFolderBrowseDTO = {
	id: 8,
	plexLibraryId: 4,
	plexServerId: 3,
	title: 'Summer',
	relativePath: 'Summer',
	parentId: null,
	isRoot: false,
	isActive: null,
	recursiveAssetCount: 2,
	recursiveAssetBytes: 4096,
};

const items: PhotoOverviewItem[] = [
	{ kind: PhotoItemKind.Folder, folder, index: 0 },
	{ kind: PhotoItemKind.Asset, asset: asset(10), index: 1 },
	{ kind: PhotoItemKind.Asset, asset: asset(11), index: 2 },
];

const i18n = () => createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } });

describe('PhotoOverviewList', () => {
	let pinia = createPinia();
	beforeAll(() => baseSetup());
	beforeEach(() => {
		pinia = createPinia();
		setActivePinia(pinia);
	});

	test('Should expose one logical mixed feed and keep poster cards free of selection controls', async () => {
		// Arrange
		const wrapper = await mountSuspended(PhotoOverviewList, {
			props: {
				context: 'folders',
				itemCount: items.length,
				itemAt: (index) => items[index] ?? null,
			},
			global: {
				plugins: [pinia, i18n()],
				stubs: {
					PosterTable: {
						props: ['itemCount'],
						template: '<div data-cy="photo-overview-scroll"><slot name="item" :index="0" /></div>',
					},
					MediaPosterCard: {
						inheritAttrs: false,
						template: '<div v-bind="$attrs"><slot name="metadata" :compact="false" /><slot name="actions" :compact="false" :activate="() => undefined" /></div>',
					},
					PhotoThumbnail: true,
					QFileSize: true,
					BaseButton: true,
				},
			},
		});

		// Assert
		expect(wrapper.get('[data-cy="photo-overview-list"]')).toBeTruthy();
		expect(wrapper.get('[data-cy="photo-folder-8"]')).toBeTruthy();
		expect(wrapper.find('[role="checkbox"]').exists()).toBe(false);
	});

	test('Should render the table item identity and emit open/download actions for a direct asset', async () => {
		// Arrange
		const wrapper = await mountSuspended(PhotoOverviewTableRow, {
			props: { item: items[1]! },
			global: { plugins: [pinia, i18n()], stubs: { QFileSize: true } },
		});

		// Act
		await wrapper.get('td:first-child button').trigger('click');
		await wrapper.get('td:last-child button').trigger('click');

		// Assert
		expect(wrapper.get('[data-cy="photo-asset-4-10"]')).toBeTruthy();
		expect(wrapper.emitted('open')).toHaveLength(1);
		expect(wrapper.emitted('download')).toHaveLength(1);
	});

	test('Should invoke the explicit table select-all action for the complete logical feed', async () => {
		// Arrange
		useSettingsStore().displaySettings.photoAlbumViewMode = ViewMode.Table;
		const tableStub = defineComponent({
			props: ['itemCount', 'itemAt'],
			emits: ['select-all'],
			setup(props, { emit }) {
				return () => h('div', { 'data-cy': 'photo-overview-table' }, [
					h('button', { 'data-cy': 'photo-select-all', onClick: () => emit('select-all', true) }, 'Select all'),
					h('span', { 'data-cy': `photo-asset-${props.itemAt(1).asset.plexLibraryId}-${props.itemAt(1).asset.id}` }),
				]);
			},
		});
		const wrapper = await mountSuspended(PhotoOverviewList, {
			props: {
				context: 'assets',
				itemCount: items.length,
				itemAt: (index) => items[index] ?? null,
			},
			global: {
				plugins: [pinia, i18n()],
				stubs: { MediaOverviewTable: tableStub },
			},
		});

		// Act
		await wrapper.get('[data-cy="photo-select-all"]').trigger('click');

		// Assert
		expect(wrapper.emitted('select-all')).toEqual([[true]]);
		expect(wrapper.get('[data-cy="photo-asset-4-10"]')).toBeTruthy();
	});
});
