<script>
import Vue from 'vue';
import { VSelect } from 'vuetify/lib';
const base = Vue.extend({ mixins: [VSelect] });

export default base.extend({
	name: 'p-select',
	props: {
		dense: {
			type: Boolean,
			default: true,
		},
		filled: {
			type: Boolean,
			default: true,
		},
		outlined: {
			type: Boolean,
			default: true,
		},
		color: {
			type: String,
			default: 'red',
		},
		hideDetails: {
			type: String,
			default: 'auto',
		},
		menuProps: {
			type: Object,
			default: () => {
				return { offsetY: true, contentClass: 'menu-background' };
			},
		},
	},
});
</script>
