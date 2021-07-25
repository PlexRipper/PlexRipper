<script lang="ts">
import Vue, { VNode, CreateElement } from 'vue';
import { RenderContext } from 'vue/types/options';
import Convert from '@mediaOverview/MediaTable/types/Convert';

export default Vue.extend({
	functional: true,
	props: {
		mediaType: {
			type: String,
			default: '',
		},
		size: {
			type: Number,
			default: 24,
		},
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(
			'v-icon',
			{
				class: {
					...(context.data.staticClass && {
						[context.data.staticClass]: true,
					}),
				},
				props: { size: context.props.size },
			},
			Convert.mediaTypeToIcon(context.props.mediaType),
		);
	},
});
</script>
