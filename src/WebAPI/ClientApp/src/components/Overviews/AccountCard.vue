<template>
	<!-- Edit Account -->
	<q-card
		v-if="account"
		class="account-card"
		:data-cy="`account-card-id-${account.id}`"
		@click="$emit('open-dialog', account)"
	>
		<q-card-section v-if="!isNew">
			{{ account ? account.displayName : t('components.account-card.no-account-name') }}
		</q-card-section>
		<q-card-section>
			<!-- Validation Chip -->
			<q-chip
				v-if="account?.isValidated"
				color="green"
				text-color="white"
			>
				{{ t('general.commands.validated') }}
			</q-chip>
			<q-chip
				v-else
				color="red"
				text-color="white"
			>
				{{ t('general.commands.not-validated') }}
			</q-chip>
			<!-- IsEnabled Chip -->
			<q-chip
				v-if="account?.isEnabled"
				color="green"
				text-color="white"
			>
				{{ t('general.commands.enabled') }}
			</q-chip>
			<q-chip
				v-else
				color="red"
				text-color="white"
			>
				{{ t('general.commands.disabled') }}
			</q-chip>
		</q-card-section>
	</q-card>
	<!-- Add new account -->
	<q-card
		v-else-if="isNew"
		class="account-card"
		data-cy="account-overview-add-account"
		@click="$emit('open-dialog', null)"
	>
		<q-card-section
			v-if="isNew"
			class="text-center"
		>
			<q-icon
				name="mdi-plus-box-outline"
				style="font-size: 90px"
			/>
		</q-card-section>
	</q-card>
	<!-- Account was invalid -->
	<q-card v-else>
		<q-card-section>
			<span>{{ t('components.account-card.invalid-account') }}</span>
		</q-card-section>
	</q-card>
</template>

<script setup lang="ts">
import type { PlexAccountDTO } from '@dto';

const { t } = useI18n();

const props = defineProps<{
	account?: PlexAccountDTO;
}>();

defineEmits<{ (e: 'open-dialog', account: PlexAccountDTO | null): void }>();

const isNew = computed(() => !props.account);
</script>

<style lang="scss">
.account-card {
	border: 2px solid red;
	max-height: 124px;

	&:hover {
		box-shadow: 0 0 20px 3px red;
		cursor: pointer;
		transition: box-shadow 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
	}
}
</style>
