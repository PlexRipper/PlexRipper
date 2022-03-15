<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.database.header') }} </template>
		<!--	Reset Database	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.reset-db" />
			</v-col>
			<!--	Reset Database button	-->
			<v-col cols="8" align-self="center">
				<p-btn :width="400" :button-type="dbResetButtonType" text-id="reset-db" @click="confirmationDialog = true" />
				<confirmation-dialog
					text-id="reset-db"
					:dialog="confirmationDialog"
					@confirm="resetDatabaseCommand"
					@cancel="confirmationDialog = false"
				/>
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import ButtonType from '@enums/buttonType';
import { resetDatabase } from '@api/settingsApi';
import Log from 'consola';
import { GlobalService } from '@service';

@Component
export default class DatabaseSection extends Vue {
	dbResetButtonType: ButtonType = ButtonType.Warning;
	confirmationDialog: boolean = false;

	resetDatabaseCommand(): void {
		this.$subscribeTo(resetDatabase(), (value) => {
			GlobalService.resetStore();
			Log.debug('reset db', value);
			if (value.isSuccess) {
				this.$router.push('/setup');
			}
		});
	}
}
</script>
