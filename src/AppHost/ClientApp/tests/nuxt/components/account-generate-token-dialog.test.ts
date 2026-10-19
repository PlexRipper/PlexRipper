import { afterEach, beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia, type Pinia } from 'pinia';
import { defineComponent, h, nextTick } from 'vue';
import { flushPromises, type VueWrapper } from '@vue/test-utils';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import type { GeneratePlexTokenResponse } from '@dto';
import { PlexAccountPaths } from '@api-urls';
import { useAccountDialogStore } from '@store';
import AccountGenerateTokenDialog from '@/components/Dialogs/AccountDialog/AccountGenerateTokenDialog.vue';
import messages from '@/lang/en-US.json';

const cardDialogStub = defineComponent({
	name: 'QCardDialogStub',
	emits: ['opened', 'closed'],
	setup(_, { slots }) {
		return () => h('section', [
			slots.title?.(),
			slots['top-row']?.(),
			slots.default?.(),
			slots.actions?.({ close: () => undefined }),
		]);
	},
});

const inputStub = defineComponent({
	inheritAttrs: false,
	props: {
		modelValue: {
			type: String,
			default: '',
		},
	},
	setup(props, { attrs }) {
		return () => h('input', { ...attrs, value: props.modelValue });
	},
});

describe('AccountGenerateTokenDialog', () => {
	const endpoint = PlexAccountPaths.generatePlexTokenEndpoint(42);
	let { mock } = baseVars();
	let pinia: Pinia;
	let wrapper: VueWrapper | undefined;

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		pinia = createPinia();
		setActivePinia(pinia);
		useAccountDialogStore().id = 42;
	});

	afterEach(() => {
		wrapper?.unmount();
		wrapper = undefined;
		mock.restore();
	});

	async function render() {
		wrapper = await mountSuspended(AccountGenerateTokenDialog, {
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: {
					QCardDialog: cardDialogStub,
					'q-input': inputStub,
					VOtpInput: true,
					IconButton: true,
					HideButton: true,
				},
			},
		});
		return wrapper;
	}

	test('Should show an inline token error and clear it when retrying after reopening', async () => {
		// Arrange
		const errorMessage = 'Plex token generation failed';
		const successfulResponse = generateResultDTO<GeneratePlexTokenResponse>({
			isUnAuthorized: false,
			needsVerificationCode: false,
			plexAuthToken: 'retried-plex-token',
		});
		const retryResponse = Promise.withResolvers<[number, typeof successfulResponse]>();
		mock.onGet(endpoint)
			.replyOnce(500, generateFailedResultDTO({
				statusCode: 500,
				errors: [{ message: errorMessage, reasons: [], metadata: {} }],
			}))
			.onGet(endpoint)
			.reply(() => retryResponse.promise);
		const rendered = await render();
		const cardDialog = rendered.findComponent(cardDialogStub);

		// Act
		cardDialog.vm.$emit('opened');
		await flushPromises();

		// Assert
		expect(rendered.get('[data-cy="generate-token-error"]').text()).toBe(errorMessage);
		expect(rendered.find('[data-cy="generate-token-dialog-token-input"]').attributes('value')).toBe('');

		// Act
		cardDialog.vm.$emit('closed');
		cardDialog.vm.$emit('opened');
		await nextTick();
		expect(rendered.find('[data-cy="generate-token-error"]').exists()).toBe(false);
		retryResponse.resolve([200, successfulResponse]);
		await flushPromises();

		// Assert
		expect(rendered.find('[data-cy="generate-token-dialog-token-input"]').attributes('value')).toBe('retried-plex-token');
		expect(rendered.find('[data-cy="generate-token-error"]').exists()).toBe(false);
	});

	test.each([
		{ kind: 'failure', status: 500 },
		{ kind: 'success', status: 200 },
	])('Should retain the newer token when an older $kind arrives late', async ({ status }) => {
		// Arrange
		const olderResponse = status === 500
			? generateFailedResultDTO({
					statusCode: 500,
					errors: [{ message: 'Older request failed', reasons: [], metadata: {} }],
				})
			: generateResultDTO<GeneratePlexTokenResponse>({
					isUnAuthorized: false,
					needsVerificationCode: false,
					plexAuthToken: 'older-plex-token',
				});
		const olderReply = Promise.withResolvers<[number, typeof olderResponse]>();
		mock.onGet(endpoint)
			.replyOnce(() => olderReply.promise)
			.onGet(endpoint)
			.reply(200, generateResultDTO<GeneratePlexTokenResponse>({
				isUnAuthorized: false,
				needsVerificationCode: false,
				plexAuthToken: 'newer-plex-token',
			}));
		const rendered = await render();
		const cardDialog = rendered.findComponent(cardDialogStub);
		cardDialog.vm.$emit('opened');
		await flushPromises();

		// Act
		cardDialog.vm.$emit('closed');
		cardDialog.vm.$emit('opened');
		await flushPromises();
		expect(rendered.find('[data-cy="generate-token-dialog-token-input"]').attributes('value')).toBe('newer-plex-token');
		olderReply.resolve([status, olderResponse]);
		await flushPromises();

		// Assert
		expect(rendered.find('[data-cy="generate-token-dialog-token-input"]').attributes('value')).toBe('newer-plex-token');
		expect(rendered.find('[data-cy="generate-token-error"]').exists()).toBe(false);
	});
});
