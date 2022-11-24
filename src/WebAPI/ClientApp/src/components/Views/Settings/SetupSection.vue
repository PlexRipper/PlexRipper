<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.setup.header') }}</template>
		<!--	Reset Database	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.setup-section.go-to-setup-page" />
			</v-col>
			<v-col cols="8" align-self="center">
				<GoToButton :width="400" text-id="go-to-setup-page" to="/setup" />
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class SetupSection extends Vue {}
</script>
