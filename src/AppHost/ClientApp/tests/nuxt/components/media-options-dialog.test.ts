import { afterEach, beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia, type Pinia } from 'pinia';
import { defineComponent } from 'vue';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import type { VueWrapper } from '@vue/test-utils';
import { baseSetup } from '@services-test-base';
import { useSettingsStore } from '@store';
import MediaOptionsDialog from '@/components/Dialogs/MediaOptionsDialog.vue';
import messages from '@/lang/en-US.json';

const cardDialogStub = defineComponent({
	name: 'QCardDialogStub',
	emits: ['opened', 'closed'],
	template: '<div><slot name="title" /><slot /></div>',
});

const passthrough = defineComponent({
	template: '<div><slot /></div>',
});

describe('MediaOptionsDialog close visibility event', () => {
	let pinia: Pinia;
	let wrapper: VueWrapper | undefined;
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		pinia = createPinia();
		setActivePinia(pinia);
	});

	afterEach(() => {
		wrapper?.unmount();
		wrapper = undefined;
	});

	async function render() {
		wrapper = await mountSuspended(MediaOptionsDialog, {
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: {
					QCardDialog: cardDialogStub,
					HelpRow: passthrough,
					'q-toggle': passthrough,
				},
			},
		});
		return wrapper;
	}

	test('Should emit visibilityChanged when an owned or offline setting changes', async () => {
		// Arrange
		const settingsStore = useSettingsStore();
		const rendered = await render();
		const cardDialog = rendered.findComponent(cardDialogStub);
		cardDialog.vm.$emit('opened');
		settingsStore.generalSettings.hideMediaFromOwnedServers = true;

		// Act
		cardDialog.vm.$emit('closed');

		// Assert
		expect(rendered.emitted('closed')).toEqual([[true, true]]);
	});

	test('Should not mark poster-only changes as visibility changes', async () => {
		// Arrange
		const settingsStore = useSettingsStore();
		const rendered = await render();
		const cardDialog = rendered.findComponent(cardDialogStub);
		cardDialog.vm.$emit('opened');
		settingsStore.generalSettings.useLowQualityPosterImages = true;

		// Act
		cardDialog.vm.$emit('closed');

		// Assert
		expect(rendered.emitted('closed')).toEqual([[true, false]]);
	});
});
