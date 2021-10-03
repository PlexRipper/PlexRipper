<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.setup.header') }} </template>
		<!--	Reset Database	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.setup-section.go-to-setup-page" />
			</v-col>
			<v-col cols="8" align-self="center">
				<p-btn :button-type="goToSetupPageBtn" :width="400" text-id="go-to-setup-page" @click="goToSetupPage" />
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import ButtonType from '@enums/buttonType';

@Component
export default class SetupSection extends Vue {
	goToSetupPageBtn: ButtonType = ButtonType.Info;

	goToSetupPage(): void {
		this.$router.push('/setup');
	}
}
</script>
