<template>
	<div
		:class="{
			'code-input-container': true,
			[className]: !!className,
		}">
		<div class="code-input">
			<p v-if="title" class="title">{{ title }}</p>
			<template v-for="(v, index) in values" :key="index">
				<input
					:ref="
						(el) => {
							if (el) inputs[index + 1] = el;
						}
					"
					class="w-14 h-14 rounded-lg border border-gray outline-none focus:outline-none focus:border-primary focus:ring-0 text-center transition-all"
					type="number"
					pattern="[0-9]"
					:style="{
						width: `${props.fieldWidth}px`,
						height: `${props.fieldHeight}px`,
					}"
					:autoFocus="autoFocus && index === autoFocusIndex"
					:data-id="index"
					:value="v"
					:required="props.required"
					:disabled="props.disabled"
					maxlength="1"
					@input="onValueChange"
					@focus="onFocus"
					@keydown="onKeyDown" />
			</template>
		</div>
	</div>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref, toRef, onBeforeUpdate } from 'vue';

const props = defineProps({
	className: String,
	fields: {
		type: Number,
		default: 3,
	},
	fieldWidth: {
		type: Number,
		default: 56,
	},
	fieldHeight: {
		type: Number,
		default: 56,
	},
	disabled: {
		type: Boolean,
		default: false,
	},
	required: {
		type: Boolean,
		default: true,
	},
	title: String,
});

const emit = defineEmits(['change', 'complete']);

const KEY_CODE = {
	backspace: 8,
	delete: 46,
	left: 37,
	up: 38,
	right: 39,
	down: 40,
};

const values = ref([]);
const iRefs = ref([]);
const inputs = ref<InstanceType<typeof HTMLInputElement>[]>([]);
const fields = toRef(props, 'fields');
const autoFocusIndex = ref(0);
const autoFocus = true;

const initVals = () => {
	let vals;
	if (values.value && values.value.length) {
		vals = [];
		for (let i = 0; i < fields.value; i++) {
			vals.push(values.value[i] || '');
		}
		autoFocusIndex.value = values.value.length >= fields.value ? 0 : values.value.length;
	} else {
		vals = Array(fields.value).fill('');
	}
	iRefs.value = [];
	for (let i = 0; i < fields.value; i++) {
		// @ts-ignore
		iRefs.value.push(i + 1);
	}
	values.value = vals;
};

const onFocus = (e) => {
	e.target.select(e);
};

const onValueChange = (e) => {
	const index = parseInt(e.target.dataset.id);
	e.target.value = e.target.value.replace(/[^\d]/gi, '');
	// this.handleKeys[index] = false;
	if (e.target.value === '' || !e.target.validity.valid) {
		return;
	}
	let next;
	const value = e.target.value;
	values.value = Object.assign([], values.value);
	if (value.length > 1) {
		let nextIndex = value.length + index - 1;
		if (nextIndex >= fields.value) {
			nextIndex = fields.value - 1;
		}
		next = iRefs.value[nextIndex];
		const split = value.split('');
		split.forEach((item, i) => {
			const cursor = index + i;
			if (cursor < fields.value) {
				// @ts-ignore
				values.value[cursor] = item;
			}
		});
	} else {
		next = iRefs.value[index + 1];
		// @ts-ignore
		values.value[index] = value;
	}
	if (next) {
		const element = inputs.value[next];
		element.focus();
		element.select();
	}
	triggerChange(values.value);
};

const onKeyDown = (e) => {
	const index = parseInt(e.target.dataset.id);
	const prevIndex = index - 1;
	const nextIndex = index + 1;
	const prev = iRefs.value[prevIndex];
	const next = iRefs.value[nextIndex];
	switch (e.keyCode) {
		case KEY_CODE.backspace: {
			e.preventDefault();
			const vals = [...values.value];
			if (values.value[index]) {
				// @ts-ignore
				vals[index] = '';
				values.value = vals;
				triggerChange(vals);
			} else if (prev) {
				// @ts-ignore
				vals[prevIndex] = '';
				inputs.value[prev].focus();
				values.value = vals;
				triggerChange(vals);
			}
			break;
		}
		case KEY_CODE.delete: {
			e.preventDefault();
			const vals = [...values.value];
			if (values.value[index]) {
				// @ts-ignore
				vals[index] = '';
				values.value = vals;
				triggerChange(vals);
			} else if (next) {
				// @ts-ignore
				vals[nextIndex] = '';
				inputs.value[next].focus();
				values.value = vals;
				triggerChange(vals);
			}
			break;
		}
		case KEY_CODE.left:
			e.preventDefault();
			if (prev) {
				inputs.value[prev].focus();
			}
			break;
		case KEY_CODE.right:
			e.preventDefault();
			if (next) {
				inputs.value[next].focus();
			}
			break;
		case KEY_CODE.up:
		case KEY_CODE.down:
			e.preventDefault();
			break;
		default:
			// this.handleKeys[index] = true;
			break;
	}
};

const triggerChange = (newValue = values.value) => {
	const val = newValue.join('');
	emit('change', val);
	emit('complete', val.length >= fields.value);
};

initVals();

onBeforeUpdate(() => {
	inputs.value = [];
});
</script>

<style scoped>
.code-input-container {
	position: relative;
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 20px;
}

.code-input {
	display: flex;
	flex-direction: row;
	justify-content: center;
	gap: 10px;
}

.code-input > input {
	border: solid 1px #a8adb7;
	font-family: 'Lato';
	font-size: 20px;
	border-radius: 8px;
	text-align: center;
	transition: 0.2s all ease-in-out;
	color: #525461;
	box-sizing: border-box;
	-webkit-appearance: initial;
}

.code-input > input:focus {
	outline: none;
	border: 1px solid #006fff;
	caret-color: #006fff;
}

.title {
	margin: 0;
	height: 20px;
	padding-bottom: 10px;
}
</style>
