<template>
	<v-progress-circular indeterminate color="red"></v-progress-circular>
</template>

<script>
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class LoadingSpinner extends Vue {}
</script>
