<template>
	<v-dialog v-model="dialog" persistent max-width="290">
		<template v-slot:activator="{ on }">
			<v-card v-on="on" hover>
				<v-card-title class="headline">{{ account.username }}</v-card-title>
				<v-card-text>
					<template>
						<v-chip v-if="account.isConfirmed" class="ma-2" color="green" text-color="white">
							Validated
						</v-chip>
						<v-chip v-else class="ma-2" color="red" text-color="white">
							NotValidated
						</v-chip>
					</template>

					<v-chip class="ma-2" color="green" text-color="white">
						Green Chip
					</v-chip>
				</v-card-text>
			</v-card>
		</template>
		<v-card>
			<v-card-title class="headline">Use Google's location service?</v-card-title>
			<v-card-text>
				<v-form ref="form" v-model="valid" lazy-validation>
					<!-- Username -->
					<v-text-field
						v-model="username"
						:rules="getUsernameRules"
						label="Username"
						full-width
						single-line
						outlined
						:value="account.username"
						required
					/>

					<!-- Password -->
					<v-text-field
						v-model="password"
						:rules="getPasswordRules"
						label="Password"
						full-width
						single-line
						outlined
						:value="account.password"
						required
					/>

					<v-select v-model="select" :items="items" :rules="[(v) => !!v || 'Item is required']" label="Item" required />

					<v-checkbox v-model="checkbox" :rules="[(v) => !!v || 'You must agree to continue!']" label="Do you agree?" required />

					<v-btn :disabled="!valid" color="success" class="mr-4" @click="validate">
						Validate
					</v-btn>

					<v-btn color="error" class="mr-4" @click="reset">
						Reset Form
					</v-btn>

					<v-btn icon>
						<v-icon>mdi-delete</v-icon>
					</v-btn>
				</v-form>
			</v-card-text>
			<v-card-actions>
				<v-spacer />
				<v-btn color="green darken-1" text @click="dialog = false">Disagree</v-btn>
				<v-btn color="green darken-1" text @click="dialog = false">Agree</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
	<!-- Plex Accounts -->
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import Log from 'consola';

interface IAccount {
	username: string;
	password: string;
	isConfirmed: boolean;
}

@Component
export default class AccountCard extends Vue {
	@Prop({ required: true, type: Object as () => IAccount })
	readonly account!: IAccount;

	dialog: boolean = false;

	username!: string;

	password!: string;

	get getUsernameRules(): any {
		return [(v) => !!v || 'Name is required', (v) => (v && v.length <= 10) || 'Name must be less than 10 characters'];
	}

	get getPasswordRules(): any {
		return [(v) => !!v || 'Name is required', (v) => (v && v.length <= 10) || 'Name must be less than 10 characters'];
	}

	checkAccount(account: IAccount): void {
		Log.debug(account);
	}
}
</script>
