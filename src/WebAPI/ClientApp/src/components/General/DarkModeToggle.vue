<template>
	<v-btn class="mx-2" icon @click="setDarkMode">
		<v-icon v-if="$vuetify.theme.dark">mdi-white-balance-sunny</v-icon>
		<v-icon v-else>mdi-moon-waxing-crescent</v-icon>
	</v-btn>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class DarkModeToggle extends Vue {
	setDarkMode(): void {
		this.$vuetify.theme.dark = !this.$vuetify.theme.dark;
	}
}
</script>
