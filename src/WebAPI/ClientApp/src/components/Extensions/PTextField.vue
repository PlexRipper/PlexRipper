<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import { VTextField } from 'vuetify/lib';

@Component
export default class PTextField extends VTextField {
	@Prop({ required: false, type: Number, default: 50 })
	readonly height!: number;

	@Prop({ required: false, type: Boolean, default: true })
	readonly dense!: boolean;

	@Prop({ required: false, type: Boolean, default: true })
	readonly outlined!: boolean;

	@Prop({ required: false, type: Boolean, default: true })
	readonly solo!: boolean;
}
</script>
