import { mountSuspended } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { describe, beforeAll, beforeEach, expect, test, vi } from 'vitest';
import { defineComponent, h } from 'vue';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, getAxiosMock } from '@services-test-base';
import { useAccountDialogStore } from '@store';
import { generateResultDTO } from '@mock';
import type { ResultDTOOfValidatePlexCredentialsDTO, ValidatePlexCredentialsDTO } from '@dto';
import { PlexAccountPaths } from '@api-urls';
import AccountVerificationCodeDialog from '../../../../src/components/Dialogs/AccountDialog/AccountVerificationCodeDialog.vue';

vi.mock('#imports', async () => {
	const actual = await vi.importActual('#imports');
	const stores = await vi.importActual('@store');

	return {
		...actual,
		useAccountDialogStore: stores.useAccountDialogStore,
		useI18n: () => ({
			t: (key: string) => key,
		}),
	};
});

vi.mock('vue-i18n', () => ({
	useI18n: () => ({
		t: (key: string) => key,
	}),
}));

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const ConfirmButtonStub = defineComponent({
	name: 'ConfirmButton',
	props: {
		loading: {
			type: Boolean,
			default: false,
		},
		disabled: {
			type: Boolean,
			default: false,
		},
	},
	emits: ['click'],
	setup(props, { emit }) {
		return () => h('button', { disabled: props.disabled, onClick: () => emit('click') }, 'confirm');
	},
});

const CancelButtonStub = defineComponent({
	name: 'CancelButton',
	emits: ['click'],
	setup(_, { emit }) {
		return () => h('button', { onClick: () => emit('click') }, 'cancel');
	},
});

const VOtpInputStub = defineComponent({
	name: 'VOtpInput',
	props: {
		value: {
			type: String,
			default: '',
		},
	},
	emits: ['update:value', 'on-complete'],
	setup() {
		return () => h('input');
	},
});

function failedValidationResult(): ResultDTOOfValidatePlexCredentialsDTO {
	return {
		isSuccess: false,
		value: null,
		errors: [],
		successes: [],
		statusCode: 401,
	};
}

function successfulValidationResult(): ResultDTOOfValidatePlexCredentialsDTO {
	return {
		isSuccess: true,
		value: {
			isValidated: true,
			is2Fa: true,
			isUnAuthorized: false,
			authenticationToken: '',
			clientId: 'client-id',
			email: 'test@example.com',
			password: 'secret',
			plexId: 1,
			title: 'Title',
			username: 'username',
			uuid: 'uuid',
		} satisfies ValidatePlexCredentialsDTO,
		errors: [],
		successes: [],
		statusCode: 200,
	};
}

describe('AccountVerificationCodeDialog', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
		getAxiosMock();
		const store = useAccountDialogStore();
		store.verificationCode = '123456';
	});

	async function mountDialog() {
		const QCardDialogStub = defineComponent({
			name: 'QCardDialog',
			setup(_, { slots }) {
				return () => h('div', [
					slots.title?.(),
					slots['top-row']?.(),
					slots.default?.(),
					slots.actions?.({ close: vi.fn() }),
				]);
			},
		});

		return mountSuspended(AccountVerificationCodeDialog, {
			global: {
				stubs: {
					QCardDialog: QCardDialogStub,
					QSubHeader: SlotStub,
					QRow: SlotStub,
					QCol: SlotStub,
					ConfirmButton: ConfirmButtonStub,
					CancelButton: CancelButtonStub,
					VOtpInput: VOtpInputStub,
				},
			},
		});
	}

	test('Should show an error message and stop loading when verification returns a failed result', async () => {
		// Arrange
		const mock = getAxiosMock();
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(401, failedValidationResult());
		const wrapper = await mountDialog();

		// Act
		wrapper.findComponent(ConfirmButtonStub).vm.$emit('click');
		await flushPromises();

		// Assert
		expect(wrapper.text()).toContain('The verification code was invalid, please try again.');
		expect(wrapper.findComponent(ConfirmButtonStub).props('loading')).toBe(false);
	});

	test('Should keep the error hidden when verification succeeds immediately', async () => {
		// Arrange
		const mock = getAxiosMock();
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO(successfulValidationResult().value!));
		const wrapper = await mountDialog();

		// Act
		wrapper.findComponent(ConfirmButtonStub).vm.$emit('click');
		await flushPromises();

		// Assert
		expect(wrapper.findComponent(ConfirmButtonStub).props('loading')).toBe(false);
		expect(wrapper.text()).not.toContain('The verification code was invalid, please try again.');
	});

	test('Should clear a previous verification error after a successful retry', async () => {
		// Arrange
		const mock = getAxiosMock();
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint())
			.replyOnce(401, failedValidationResult())
			.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint())
			.reply(200, generateResultDTO(successfulValidationResult().value!));
		const wrapper = await mountDialog();

		// Act
		wrapper.findComponent(ConfirmButtonStub).vm.$emit('click');
		await flushPromises();
		expect(wrapper.text()).toContain('The verification code was invalid, please try again.');
		wrapper.findComponent(ConfirmButtonStub).vm.$emit('click');
		await flushPromises();

		// Assert
		expect(wrapper.text()).not.toContain('The verification code was invalid, please try again.');
		expect(wrapper.findComponent(ConfirmButtonStub).props('loading')).toBe(false);
	});
});
