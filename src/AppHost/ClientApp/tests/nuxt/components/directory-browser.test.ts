import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { defineComponent, h } from 'vue';
import { flushPromises } from '@vue/test-utils';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { generateResultDTO } from '@mock';
import { FolderPathPaths } from '@api/api-paths';
import { useDialogStore } from '@store';
import DirectoryBrowser from '@/components/Dialogs/DirectoryBrowser.vue';
import { FileSystemEntityType, FolderType, PlexMediaType, type FileSystemDTO, type FileSystemModelDTO, type FolderPathDTO } from '@dto';
import { DialogType } from '@enums';
import messages from '@/lang/en-US.json';

const CardDialog = defineComponent({
	name: 'QCardDialog',
	emits: ['opened', 'closed'],
	setup(_, { slots, emit }) {
		return () => h('section', [
			slots.title?.(),
			slots['top-row']?.(),
			slots.default?.(),
			slots.actions?.({ close: () => emit('closed') }),
		]);
	},
});

const PTextField = defineComponent({
	name: 'PTextField',
	props: { modelValue: { type: String, default: '' } },
	emits: ['update:modelValue'],
	setup(props) {
		return () => h('input', { value: props.modelValue });
	},
});

const ActionButton = (name: string) => defineComponent({
	name,
	props: {
		disabled: { type: Boolean, default: false },
		loading: { type: Boolean, default: false },
	},
	emits: ['click'],
	setup(props, { emit, attrs }) {
		return () => h('button', {
			...attrs,
			'data-action': name,
			'data-cy': attrs.cy,
			disabled: props.disabled,
			'data-loading': props.loading ? 'true' : undefined,
			onClick: () => emit('click'),
		});
	},
});

const Transparent = (name: string, tag = 'div') => defineComponent({
	name,
	setup(_, { slots }) {
		return () => h(tag, slots.default?.());
	},
});

const Alert = defineComponent({
	name: 'QAlert',
	setup(_, { attrs, slots }) {
		return () => h('div', { ...attrs, 'data-cy': attrs.cy }, slots.default?.());
	},
});

const folderPath: FolderPathDTO = {
	id: 7,
	displayName: 'Movies',
	directory: '/root',
	mediaType: PlexMediaType.Movie,
	folderType: FolderType.MovieFolder,
	isValid: true,
	isDefault: true,
};

const writableDirectory: FileSystemModelDTO = {
	name: 'Movies',
	path: '/root/Movies',
	type: FileSystemEntityType.Folder,
	extension: '',
	size: 0,
	lastModified: '',
	hasReadPermission: true,
	hasWritePermission: true,
};

const mounted: Array<{ unmount: () => void }> = [];

afterEach(() => {
	mounted.forEach((wrapper) => wrapper.unmount());
	mounted.length = 0;
	vi.restoreAllMocks();
});

describe('DirectoryBrowser', () => {
	let { mock } = baseVars();
	beforeAll(() => baseSetup());
	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	async function render(props: { saving: boolean; errorMessage: string }) {
		const directoryResult: FileSystemDTO = {
			current: { ...writableDirectory, path: folderPath.directory },
			directories: [writableDirectory],
			files: [],
			parent: '/',
		};
		mock.onGet(FolderPathPaths.getFolderPathDirectoryEndpoint()).reply(200, generateResultDTO(directoryResult));
		const wrapper = await mountSuspended(DirectoryBrowser, {
			props,
			global: {
				plugins: [createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: {
					QCardDialog: CardDialog,
					PTextField,
					ConfirmButton: ActionButton('ConfirmButton'),
					CancelButton: ActionButton('CancelButton'),
					QAlert: Alert,
					QRow: Transparent('QRow'),
					QCol: Transparent('QCol'),
					'q-markup-table': Transparent('QMarkupTable', 'table'),
					'q-tooltip': true,
				},
			},
		});
		mounted.push(wrapper);
		return wrapper;
	}

	test('Should emit selected paths without closing and report both lifecycle directions', async () => {
		// Arrange
		const wrapper = await render({ saving: false, errorMessage: '' });
		const dialogStore = useDialogStore();
		const closeDialog = vi.spyOn(dialogStore, 'closeDialog');
		const cardDialog = wrapper.getComponent(CardDialog);

		// Act
		cardDialog.vm.$emit('opened', folderPath);
		await flushPromises();
		await wrapper.get('[data-cy="directory-browser-confirm-button"]').trigger('click');

		// Assert
		expect(wrapper.emitted('opened')?.[0]?.[0]).toEqual(folderPath);
		expect(wrapper.emitted('confirm')?.[0]?.[0]).toEqual(folderPath);
		expect(wrapper.emitted('closed')).toBeUndefined();
		expect(closeDialog).not.toHaveBeenCalledWith(DialogType.DirectoryBrowserDialog);

		// Act implicit close, then explicit cancel
		cardDialog.vm.$emit('closed');
		await wrapper.get('[data-action="CancelButton"]').trigger('click');

		// Assert
		expect(wrapper.emitted('closed')).toHaveLength(1);
		expect(wrapper.emitted('cancel')).toHaveLength(1);
		expect(closeDialog).toHaveBeenCalledWith(DialogType.DirectoryBrowserDialog);
	});

	test('Should show save errors and block navigation and confirmation while saving', async () => {
		// Arrange
		const wrapper = await render({ saving: false, errorMessage: '' });
		const cardDialog = wrapper.getComponent(CardDialog);
		cardDialog.vm.$emit('opened', folderPath);
		await flushPromises();
		await wrapper.setProps({ saving: true, errorMessage: 'The directory could not be saved.' });
		const initialRequests = mock.history.get.length;

		// Act
		await wrapper.get('[data-cy="directory-browser-row-0"]').trigger('click');
		await wrapper.get('[data-cy="directory-browser-confirm-button"]').trigger('click');
		await flushPromises();

		// Assert
		expect(wrapper.get('[data-cy="directory-browser-save-error"]').text()).toContain('The directory could not be saved.');
		expect(wrapper.get('[data-cy="directory-browser-confirm-button"]').attributes('disabled')).toBeDefined();
		expect(wrapper.getComponent(PTextField).attributes('readonly')).toBeDefined();
		expect(mock.history.get.length).toBe(initialRequests);
		expect(wrapper.emitted('confirm')).toBeUndefined();
	});
});
