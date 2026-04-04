import { mountSuspended } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { describe, beforeEach, expect, test, vi } from 'vitest';
import { defineComponent, h, reactive } from 'vue';
import { of } from 'rxjs';
import { generateFailedResultDTO } from '@mock';
import AuthenticationOverview from '@components/Overviews/AuthenticationOverview.vue';

const authStoreMock = reactive({
	username: 'updated-user',
	password: 'updated-password',
	confirmPassword: 'updated-password',
	isPasswordValid: true,
	hasPasswordChanged: true,
	equalPassword: true,
	canUpdateCredentials: true,
	refreshCredentials: vi.fn(() => of(null)),
	updateCredentials: vi.fn(() => of(generateFailedResultDTO())),
});

vi.mock('vue-i18n', () => ({
	useI18n: () => ({
		t: (key: string) => key,
	}),
}));

vi.mock('@store', () => ({
	useAuthenticationStore: () => authStoreMock,
}));

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const InputStub = defineComponent({
	name: 'InputStub',
	props: {
		modelValue: {
			type: String,
			default: '',
		},
	},
	emits: ['update:modelValue', 'blur'],
	setup(props, { emit }) {
		return () => h('input', {
			value: props.modelValue,
			onInput: (event: Event) => emit('update:modelValue', (event.target as HTMLInputElement).value),
			onBlur: () => emit('blur'),
		});
	},
});

const ValidationButtonStub = defineComponent({
	name: 'ValidationButton',
	props: {
		loading: {
			type: Boolean,
			default: false,
		},
		isValidated: {
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
		return () => h('button', {
			'data-cy': 'save-credentials-button',
			'data-loading': props.loading ? 'true' : 'false',
			'data-validated': props.isValidated ? 'true' : 'false',
			disabled: props.disabled,
			onClick: () => emit('click'),
		}, 'save');
	},
});

describe('AuthenticationOverview', () => {
	beforeEach(() => {
		authStoreMock.username = 'updated-user';
		authStoreMock.password = 'updated-password';
		authStoreMock.confirmPassword = 'updated-password';
		authStoreMock.isPasswordValid = true;
		authStoreMock.hasPasswordChanged = true;
		authStoreMock.equalPassword = true;
		authStoreMock.canUpdateCredentials = true;
		authStoreMock.refreshCredentials = vi.fn(() => of(null));
		authStoreMock.updateCredentials = vi.fn(() => of(generateFailedResultDTO()));
	});

	test('Should keep the save button invalid when updating credentials fails', async () => {
		// Arrange
		const wrapper = await mountSuspended(AuthenticationOverview, {
			global: {
				stubs: {
					HelpRow: SlotStub,
					PasswordInputField: InputStub,
					PasswordStrength: SlotStub,
					QAlert: SlotStub,
					ValidationButton: ValidationButtonStub,
					'q-input': InputStub,
				},
			},
		});

		// Act
		await wrapper.find('[data-cy="save-credentials-button"]').trigger('click');
		await flushPromises();

		// Assert
		expect(wrapper.find('[data-cy="save-credentials-button"]').attributes('data-loading')).toBe('false');
		expect(wrapper.find('[data-cy="save-credentials-button"]').attributes('data-validated')).toBe('false');
	});
});
