<template>
	<QCardDialog
		:name="DialogType.AccountDialog"
		persistent
		:type="{} as IAccountDialog"
		cy="account-dialog-form"
		close-button
		@opened="accountDialogStore.openDialog"
		@closed="accountDialogStore.closeDialog">
		<!-- Dialog Header -->
		<template #title>
			<QRow>
				<QCol>
					<QText
						size="h5"
						bold="bold">
						{{ getDialogHeader }}
					</QText>
				</QCol>
			</QRow>
		</template>
		<template #default>
			<div>
				<AccountForm />
				<Print>{{ accountDialogStore.$state }}</Print>
			</div>
		</template>
		<!-- Dialog Actions	-->
		<template #actions>
			<QRow
				justify="between"
				gutter="md">
				<QCol cols="5">
					<QRow gutter="sm">
						<!-- Delete account -->
						<QCol v-if="!accountDialogStore.isNewAccount">
							<DeleteButton
								block
								cy="account-dialog-delete-button"
								@click="dialogStore.openDialog(DialogType.AccountConfirmationDialog)" />
						</QCol>
						<QCol v-if="!accountDialogStore.isNewAccount && !accountDialogStore.isAuthTokenMode">
							<BaseButton
								block
								cy="account-dialog-generate-token-button"
								:label="$t('components.account-dialog.generate-token-button')"
								@click="dialogStore.openDialog(DialogType.AccountGenerateTokenDialog)" />
						</QCol>
					</QRow>
				</QCol>
				<QCol cols="5">
					<QRow gutter="sm">
						<!-- Validation button -->
						<QCol>
							<AccountValidationButton
								:color="validationStyle.color"
								:icon="validationStyle.icon"
								:label="validationStyle.text"
								:loading="accountDialogStore.validateLoading"
								:disabled="accountDialogStore.validateLoading"
								cy="account-dialog-validate-button"
								block
								@click="validatePlexAccount" />
						</QCol>
						<!-- Save account -->
						<QCol>
							<SaveButton
								:label="accountDialogStore.isNewAccount ? $t('general.commands.save') : $t('general.commands.update')"
								:cy="`account-dialog-${accountDialogStore.isNewAccount ? 'save' : 'update'}-button`"
								:loading="accountDialogStore.savingLoading"
								block
								@click="saveAccount" />
						</QCol>
					</QRow>
				</QCol>
			</QRow>
		</template>
	</QCardDialog>

	<!--	Account Verification Code Dialog	-->
	<AccountVerificationCodeDialog />

	<!--	Account Token Validate Dialog	-->
	<AccountTokenValidateDialog />

	<!--	Account Generate Token Dialog	-->
	<AccountGenerateTokenDialog />

	<!--	Delete Confirmation Dialog	-->
	<ConfirmationDialog
		class="q-mr-md"
		:confirm-loading="accountDialogStore.deleteLoading"
		:name="DialogType.AccountConfirmationDialog"
		:title="$t('confirmation.delete-account.title')"
		:text="$t('confirmation.delete-account.text')"
		:warning="$t('confirmation.delete-account.warning')"
		@confirm="deleteAccount" />
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import type { IAccountDialog } from '@interfaces';
import { DialogType } from '@enums';
import { useAccountDialogStore } from '@store';
import { useDialogStore, useI18n } from '#imports';

const { t } = useI18n();
const accountDialogStore = useAccountDialogStore();
const dialogStore = useDialogStore();

const getDialogHeader = computed(() => {
	const displayName = accountDialogStore.displayName;
	let title: string;
	if (accountDialogStore.isNewAccount) {
		title = t('components.account-dialog.add-account-title', {
			name: displayName,
		});
	} else {
		title = t('components.account-dialog.edit-account-title', {
			name: displayName,
		});
	}
	// Remove the colon if the display name is empty
	return displayName ? title : title.replace(':', '');
});

const validationStyle = computed(
	(): {
		color: 'default' | 'positive' | 'warning' | 'negative';
		icon: string;
		text: string;
	} => {
		if (accountDialogStore.hasValidationErrors) {
			return {
				color: 'negative',
				icon: 'mdi-alert-circle-outline',
				text: t('general.commands.validate'),
			};
		}
		if (accountDialogStore.isValidated && !accountDialogStore.hasValidationErrors) {
			return {
				color: 'positive',
				icon: 'mdi-check-bold',
				text: '',
			};
		}
		return {
			color: 'default',
			icon: 'mdi-text-box-search-outline',
			text: t('general.commands.validate'),
		};
	},
);

function validatePlexAccount() {
	useSubscription(accountDialogStore.validatePlexAccount().subscribe());
}

function deleteAccount() {
	useSubscription(accountDialogStore.deleteAccount().subscribe());
}

function saveAccount() {
	useSubscription(accountDialogStore.saveAccount().subscribe());
}
</script>
