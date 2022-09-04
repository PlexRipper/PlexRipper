<template>
	<v-card hover outlined max-height="130" class="glass-background" @click="openDialog()">
		<v-card-title v-if="!isNew">{{
			account ? account.displayName : $t('components.account-card.no-account-name')
		}}</v-card-title>
		<!-- Add new account -->
		<v-card-text v-if="isNew" class="text-center">
			<v-icon style="font-size: 100px">mdi-plus-box-outline</v-icon>
		</v-card-text>
		<!-- Edit Account -->
		<v-card-text v-else>
			<!-- Validation Chip -->
			<v-chip v-if="account.isValidated" class="ma-2" color="green" text-color="white">
				{{ $t('general.commands.validated') }}
			</v-chip>
			<v-chip v-else class="ma-2" color="red" text-color="white">
				{{ $t('general.commands.not-validated') }}
			</v-chip>
			<!-- IsEnabled Chip -->
			<v-chip v-if="account.isEnabled" class="ma-2" color="green" text-color="white">
				{{ $t('general.commands.enabled') }}
			</v-chip>
			<v-chip v-else class="ma-2" color="red" text-color="white">
				{{ $t('general.commands.disabled') }}
			</v-chip>
		</v-card-text>
	</v-card>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import type { PlexAccountDTO } from '@dto/mainApi';

@Component
export default class AccountCard extends Vue {
	@Prop({ type: Object as () => PlexAccountDTO })
	readonly account!: PlexAccountDTO;

	validateLoading: boolean = false;

	valid: boolean = false;

	displayName: string = '';

	isEnabled: boolean = true;

	get isNew(): boolean {
		return !this.account;
	}

	openDialog(): void {
		this.$emit('open-dialog', this.account);
	}
}
</script>
