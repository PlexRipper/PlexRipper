<template>
	<v-dialog :value="show" max-width="500" @click:outside="close">
		<v-card>
			<v-card-title class="headline">{{ getHelpText.title }}</v-card-title>

			<v-card-text>{{ getHelpText.text }} </v-card-text>

			<!--	Close action	-->
			<v-card-actions>
				<v-spacer></v-spacer>
				<v-btn color="green darken-1" text @click="close"> Close </v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

interface IHelpText {
	id: string;
	title: string;
	text: string;
}

@Component
export default class HelpDialog extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly show!: boolean;

	@Prop({ required: true, type: String })
	readonly id!: string;

	helpText: IHelpText[] = [];

	close(): void {
		this.$emit('close');
	}

	get getHelpText(): IHelpText {
		return (
			this.helpText.find((x) => x.id === this.id) ?? { id: 'null', title: 'Could not find the correct help page', text: '?' }
		);
	}

	created(): void {
		this.helpText = [
			{
				id: 'advanced-1',
				title: 'Download Segments',
				text:
					'PlexRipper supports multi-threaded downloading and can divide the media that will be downloaded into different parts and download each part individually. This tends to increase the overall download speed but the downside is that it will require extra time to merge the different parts afterwards into one complete media file. Setting this value to 1 will disable multi-threaded downloading and download the media file as one complete file. The number of download segments has no influence on the speed of the media file being merged.',
			},
		];
	}
}
</script>
