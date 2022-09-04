<template>
	<p-section>
		<template #header> {{ $t('pages.settings.debug.header') }}</template>
		<!--	Reset Database	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<p-btn button-type="debugButtonType" text-id="add-alert" @click="addAlert" />
			</v-col>
			<v-col cols="8" align-self="center"></v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import ButtonType from '@enums/buttonType';
import { resetDatabase } from '@api/settingsApi';
import { AlertService } from '@service';

@Component
export default class DebugSection extends Vue {
	debugButtonType: ButtonType = ButtonType.Info;

	addAlert(): void {
		AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
		AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
	}

	// TODO Fix the reset button for the database
	resetDatabaseCommand(): void {
		useSubscription(
			resetDatabase().subscribe(() => {
				this.$router.push('/setup');
			}),
		);
	}
}
</script>
