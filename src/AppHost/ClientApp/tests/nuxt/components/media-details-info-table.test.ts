import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia, type Pinia } from 'pinia';
import { defineComponent, h } from 'vue';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { baseSetup } from '@services-test-base';
import { useDialogStore } from '@store';
import { DialogType } from '@enums';
import MediaDetailsInfoTable from '@components/MediaOverview/MediaDetailsInfoTable.vue';
import messages from '@/lang/en-US.json';

const CardDialog = defineComponent({
	name: 'QCardDialog',
	inheritAttrs: false,
	props: {
		cy: {
			type: String,
			default: 'q-card-dialog-cy',
		},
	},
	emits: ['closed'],
	setup(props, { slots, emit }) {
		const close = () => emit('closed');
		return () => h('section', { 'data-cy': props.cy }, [
			slots.title?.(),
			slots.default?.(),
			slots.actions?.({ close }),
		]);
	},
});

describe('MediaDetailsInfoTable summary', () => {
	const mounted: Array<{ unmount: () => void }> = [];
	let pinia: Pinia;

	beforeAll(() => baseSetup());
	beforeEach(() => {
		pinia = createPinia();
		setActivePinia(pinia);
	});
	afterEach(() => {
		mounted.forEach((wrapper) => wrapper.unmount());
		mounted.length = 0;
		vi.restoreAllMocks();
	});

	async function render(summary: string) {
		const dialogStore = useDialogStore();
		const openDialog = vi.spyOn(dialogStore, 'openDialog');
		const wrapper = await mountSuspended(MediaDetailsInfoTable, {
			props: {
				title: 'Test media',
				summaryLabel: 'Summary:',
				summary,
			},
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: { QCardDialog: CardDialog },
			},
		});
		mounted.push(wrapper);
		return { wrapper, openDialog };
	}

	test('Should omit an empty summary row', async () => {
		// Arrange / Act
		const { wrapper } = await render('  \n\t');

		// Assert
		expect(wrapper.findAll('tbody tr')).toHaveLength(1);
		expect(wrapper.find('[data-cy="media-summary-read-more"]').exists()).toBe(false);
	});

	test('Should expose the whole short summary as a link trigger', async () => {
		// Arrange / Act
		const { wrapper, openDialog } = await render('A short summary.');

		// Assert
		expect(wrapper.findAll('tbody tr')).toHaveLength(2);
		expect(wrapper.get('tbody').text()).toContain('A short summary.');
		expect(wrapper.get('.media-summary-row td').attributes('colspan')).toBe('2');
		expect(wrapper.get('[data-cy="media-summary-read-more"]').element.tagName).toBe('A');
		expect(wrapper.get('.media-summary-read-more').text()).toBe('Read more');

		await wrapper.get('.media-summary-preview').trigger('click');
		expect(openDialog).toHaveBeenCalledWith(DialogType.MediaSummaryDialog);
	});

	test('Should render complete paragraphs in the shared scrollable dialog', async () => {
		// Arrange
		const summary = [
			'First paragraph.',
			'Second paragraph should keep its own spacing.',
			`A long paragraph ${'with additional detail '.repeat(30)}`,
		].join('\n');
		const { wrapper, openDialog } = await render(summary);

		// Act
		await wrapper.get('.media-summary-preview').trigger('click');

		// Assert
		expect(openDialog).toHaveBeenCalledWith(DialogType.MediaSummaryDialog);
		const dialog = wrapper.get('[data-cy="media-summary-dialog"]');
		expect(dialog.findAll('.media-summary-dialog__paragraph')).toHaveLength(3);
		expect(dialog.text()).toContain('First paragraph.');
		expect(dialog.text()).toContain('Second paragraph should keep its own spacing.');
		expect(dialog.text()).toContain('A long paragraph');
		expect(wrapper.find('[data-cy="media-summary-dialog-close"]').exists()).toBe(true);
	});
});
