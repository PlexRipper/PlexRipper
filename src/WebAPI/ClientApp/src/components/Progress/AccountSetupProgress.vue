<template>
	<!-- The setup account progress -->
	<v-card>
		<v-card-text>
			<template v-if="accountSetupProgress">
				<print :object="accountSetupProgress" />
				<progress-component :percentage="accountSetupProgress.percentage" :text="`Retrieving accessible servers 1 of 8`" />
			</template>
			<v-simple-table class="section-table">
				<tbody>
					<tr v-for="server in plexServers" :key="server.id">
						<td style="width: 25%">{{ server.name }}</td>
						<td>{{ server.serverUrl }}</td>
					</tr>
				</tbody>
			</v-simple-table>
		</v-card-text>
		<v-card-actions>
			<p-btn :button-type="hideButtonType" />
			<p-btn :button-type="confirmButtonType" @click="refreshAccount(1)" />
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { AccountService, ProgressService } from '@service';
import { PlexAccountRefreshProgress, PlexServerDTO } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import { refreshAccount } from '@api/accountApi';
import { switchMap } from 'rxjs';
import { inspectPlexServers } from '@api/plexServerApi';
import { tap } from 'rxjs/operators';

@Component
export default class AccountSetupProgress extends Vue {
	accountSetupProgress: PlexAccountRefreshProgress | null = null;

	hideButtonType: ButtonType = ButtonType.Hide;
	confirmButtonType: ButtonType = ButtonType.Confirm;
	plexServers: PlexServerDTO[] = [];

	refreshAccount(accountId: number = 0): void {
		this.$subscribeTo(ProgressService.getPlexAccountRefreshProgress(), (progress) => {
			if (progress) {
				this.accountSetupProgress = progress;
			}
		});

		this.$subscribeTo(
			refreshAccount(accountId).pipe(
				// Get account with accessible plexServers
				switchMap(() => AccountService.fetchAccount(accountId)),
				// Save plexServers
				tap((account) => {
					if (account) {
						this.plexServers = account.plexServers;
					}
				}),
				// Check status and connectivity of all plexServers
				switchMap(() =>
					inspectPlexServers(
						accountId,
						this.plexServers.map((x) => x.id),
					),
				),
			),
			() => {},
		);
	}

	created(): void {}
}
</script>
