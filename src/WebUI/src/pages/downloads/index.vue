<template>
	<p>Downloads</p>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class Downloads extends Vue {}
</script>
