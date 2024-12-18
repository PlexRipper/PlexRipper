<template>
	<!-- Help Label -->
	<div class="help-row-label">
		<QSubHeader>
			{{ help.label }}
		</QSubHeader>
	</div>
	<!-- Help Icon -->
	<div class="help-row-icon">
		<IconButton
			v-if="hasHelpPage"
			icon="mdi-help-circle-outline"
			class="q-ma-sm"
			@click="helpStore.openHelpDialog(help)" />
	</div>
	<!-- Default Form Slot -->
	<div :class="{ 'help-row-default-slot': true, 'q-py-sm': true, 'flex': centerSlot, 'justify-center': centerSlot } ">
		<slot />
	</div>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { useHelpStore } from '@store';
import type { IHelp } from '@interfaces';

const { t } = useI18n();
const helpStore = useHelpStore();

const props = withDefaults(defineProps<Partial<IHelp> & { value?: IHelp; centerSlot?: boolean }>(), {
	label: '',
	title: '',
	text: '',
	centerSlot: false,
});

const help = computed(() => props.value ?? {
	label: props.label !== '' ? props.label : t('help.default.label'),
	title: props.title,
	text: props.text,
});

const hasHelpPage = computed(() => {
	return get(help).title !== '' && get(help).text !== '';
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

.help-row {

  &-label, &-icon {
    display: flex;
    align-items: center;
  }

  &-label {
    white-space: nowrap;
  }

  &-icon {
    color: $primary;
  }

  &-default-slot {
    white-space: break-spaces;
    justify-content: left;
  }
}
</style>
