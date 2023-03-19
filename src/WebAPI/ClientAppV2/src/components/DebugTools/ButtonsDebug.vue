<template>
	<q-row align="center" justify="around">
		<q-col cols="6">
			<AccountValidationButton/>
		</q-col>
		<q-col cols="6">
			<AddIconButton/>
		</q-col>
		<q-col cols="6">
			<BaseButton/>
		</q-col>
		<q-col cols="6">
			<CancelButton/>
		</q-col>
		<q-col cols="6">
			<CheckConnectionButton :loading="checkConnectionButton" @click="checkConnectionButton = !checkConnectionButton"/>
			{{ checkConnectionButton }}
		</q-col>
		<q-col cols="6">
			<ConfirmButton/>
		</q-col>
		<q-col cols="6">
			<DebugButton/>
		</q-col>
		<q-col cols="6">
			<DeleteButton/>
		</q-col>
		<q-col cols="6">
			<DeleteIconButton/>
		</q-col>
		<q-col cols="6">
			<EditIconButton/>
		</q-col>
		<q-col cols="6">
			<ExternalLinkButton/>
		</q-col>
		<q-col cols="6">
			<GoToButton/>
		</q-col>
		<q-col cols="6">
			<HelpButton/>
		</q-col>
		<q-col cols="6">
			<HideButton/>
		</q-col>
		<q-col cols="6">
			<NavigationFinishSetupButton/>
		</q-col>
		<q-col cols="6">
			<NavigationNextButton/>
		</q-col>
		<q-col cols="6">
			<NavigationPreviousButton/>
		</q-col>
		<q-col cols="6">
			<NavigationSkipSetupButton/>
		</q-col>
		<q-col cols="6">
			<ResetButton/>
		</q-col>
		<q-col cols="6">
			<SaveButton/>
		</q-col>
		<q-col cols="6">
			<SaveIconButton/>
		</q-col>
		<q-col cols="6">
			<WarningButton/>
		</q-col>
	</q-row>

</template>

<script setup lang="ts">
import Log from 'consola';
import {ref} from 'vue';

const checkConnectionButton = ref<boolean>(false);

</script>
