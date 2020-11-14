<template>
	<v-container fluid :class="['page', $getThemeClass()]">
		<slot></slot>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Page extends Vue {}
</script>
