<template>
	<v-card hover max-height="130" @click="openDialog()">
		<v-card-title v-if="!isNew" class="headline">{{ account ? account.displayName : '' }}</v-card-title>
		<!-- Add new account -->
		<v-card-text v-if="isNew" class="text-center">
			<v-icon style="font-size: 100px">mdi-plus-box-outline</v-icon>
		</v-card-text>
		<!-- Edit Account -->
		<v-card-text v-else>
			<template>
				<v-chip v-if="account.isValidated" class="ma-2" color="green" text-color="white"> Validated </v-chip>
				<v-chip v-else class="ma-2" color="red" text-color="white"> NotValidated </v-chip>
			</template>
			<v-chip v-if="account.isEnabled" class="ma-2" color="green" text-color="white"> Enabled </v-chip>
			<v-chip v-else class="ma-2" color="red" text-color="white"> Disabled </v-chip>
		</v-card-text>
	</v-card>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import type { PlexAccountDTO } from '@dto/mainApi';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import HelpIcon from '@components/Help/HelpIcon.vue';
import Log from 'consola';

@Component({
	components: {
		LoadingSpinner,
		HelpIcon,
	},
})
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
