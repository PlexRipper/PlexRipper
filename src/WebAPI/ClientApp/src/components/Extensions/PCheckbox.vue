<script>
import VCheckbox from 'vuetify/lib/components/VCheckbox/VCheckbox';

export default {
	name: 'PCheckbox',
	extends: VCheckbox,
	props: {
		color: {
			type: String,
			default: 'red',
		},
		hideDetails: {
			type: Boolean,
			default: true,
		},
	},
};
</script>
