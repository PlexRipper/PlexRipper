<template>
	<v-container fluid :class="['page', $getThemeClass(), 'fill-height']">
		<slot></slot>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Page extends Vue {}
</script>
