<template>
	<v-container>
		<!-- Plex Accounts -->
		<v-row v-for="(account, index) in accounts" :key="index">
			<v-col cols="12" sm="6">
				<v-text-field label="Username" full-width single-line outlined :value="account.username" />
			</v-col>

			<v-col cols="12" sm="6">
				<v-text-field label="Password" full-width single-line outlined :value="account.password" />
			</v-col>
		</v-row>
		<v-row>
			<v-col>
				<v-btn block color="primary">Add Plex Account</v-btn>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Log from 'consola';

interface IPlexAccounts {
	username: string;
	password: string;
	isConfifmed: boolean;
}

@Component
export default class Settings extends Vue {
	accounts: IPlexAccounts[] = [
		{
			username: 'test',
			password: 'password123',
			isConfifmed: false,
		},
	];


	checkAccount(account: IPlexAccounts){

	}

	async mounted(): Promise<void> {
		try {
			const ip = await this.$axios.$get('/weatherforecast');
			Log.debug(ip);
		} catch (error) {
			Log.error(error);
		}
	}
}
</script>
