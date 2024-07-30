<template>
	<QRow
		justify="between"
		class="q-my-md"
	>
		<q-btn
			color="white"
			text-color="black"
			:label="$t('debug.button-labels.standard')"
		/>
		<q-btn
			color="primary"
			:label="$t('debug.button-labels.primary')"
		/>
		<q-btn
			color="secondary"
			:label="$t('debug.button-labels.secondary')"
		/>
		<q-btn
			color="amber"
			glossy
			:label="$t('debug.button-labels.amber')"
		/>
		<q-btn
			color="brown-5"
			:label="$t('debug.button-labels.brown5')"
		/>
		<q-btn
			color="deep-orange"
			glossy
			:label="$t('debug.button-labels.deep-orange')"
		/>
		<q-btn
			color="purple"
			:label="$t('debug.button-labels.purple')"
		/>
		<q-btn
			color="black"
			:label="$t('debug.button-labels.black')"
		/>
	</QRow>
	<q-markup-table>
		<tbody>
			<tr>
				<td>{{ $t('debug.buttons.base-button') }}</td>
				<td>
					<BaseButton />
				</td>
				<td>{{ $t('debug.buttons.add-icon-button') }}</td>
				<td>
					<AddIconButton />
				</td>
			</tr>
			<tr>
				<td>{{ $t('debug.buttons.account-validation-button') }}</td>
				<td>
					<AccountValidationButton />
				</td>
				<td>{{ $t('debug.buttons.cancel-button') }}</td>
				<td>
					<CancelButton />
				</td>
			</tr>
			<tr>
				<td>{{ $t('debug.buttons.check-connection-button') }}</td>
				<td>
					<CheckConnectionButton
						:loading="checkConnectionButton"
						@click="checkConnectionButton = !checkConnectionButton"
					/>
					{{ checkConnectionButton }}
				</td>
				<td>{{ $t('debug.buttons.save-button') }}</td>
				<td>
					<SaveButton />
				</td>
			</tr>
			<tr>
				<td>{{ $t('debug.buttons.debug-button') }}</td>
				<td>
					<DebugButton />
				</td>
				<td>{{ $t('debug.buttons.delete-button') }}</td>
				<td>
					<DeleteButton />
				</td>
			</tr>

			<tr>
				<td>{{ $t('debug.buttons.external-link-button') }}</td>
				<td>
					<ExternalLinkButton />
				</td>
				<td>{{ $t('debug.buttons.navigation-next-button') }}</td>
				<td>
					<NavigationNextButton />
				</td>
			</tr>
			<tr>
				<td>{{ $t('debug.buttons.go-to-button') }}</td>
				<td>
					<GoToButton />
				</td>
				<td>{{ $t('debug.buttons.navigation-previous-button') }}</td>
				<td>
					<NavigationPreviousButton />
				</td>
			</tr>
			<tr>
				<td>{{ $t('debug.buttons.help-button') }}</td>
				<td>
					<HelpButton />
				</td>
				<td>{{ $t('debug.buttons.navigation-skip-setup-button') }}</td>
				<td>
					<NavigationSkipSetupButton />
				</td>
			</tr>
			<tr>
				<td>{{ $t('debug.buttons.hide-button') }}</td>
				<td>
					<HideButton />
				</td>
				<td>{{ $t('debug.buttons.reset-button') }}</td>
				<td>
					<ResetButton />
				</td>
			</tr>
			<tr>
				<td>{{ $t('debug.buttons.confirm-button') }}</td>
				<td>
					<ConfirmButton />
				</td>
				<td>{{ $t('debug.buttons.warning-button') }}</td>
				<td>
					<WarningButton :label="$t('general.commands.reset-db')" />
				</td>
			</tr>
			<tr>
				<td>{{ $t('debug.buttons.navigation-finish-setup-button') }}</td>
				<td>
					<NavigationFinishSetupButton />
				</td>
				<td>{{ $t('debug.buttons.delete-icon-button') }}</td>
				<td>
					<DeleteIconButton />
				</td>
			</tr>
		</tbody>
	</q-markup-table>
</template>

<script setup lang="ts">
const checkConnectionButton = ref<boolean>(false);
</script>
