<template>
	<v-row no-gutters style="width: 100%">
		<v-col cols="12">
			<!-- Header	-->
			<v-row no-gutters>
				<v-col>
					<h1>
						<slot name="header"></slot>
					</h1>
					<v-divider />
				</v-col>
			</v-row>
			<!--	Section Content	-->
			<slot></slot>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class PSection extends Vue {}
</script>
