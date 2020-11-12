<template>
	<v-app>
		<help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false" />

		<v-main>
			<nuxt />
		</v-main>
		<footer />
	</v-app>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import HelpDialog from '@components/Help/HelpDialog.vue';
import Footer from '@components/Footer/Footer.vue';
import HelpService from '@service/helpService';

@Component({
	components: {
		HelpDialog,
		Footer,
	},
})
export default class Empty extends Vue {
	helpDialogState: boolean = false;
	helpId: string = '';

	created(): void {
		HelpService.getHelpDialog().subscribe((helpId) => {
			this.helpId = helpId;
			this.helpDialogState = true;
		});
	}
}
</script>
