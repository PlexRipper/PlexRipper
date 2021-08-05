<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.database.header') }} </template>
		<!--	Reset Database	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.reset-db" />
			</v-col>
			<v-col cols="8" align-self="center">
				<confirmation-dialog
					:button-type="dbResetButtonType"
					button-text-id="reset-db"
					text-id="reset-db"
					:width="200"
					@confirm="resetDatabaseCommand"
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
