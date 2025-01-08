<template>
	<QCol
		align-self="stretch"
		:cols="12"
		style="max-height: 76px;">
		<q-separator class="q-mb-md" />
		<QRow
			justify="between"
			align="center"
			class="q-my-md">
			<!-- Language Selector -->
			<QCol :cols="2">
				<LanguageSelect
					class="q-ml-md"
					dense />
			</QCol>
			<!-- Navigation buttons -->
			<QCol>
				<QRow
					justify="center"
					align="center">
					<!-- Back button -->
					<QCol
						v-if="!isBackDisabled"
						class="q-mx-md"
						:cols="3">
						<NavigationPreviousButton
							:disabled="isBackDisabled"
							cy="setup-page-previous-button"
							@click="back" />
					</QCol>
					<!-- Next Button -->
					<QCol
						v-if="!isNextDisabled"
						:cols="isBackDisabled ? 9 : 3"
						class="q-mx-md">
						<ConfirmButton
							v-if="model === 1"
							cy="setup-disclaimer-accept-button"
							:label="$t('pages.setup.disclaimer.i-agree-disclaimer-button')"
							block
							@click="next" />
						<NavigationNextButton
							v-else
							block
							:disabled="isNextDisabled"
							cy="setup-page-next-button"
							@click="next" />
					</QCol>
				</QRow>
			</QCol>
			<!--	Skip button	-->
			<QCol
				class="q-mx-md"
				cols="auto">
				<NavigationSkipSetupButton
					v-if="isSkipVisible"
					:disabled="isNextDisabled"
					@click="dialogStore.openDialog(DialogType.SetupSkipConfirmationDialog)" />
				<!--	Finish button	-->
				<NavigationFinishSetupButton
					v-if="isFinishButtonVisible"
					cy="setup-page-skip-setup-button"
					@click="emits('finish')" />
				<ConfirmationDialog
					:name="DialogType.SetupSkipConfirmationDialog"
					:text="$t('confirmation.skip-setup.text')"
					:title="$t('confirmation.skip-setup.title')"
					@confirm="emits('finish')" />
			</QCol>
		</QRow>
	</QCol>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { DialogType } from '@enums';
import { useDialogStore } from '@store';

const dialogStore = useDialogStore();

const model = defineModel<number>({
	default: 1,
});

const props = defineProps<{ maxPages: number }>();

const emits = defineEmits<{
	(e: 'finish'): void;
}>();

const isBackDisabled = computed(() => {
	return get(model) === 1;
});

const isNextDisabled = computed(() => {
	return get(model) === props.maxPages;
});

const isSkipVisible = computed(() => {
	return !get(isNextDisabled) && get(model) !== 1;
});

const isFinishButtonVisible = computed(() => {
	return get(model) === props.maxPages;
});

function next() {
	if (model.value < props.maxPages) {
		set(model, get(model) + 1);
	}
}

function back() {
	if (get(model) > 1) {
		set(model, get(model) - 1);
	}
}
</script>
