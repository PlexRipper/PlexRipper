<template>
	<v-tooltip top :disabled="valid">
		<template #activator="{ on }">
			<v-icon class="valid-icon" :size="size" :color="valid ? 'green' : 'red'" dark v-on="on">
				{{ getIcon }}
			</v-icon>
		</template>
		<span>{{ text }}</span>
	</v-tooltip>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class ValidIcon extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly valid!: boolean;

	@Prop({ required: true, type: String })
	readonly text!: string;

	@Prop({ required: false, type: Number, default: 30 })
	readonly size!: number;

	get getIcon(): string {
		return this.valid ? 'mdi-check-circle-outline' : 'mdi-alert-circle';
	}
}
</script>
<style lang="scss">
.valid-icon {
	margin: 10px;
}
</style>
