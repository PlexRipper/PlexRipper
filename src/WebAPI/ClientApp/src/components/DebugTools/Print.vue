<template>
	<v-expansion-panels>
		<v-expansion-panel>
			<v-expansion-panel-header> {{ title }} </v-expansion-panel-header>
			<v-expansion-panel-content :style="{ height: object ? height + 'px' : '0px' }">
				<perfect-scrollbar>
					<pre>{{ object }}</pre>
				</perfect-scrollbar>
			</v-expansion-panel-content>
		</v-expansion-panel>
	</v-expansion-panels>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class Print extends Vue {
	@Prop({ required: false, type: String, default: 'Print' })
	readonly title!: string;

	@Prop({ required: true })
	readonly object!: any;

	@Prop({ required: false, type: Number, default: 500 })
	readonly height!: number;
}
</script>
