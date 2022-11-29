<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.database.header') }}</template>
		<!--	Reset Database	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.reset-db" />
			</v-col>
			<!--	Reset Database button	-->
			<v-col cols="8" align-self="center">
				<WarningButton :width="400" text-id="reset-db" @click="confirmationDialog = true" />
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
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { resetDatabase } from '@api/settingsApi';
import { GlobalService } from '@service';

@Component
export default class DatabaseSection extends Vue {
	confirmationDialog: boolean = false;

	resetDatabaseCommand(): void {
		useSubscription(
			resetDatabase().subscribe((value) => {
				GlobalService.resetStore();
				Log.debug('reset db', value);
				if (value.isSuccess) {
					this.$router.push('/setup');
				}
			}),
		);
	}
}
</script>
