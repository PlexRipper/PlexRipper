<template>
	<v-dialog v-model="show" max-width="800" @click:outside="close">
		<v-card>
			<v-card-title class="headline i18n-formatting">{{ alert.title }}</v-card-title>

			<v-card-text class="i18n-formatting">
				<p>{{ alert.text }}</p>
				<pre style="white-space: break-spaces">{{ errors }}</pre>
			</v-card-text>

			<!--	Close action	-->
			<v-card-actions>
				<v-spacer></v-spacer>
				<v-btn color="green darken-1" text @click="close"> {{ $t('general.commands.close') }}</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import type IAlert from '@interfaces/IAlert';
import { IError } from '@dto/mainApi';

@Component
export default class AlertDialog extends Vue {
	show: boolean = true;

	@Prop({ required: true, type: Object as () => IAlert })
	readonly alert!: IAlert;

	get errors(): IError[] {
		if (this.alert?.result?.errors) {
			return this.alert.result.errors;
		}
		return [];
	}

	close(): void {
		this.show = false;
		this.$emit('close', this.alert);
	}
}
</script>
