<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'NavigationFinishSetupButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					textId: 'finish-setup',
					icon: 'mdi-check-circle-outline',
					iconAlign: 'right',
					color: 'positive',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
