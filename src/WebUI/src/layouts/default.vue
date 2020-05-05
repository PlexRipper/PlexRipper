<template>
	<v-app>
		<v-content>
			<v-container fluid>
				<v-row>
					<v-col cols="3">
						<navigation-drawer />
					</v-col>
					<v-col>
						<nuxt />
					</v-col>
				</v-row>
			</v-container>
		</v-content>
		<v-footer app>
			<span>&copy; {{ currentYear }} </span>
		</v-footer>
	</v-app>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import NavigationDrawer from '@/components/NavigationDrawer.vue';
@Component({
	components: {
		NavigationDrawer,
	},
})
export default class Default extends Vue {
	get currentYear(): number {
		return new Date().getFullYear();
	}

	created(): void {
		this.$vuetify.theme.dark = true;
	}
}
</script>

<style lang="scss">
@import '@/assets/scss/style.scss';
</style>
