<template>
	<v-dialog :value="dialog" persistent :max-width="500">
		<v-card :max-width="600">
			<v-card-title>{{ $t('components.account-verification-code-dialog.title') }}</v-card-title>
			<v-card-subtitle>{{ $t('components.account-verification-code-dialog.sub-title') }}</v-card-subtitle>
			<v-card-text>
				<!--	Verification Code input	-->
				<v-row justify="center">
					<v-col cols="auto">
						<CodeInput :loading="false" class="input" @change="onChange" @complete="onComplete" @keyup.enter="onEnter" />
					</v-col>
				</v-row>
				<v-row v-if="errors.length > 0" justify="center">
					<v-col cols="auto">
						<span style="color: red; font-weight: bold">{{ $t('components.account-verification-code-dialog.error') }}</span>
					</v-col>
				</v-row>
			</v-card-text>
			<v-card-actions>
				<!--	Submit button	-->
				<v-row justify="center">
					<v-col cols="auto">
						<p-btn :button-type="getCancelButton" @click="closeDialog" />
					</v-col>
					<v-spacer />
					<v-col cols="auto">
						<p-btn :button-type="getSubmitButton" :disabled="code.length < 6" @click="submitCode" />
					</v-col>
				</v-row>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue } from 'vue-property-decorator';
import CodeInput from 'vue-verification-code-input';
import ButtonType from '@enums/buttonType';
import { Error } from '@dto/mainApi';
import { map } from 'rxjs/operators';

@Component<AccountVerificationCodeDialog>({ components: { CodeInput } })
export default class AccountVerificationCodeDialog extends Vue {
	@Prop({ required: false, type: Boolean, default: false })
	readonly dialog!: boolean;

	@Prop({ required: true, type: Array as () => Error[] })
	readonly errors!: Error[];

	code: string = '0';

	get getCancelButton(): ButtonType {
		return ButtonType.Cancel;
	}

	get getSubmitButton(): ButtonType {
		return ButtonType.Confirm;
	}

	onChange(v) {
		this.code = v;
	}

	onComplete(v) {
		Log.info('onComplete ', v);
	}

	closeDialog() {
		this.$emit('close');
	}

	submitCode() {
		this.$emit('submit', this.code);
	}

	onEnter() {
		Log.info('Enter pressed');
	}

	mounted() {
		// If an error appears, then clear the code
		this.$subscribeTo(this.$watchAsObservable('errors').pipe(map((x) => x.newValue)), (value) => {
			if (value.length > 0) {
				this.code = '0';
			}
		});
	}
}
</script>
