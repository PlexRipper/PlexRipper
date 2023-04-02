<script lang="ts">
import { h, defineComponent, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'CancelButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(BaseButton, {
			...mergeProps(this.$props, {
				icon: 'mdi-alert',
				textId: 'cancel',
			}),
		} as Partial<IBaseButtonProps>);
	},
});
</script>
