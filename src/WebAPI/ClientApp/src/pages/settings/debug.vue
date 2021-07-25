<template>
	<page>
		<debug-section />
	</page>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';

import DebugSection from '~/pages/settings/components/DebugSection.vue';

@Component({
	components: {
		DebugSection,
	},
})
export default class DebugSettings extends Vue {}
</script>
