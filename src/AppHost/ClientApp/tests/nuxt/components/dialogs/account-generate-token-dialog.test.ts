import { mountSuspended } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { describe, beforeAll, beforeEach, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { defineComponent, h, nextTick } from 'vue';
import { baseSetup, getAxiosMock } from '@services-test-base';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { useAccountDialogStore } from '@store';
import type { GeneratePlexTokenResponse } from '@dto';
import AccountGenerateTokenDialog from '@components/Dialogs/AccountDialog/AccountGenerateTokenDialog.vue';

vi.mock('#imports', async () => {
	const actual = await vi.importActual('#imports');
	const stores = await vi.importActual('@store');

	return {
		...actual,
		useAccountDialogStore: stores.useAccountDialogStore,
	};
});

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const QCardDialogStub = defineComponent({
	name: 'QCardDialog',
	emits: ['opened'],
	setup(_, { slots }) {
		return () => h('div', [
			slots.title?.(),
			slots['top-row']?.(),
			slots.default?.(),
			slots.actions?.({ close: vi.fn() }),
		]);
	},
});

const QInputStub = defineComponent({
	name: 'q-input',
	props: {
		modelValue: {
			type: String,
			default: '',
		},
	},
	setup(props) {
		return () => h('div', { 'data-cy': 'token-input' }, props.modelValue);
	},
});

function generateTokenResponse(partial?: Partial<GeneratePlexTokenResponse>): GeneratePlexTokenResponse {
	return {
		isUnAuthorized: false,
		needsVerificationCode: false,
		plexAuthToken: '',
		...partial,
	};
}

describe('AccountGenerateTokenDialog', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
		getAxiosMock();
		const store = useAccountDialogStore();
		store.id = 0;
		store.isValidated = false;
		store.authenticationToken = 'auth-token';
	});

	async function mountDialog() {
		return mountSuspended(AccountGenerateTokenDialog, {
			global: {
				mocks: {
					$t: (key: string) => key,
				},
				stubs: {
					QCardDialog: QCardDialogStub,
					QSubHeader: SlotStub,
					QRow: SlotStub,
					QCol: SlotStub,
					HideButton: SlotStub,
					IconButton: SlotStub,
					'q-input': QInputStub,
				},
			},
		});
	}

	test('Should clear a stale generated token when reopening the dialog', async () => {
		// Arrange
		const mock = getAxiosMock();
		mock.onGet('/api/PlexAccount/generate-token/0')
			.replyOnce(200, generateResultDTO(generateTokenResponse({ plexAuthToken: 'old-token' })))
			.onGet('/api/PlexAccount/generate-token/0')
			.reply(() => new Promise(() => {}));
		const wrapper = await mountDialog();
		const dialog = wrapper.findComponent(QCardDialogStub);

		// Act
		dialog.vm.$emit('opened');
		await flushPromises();
		expect(wrapper.text()).toContain('old-token');

		dialog.vm.$emit('opened');
		await nextTick();

		// Assert
		expect(wrapper.text()).not.toContain('old-token');
	});

	test('Should hide stale verification input when reopening the dialog', async () => {
		// Arrange
		const mock = getAxiosMock();
		mock.onGet('/api/PlexAccount/generate-token/0')
			.replyOnce(200, generateResultDTO(generateTokenResponse({ needsVerificationCode: true })))
			.onGet('/api/PlexAccount/generate-token/0')
			.reply(() => new Promise(() => {}));
		const wrapper = await mountDialog();
		const dialog = wrapper.findComponent(QCardDialogStub);

		// Act
		dialog.vm.$emit('opened');
		await flushPromises();
		expect(wrapper.find('[data-cy="2fa-code-verification-input"]').exists()).toBe(true);

		dialog.vm.$emit('opened');
		await nextTick();

		// Assert
		expect(wrapper.find('[data-cy="2fa-code-verification-input"]').exists()).toBe(false);
	});

	test('Should keep the verification input visible when token verification fails', async () => {
		// Arrange
		const mock = getAxiosMock();
		mock.onGet('/api/PlexAccount/generate-token/0')
			.replyOnce(200, generateResultDTO(generateTokenResponse({ needsVerificationCode: true })));
		mock.onGet('/api/PlexAccount/generate-token/0')
			.reply(401, generateFailedResultDTO());
		const wrapper = await mountDialog();
		const dialog = wrapper.findComponent(QCardDialogStub);

		dialog.vm.$emit('opened');
		await flushPromises();

		// Act
		const inputs = wrapper.findAll('[data-test="single-input"]');
		for (const [index, input] of inputs.entries()) {
			await input.setValue(String(index + 1));
		}
		await flushPromises();

		// Assert
		expect(wrapper.find('[data-cy="2fa-code-verification-input"]').exists()).toBe(true);
	});
});
