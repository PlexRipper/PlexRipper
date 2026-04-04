import { mountSuspended } from '@nuxt/test-utils/runtime';
import { describe, beforeEach, expect, test, vi } from 'vitest';
import { defineComponent, h, nextTick } from 'vue';
import MediaSelectionDialog from '@components/Dialogs/MediaSelectionDialog.vue';

const { mediaOverviewStoreMock } = vi.hoisted(() => ({
	mediaOverviewStoreMock: {
		itemsLength: 10,
		setSelectionRange: vi.fn<(min: number, max: number) => void>(),
	},
}));

vi.mock('@store', () => ({
	useMediaOverviewStore: vi.fn(() => mediaOverviewStoreMock),
}));

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const QCardDialogStub = defineComponent({
	name: 'QCardDialog',
	emits: ['opened', 'closed'],
	setup(_, { slots }) {
		return () => h('div', [
			slots.title?.(),
			slots.default?.(),
			slots.actions?.({ close: vi.fn() }),
		]);
	},
});

const QInputStub = defineComponent({
	name: 'q-input',
	props: {
		modelValue: {
			type: [Number, String],
			default: '',
		},
	},
	emits: ['update:modelValue'],
	setup() {
		return () => h('input');
	},
});

const QRangeStub = defineComponent({
	name: 'q-range',
	props: {
		modelValue: {
			type: Object,
			required: true,
		},
		min: {
			type: Number,
			default: 1,
		},
		max: {
			type: Number,
			default: 1,
		},
	},
	setup() {
		return () => h('div');
	},
});

const BaseButtonStub = defineComponent({
	name: 'BaseButton',
	props: {
		label: {
			type: String,
			default: '',
		},
	},
	emits: ['click'],
	setup(props, { emit }) {
		return () => h('button', { onClick: () => emit('click') }, props.label);
	},
});

describe('MediaSelectionDialog', () => {
	beforeEach(() => {
		mediaOverviewStoreMock.itemsLength = 10;
		mediaOverviewStoreMock.setSelectionRange.mockReset();
	});

	async function mountDialog() {
		const wrapper = await mountSuspended(MediaSelectionDialog, {
			global: {
				stubs: {
					QCardDialog: QCardDialogStub,
					QRow: SlotStub,
					QCol: SlotStub,
					BaseButton: BaseButtonStub,
					'q-input': QInputStub,
					'q-range': QRangeStub,
				},
			},
		});

		wrapper.findComponent(QCardDialogStub).vm.$emit('opened');
		await nextTick();

		return wrapper;
	}

	test('Should treat typed minimum input as an absolute value', async () => {
		// Arrange
		const wrapper = await mountDialog();

		// Act
		wrapper.findAllComponents(QInputStub)[0]!.vm.$emit('update:modelValue', 3);
		await nextTick();

		// Assert
		expect(wrapper.findComponent(QRangeStub).props('modelValue')).toEqual({
			min: 3,
			max: 10,
		});
	});

	test('Should treat typed maximum input as an absolute value', async () => {
		// Arrange
		const wrapper = await mountDialog();

		// Act
		wrapper.findAllComponents(QInputStub)[1]!.vm.$emit('update:modelValue', 7);
		await nextTick();

		// Assert
		expect(wrapper.findComponent(QRangeStub).props('modelValue')).toEqual({
			min: 1,
			max: 7,
		});
	});
});
