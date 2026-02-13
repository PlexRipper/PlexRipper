<template>
	<div class="alphabet-navigation-container">
		<div class="alphabet-navigation">
			<q-btn
				v-for="value in mediaOverviewStore.scrollAlphabet"
				:key="value"
				class="navigation-btn"
				:label="value"
				flat
				square
				no-wrap
				:data-cy="`letter-${value}-alphabet-navigation-btn`"
				@click="sendMediaOverviewScrollToCommand(value)" />
		</div>
	</div>
</template>

<script setup lang="ts">
import { sendMediaOverviewScrollToCommand } from '@composables/event-bus';

const mediaOverviewStore = useMediaOverviewStore();
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';
@use '@/assets/scss/variables' as *;

.alphabet-navigation-container {
  display: flex;
  align-content: stretch;
  align-items: stretch;
  align-self: stretch;
  justify-content: center;
  flex: 0 0 30px;
  max-height: $page-height-minus-app-bar-minus-media-overview-bar;

  .alphabet-navigation {
    display: flex;
    justify-content: space-around;
    flex: 0 0 100%;
    flex-direction: column;
    overflow-y: auto;
    scrollbar-width: none;

    &::-webkit-scrollbar {
      display: none;
    }

    .navigation-btn {
      @extend .fade-out-border;
      flex: 1 1 25px;
      text-align: center;
      font-weight: bold;
      background: transparent !important;

      &:hover {
        &::before {
          opacity: 0.2 !important;
        }
      }
    }
  }
}

body {
  &.body--dark {
    .navigation-btn {
      color: red;
    }
  }

  &.body--light {
    .navigation-btn {
      color: darkred;
    }
  }
}
</style>
