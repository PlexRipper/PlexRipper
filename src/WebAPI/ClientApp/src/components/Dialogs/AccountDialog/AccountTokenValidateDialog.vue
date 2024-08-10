<template>
	<QCardDialog
		:name="name"
		persistent
		max-width="700px"
		cy="auth-token-validation-dialog">
		<template #title>
			{{ $t('components.account-token-validate-dialog.title') }}
		</template>
		<template #top-row>
			<QSubHeader>{{ $t('components.account-token-validate-dialog.sub-header', { authToken: account.authenticationToken }) }}</QSubHeader>
		</template>
		<template #default>
			<q-markup-table wrap-cells>
				<tbody>
					<tr>
						<td>
							<QText :value="$t('components.account-token-validate-dialog.column.title')" />
						</td>
						<td>
							<QText :value="account.title" />
						</td>
					</tr>
					<tr>
						<td>
							<QText :value="$t('components.account-token-validate-dialog.column.username')" />
						</td>
						<td>
							<QText :value="account.username" />
						</td>
					</tr>
					<tr>
						<td>
							<QText :value="$t('components.account-token-validate-dialog.column.email')" />
						</td>
						<td>
							<QText :value="account.email" />
						</td>
					</tr>
				</tbody>
			</q-markup-table>
		</template>
		<template #actions="{ close }">
			<QRow justify="end">
				<!--	Confirm	-->
				<QCol cols="auto">
					<HideButton
						cy="auth-token-validation-dialog-confirm-button"
						@click="close" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import type { PlexAccountDTO } from '@dto';

defineProps<{
	name: string;
	account: PlexAccountDTO;
}>();

defineEmits<{
	(e: 'close' | 'confirm'): void;
}>();
</script>
