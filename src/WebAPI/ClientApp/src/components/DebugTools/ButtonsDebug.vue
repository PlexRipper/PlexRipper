<template>
	<QRow
		justify="between"
		class="q-my-md"
	>
		<q-btn
			color="white"
			text-color="black"
			label="Standard"
		/>
		<q-btn
			color="primary"
			label="Primary"
		/>
		<q-btn
			color="secondary"
			label="Secondary"
		/>
		<q-btn
			color="amber"
			glossy
			label="Amber"
		/>
		<q-btn
			color="brown-5"
			label="Brown 5"
		/>
		<q-btn
			color="deep-orange"
			glossy
			label="Deep Orange"
		/>
		<q-btn
			color="purple"
			label="Purple"
		/>
		<q-btn
			color="black"
			label="Black"
		/>
	</QRow>
	<q-markup-table>
		<tbody>
			<tr>
				<td>BaseButton</td>
				<td>
					<BaseButton />
				</td>
				<td>AddIconButton</td>
				<td>
					<AddIconButton />
				</td>
			</tr>
			<tr>
				<td>AccountValidationButton</td>
				<td>
					<AccountValidationButton />
				</td>
				<td>CancelButton</td>
				<td>
					<CancelButton />
				</td>
			</tr>
			<tr>
				<td>CheckConnectionButton</td>
				<td>
					<CheckConnectionButton
						:loading="checkConnectionButton"
						@click="checkConnectionButton = !checkConnectionButton"
					/>
					{{ checkConnectionButton }}
				</td>
				<td>SaveButton</td>
				<td>
					<SaveButton />
				</td>
			</tr>
			<tr>
				<td>DebugButton</td>
				<td>
					<DebugButton />
				</td>
				<td>DeleteButton</td>
				<td>
					<DeleteButton />
				</td>
			</tr>

			<tr>
				<td>ExternalLinkButton</td>
				<td>
					<ExternalLinkButton />
				</td>
				<td>NavigationNextButton</td>
				<td>
					<NavigationNextButton />
				</td>
			</tr>
			<tr>
				<td>GoToButton</td>
				<td>
					<GoToButton />
				</td>
				<td>NavigationPreviousButton</td>
				<td>
					<NavigationPreviousButton />
				</td>
			</tr>
			<tr>
				<td>HelpButton</td>
				<td>
					<HelpButton />
				</td>
				<td>NavigationSkipSetupButton</td>
				<td>
					<NavigationSkipSetupButton />
				</td>
			</tr>
			<tr>
				<td>HideButton</td>
				<td>
					<HideButton />
				</td>
				<td>ResetButton</td>
				<td>
					<ResetButton />
				</td>
			</tr>
			<tr>
				<td>ConfirmButton</td>
				<td>
					<ConfirmButton />
				</td>
				<td>WarningButton</td>
				<td>
					<WarningButton text-id="reset-db" />
				</td>
			</tr>
			<tr>
				<td>NavigationFinishSetupButton</td>
				<td>
					<NavigationFinishSetupButton />
				</td>
				<td>DeleteIconButton</td>
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
