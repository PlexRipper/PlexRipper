<template>
	<v-app dark class="no-background">
		<v-container>
			<v-row justify="center">
				<v-col cols="auto">
					<h1 class="text-left">
						<pre><code class="no-background">{{ errorFormatted }}</code></pre>
					</h1>
				</v-col>
			</v-row>
			<v-row justify="center">
				<v-col cols="auto">
					<h1>
						<NuxtLink to="/">Go back to home page </NuxtLink>
					</h1>
				</v-col>
			</v-row>
		</v-container>
	</v-app>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class ErrorLayout extends Vue {
	@Prop()
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	error!: any;

	get errorFormatted(): string {
		return JSON.stringify(this.error, null, 4);
	}

	layout(): string {
		return 'error';
	}
}
</script>
