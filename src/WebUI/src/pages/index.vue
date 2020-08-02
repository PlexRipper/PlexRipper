<template>
	<v-row>
		<v-col>
			<h1>WORKING!</h1>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { getAccount } from '@api/accountApi';
import Log from 'consola';

@Component
export default class Home extends Vue {
	created() {
		getAccount(2).subscribe((data) => {
			Log.debug('From Home Test:', data);
		});
	}
}
</script>
