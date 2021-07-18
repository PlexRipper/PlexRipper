<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import { VCheckbox } from 'vuetify/lib';

@Component
export default class PCheckbox extends VCheckbox {
	@Prop({ required: false, type: String, default: 'red' })
	readonly color!: string;

	@Prop({ required: false, type: Boolean, default: true })
	readonly hideDetails!: boolean;
}
</script>
