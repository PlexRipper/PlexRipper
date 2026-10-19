import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import type { Pinia } from 'pinia';
import { defineComponent, h } from 'vue';
import { flushPromises } from '@vue/test-utils';
import { mockNuxtImport, mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import type * as VueI18n from 'vue-i18n';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { FolderPathPaths } from '@api/api-paths';
import { FolderType, PlexMediaType, type FolderPathDTO } from '@dto';
import type { ResultDTO } from '@interfaces';
import { DialogType } from '@enums';
import { useDialogStore, useFolderPathStore } from '@store';
import FolderPathsOverview from '@/components/Overviews/FolderPathsOverview.vue';
import messages from '@/lang/en-US.json';

const notify = vi.hoisted(() => vi.fn());
mockNuxtImport('showErrorNotification', () => notify);

// mountSuspended recomputes store grouping outside setup; translation is not under test.
vi.mock('vue-i18n', async (importOriginal) => {
	const actual = await importOriginal<typeof VueI18n>();
	return {
		...actual,
		useI18n: () => ({ t: (key: string) => key }),
	};
});

const DirectoryBrowser = defineComponent({
	name: 'DirectoryBrowser',
	props: {
		saving: { type: Boolean, required: true },
		errorMessage: { type: String, required: true },
	},
	emits: ['confirm', 'opened', 'closed'],
	setup(props) {
		return () => h('div', {
			'data-cy': 'directory-browser',
			'data-saving': props.saving ? 'true' : 'false',
		}, props.errorMessage);
	},
});

const ConfirmationDialog = defineComponent({
	name: 'ConfirmationDialog',
	props: {
		warning: { type: String, default: '' },
		confirmLoading: { type: Boolean, default: false },
	},
	emits: ['confirm', 'cancel'],
	setup(props, { emit }) {
		return () => h('div', { 'data-cy': 'confirmation-dialog' }, [
			h('span', { 'data-cy': 'confirmation-warning' }, props.warning),
			h('button', { 'data-cy': 'confirmation-confirm', onClick: () => emit('confirm') }),
		]);
	},
});

const HelpRow = defineComponent({
	name: 'HelpRow',
	props: {
		editModel: { type: String, default: '' },
	},
	emits: ['update:editModel'],
	setup(props, { slots }) {
		return () => h('div', { 'data-edit-model': props.editModel }, [
			h('span', props.editModel),
			slots.default?.(),
		]);
	},
});

function passthrough(name: string, tag = 'div') {
	return defineComponent({
		name,
		inheritAttrs: false,
		setup(_, { attrs, slots }) {
			return () => h(tag, attrs, slots.default?.());
		},
	});
}

const ActionIcon = (name: string) => defineComponent({
	name,
	props: { cy: { type: String, default: '' } },
	emits: ['click'],
	setup(props, { emit }) {
		return () => h('button', { 'data-cy': props.cy, onClick: () => emit('click') });
	},
});

const defaultDownloadPath: FolderPathDTO = {
	id: 1,
	displayName: 'Download folder',
	directory: '/downloads',
	mediaType: PlexMediaType.None,
	folderType: FolderType.DownloadFolder,
	isValid: true,
	isDefault: true,
};
const AddIconButton = ActionIcon('AddIconButton');
const DeleteIconButton = ActionIcon('DeleteIconButton');

const customDownloadPath: FolderPathDTO = {
	id: 42,
	displayName: 'Custom download',
	directory: '/custom-downloads',
	mediaType: PlexMediaType.None,
	folderType: FolderType.DownloadFolder,
	isValid: true,
	isDefault: false,
};
function failedResult(statusCode: number, message: string): ResultDTO<FolderPathDTO> {
	return generateFailedResultDTO({
		statusCode,
		errors: [{ message, reasons: [], metadata: {} }],
	});
}

const mounted: Array<{ unmount: () => void }> = [];

afterEach(() => {
	mounted.forEach((wrapper) => wrapper.unmount());
	mounted.length = 0;
	vi.restoreAllMocks();
});

describe('FolderPathsOverview persistence ownership', () => {
	let { mock } = baseVars();
	let pinia: Pinia;
	beforeAll(() => baseSetup());
	beforeEach(() => {
		mock = getAxiosMock();
		pinia = createPinia();
		setActivePinia(pinia);
	});

	async function render(paths: FolderPathDTO[]) {
		mock.onGet(FolderPathPaths.getAllFolderPathsEndpoint()).reply(200, generateResultDTO(paths));
		await subscribeSpyTo(useFolderPathStore().setup()).onComplete();
		const wrapper = await mountSuspended(FolderPathsOverview, {
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: {
					DirectoryBrowser,
					ConfirmationDialog,
					HelpRow,
					QSection: passthrough('QSection', 'section'),
					QRow: passthrough('QRow'),
					QCol: passthrough('QCol'),
					'q-tabs': passthrough('QTabs'),
					'q-tab': passthrough('QTab'),
					'q-icon': passthrough('QIcon'),
					QMediaTypeIcon: passthrough('QMediaTypeIcon'),
					ValidIcon: passthrough('ValidIcon'),
					DeleteIconButton,
					AddIconButton,
				},
			},
		});
		mounted.push(wrapper);
		return wrapper;
	}

	test.each(['failure', 'success'])('Should keep a newer directory save owned when an older %s completes', async (olderKind) => {
		// Arrange
		const olderResult = olderKind === 'failure'
			? failedResult(409, 'The older directory write failed.')
			: generateResultDTO({ ...customDownloadPath, directory: '/older' });
		const oldReply = Promise.withResolvers<[number, ResultDTO<FolderPathDTO>]>();
		const currentReply = Promise.withResolvers<[number, ResultDTO<FolderPathDTO>]>();
		const updatedPath = { ...customDownloadPath, directory: '/current' };
		mock.onPut(FolderPathPaths.updateFolderPathEndpoint())
			.replyOnce(() => oldReply.promise)
			.onPut(FolderPathPaths.updateFolderPathEndpoint())
			.reply(() => currentReply.promise);
		const wrapper = await render([defaultDownloadPath, customDownloadPath]);
		const browser = wrapper.getComponent(DirectoryBrowser);
		const dialogStore = useDialogStore();
		const closeDialog = vi.spyOn(dialogStore, 'closeDialog');

		// Act first attempt, duplicate attempt, close/reopen, and current attempt
		browser.vm.$emit('opened', customDownloadPath);
		browser.vm.$emit('confirm', { ...customDownloadPath, directory: '/older' });
		browser.vm.$emit('confirm', { ...customDownloadPath, directory: '/duplicate' });
		await flushPromises();
		expect(mock.history.put).toHaveLength(1);
		browser.vm.$emit('closed');
		browser.vm.$emit('opened', customDownloadPath);
		browser.vm.$emit('confirm', updatedPath);
		await flushPromises();
		expect(mock.history.put).toHaveLength(2);
		oldReply.resolve([200, olderResult]);
		await flushPromises();
		expect(browser.props('errorMessage')).toBe('');
		expect(browser.props('saving')).toBe(true);
		expect(closeDialog).not.toHaveBeenCalledWith(DialogType.DirectoryBrowserDialog);
		currentReply.resolve([200, generateResultDTO(updatedPath)]);
		await flushPromises();

		// Assert
		expect(useFolderPathStore().getFolderPath(customDownloadPath.id)?.directory).toBe('/current');
		expect(browser.props('errorMessage')).toBe('');
		expect(browser.props('saving')).toBe(false);
		expect(closeDialog).toHaveBeenCalledWith(DialogType.DirectoryBrowserDialog);
	});
	test('Should retain failed create and name drafts and apply both successful retries', async () => {
		// Arrange
		const createdPath: FolderPathDTO = {
			...customDownloadPath,
			id: 99,
			displayName: 'Download Folder Path',
			directory: '/new-downloads',
		};
		const createFailure = failedResult(409, 'The folder path already exists.');
		const createSuccess = generateResultDTO(createdPath);
		mock.onPost(FolderPathPaths.createFolderPathEndpoint())
			.replyOnce(200, createFailure)
			.onPost(FolderPathPaths.createFolderPathEndpoint())
			.reply(200, createSuccess);
		const wrapper = await render([defaultDownloadPath]);
		const browser = wrapper.getComponent(DirectoryBrowser);
		const addButton = wrapper.getComponent(AddIconButton);
		const draft: FolderPathDTO = {
			...createdPath,
			id: 0,
			isValid: false,
		};

		// Act failed create and retry
		addButton.vm.$emit('click');
		browser.vm.$emit('opened', draft);
		browser.vm.$emit('confirm', draft);
		await flushPromises();

		// Assert failed create
		expect(browser.props('errorMessage')).toContain('The folder path already exists.');
		expect(useFolderPathStore().getFolderPath(createdPath.id)).toBeUndefined();
		browser.vm.$emit('confirm', draft);
		await flushPromises();
		expect(useFolderPathStore().getFolderPath(createdPath.id)?.directory).toBe('/new-downloads');

		// Arrange display-name failure and retry
		const nameFailure = failedResult(409, 'The display name is already in use.');
		const renamedPath = { ...createdPath, displayName: 'Renamed downloads' };
		const nameSuccess = generateResultDTO(renamedPath);
		mock.onPut(FolderPathPaths.updateFolderPathEndpoint())
			.replyOnce(200, nameFailure)
			.onPut(FolderPathPaths.updateFolderPathEndpoint())
			.reply(200, nameSuccess);
		const nameRow = wrapper.findAllComponents(HelpRow).find((row) => row.props('editModel') === createdPath.displayName);
		expect(nameRow).toBeDefined();

		// Act and assert name failure/retry
		nameRow!.vm.$emit('update:editModel', renamedPath.displayName);
		await flushPromises();
		expect(useFolderPathStore().getFolderPath(createdPath.id)?.displayName).toBe(createdPath.displayName);
		expect(nameRow!.props('editModel')).toBe(createdPath.displayName);
		expect(notify).toHaveBeenCalledWith(expect.stringContaining('The display name is already in use.'));
		nameRow!.vm.$emit('update:editModel', renamedPath.displayName);
		await flushPromises();
		expect(useFolderPathStore().getFolderPath(createdPath.id)?.displayName).toBe(renamedPath.displayName);
	});

	test('Should retain failed delete confirmation and selected row until a successful retry', async () => {
		// Arrange
		const deleteFailure = failedResult(503, 'The folder path could not be deleted.');
		const deleteSuccess: ResultDTO = {
			isSuccess: true,
			statusCode: 200,
			errors: [],
			successes: [],
		};
		mock.onDelete(FolderPathPaths.deleteFolderPathEndpoint(customDownloadPath.id))
			.replyOnce(200, deleteFailure)
			.onDelete(FolderPathPaths.deleteFolderPathEndpoint(customDownloadPath.id))
			.reply(200, deleteSuccess);
		const wrapper = await render([defaultDownloadPath, customDownloadPath]);
		const deleteButton = wrapper.getComponent(DeleteIconButton);
		const confirmation = wrapper.getComponent(ConfirmationDialog);
		const dialogStore = useDialogStore();
		const closeDialog = vi.spyOn(dialogStore, 'closeDialog');

		// Act failed delete
		deleteButton.vm.$emit('click');
		confirmation.vm.$emit('confirm');
		confirmation.vm.$emit('confirm');
		await flushPromises();

		// Assert failed confirmation and retained row
		expect(confirmation.props('warning')).toContain('The folder path could not be deleted.');
		expect(confirmation.props('confirmLoading')).toBe(false);
		expect(useFolderPathStore().getFolderPath(customDownloadPath.id)).toEqual(customDownloadPath);
		confirmation.vm.$emit('confirm');
		await flushPromises();
		expect(useFolderPathStore().getFolderPath(customDownloadPath.id)).toBeUndefined();
		expect(closeDialog).toHaveBeenCalledWith(DialogType.FolderPathDeleteConfirmationDialog);
	});
});
