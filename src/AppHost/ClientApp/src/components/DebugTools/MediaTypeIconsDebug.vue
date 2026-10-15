<template>
	<QSection
		header="Media Type Icons"
		data-cy="media-type-icons-debug">
		<QRow class="media-type-icons-debug__grid q-col-gutter-md q-row-gutter-md">
			<QCol
				v-for="item in mediaTypes"
				:key="item.mediaType"
				cols="12"
				sm="6"
				md="4"
				lg="3">
				<q-card
					flat
					bordered
					class="media-type-icons-debug__card"
					:data-cy="`media-type-icon-${item.mediaType}`">
					<q-card-section class="media-type-icons-debug__card-content">
						<QMediaTypeIcon
							:media-type="item.mediaType"
							:size="40"
							aria-hidden="true" />
						<div>
							<div class="media-type-icons-debug__label">
								{{ item.mediaType }}
							</div>
							<code>{{ item.icon }}</code>
						</div>
					</q-card-section>
				</q-card>
			</QCol>
		</QRow>
	</QSection>
</template>

<script setup lang="ts">
import Convert from '@class/Convert';
import { PlexMediaType } from '@dto';

const mediaTypes = Object.values(PlexMediaType).map((mediaType) => ({
	mediaType,
	icon: Convert.mediaTypeToIcon(mediaType),
}));
</script>

<style scoped lang="scss">
.media-type-icons-debug__card {
	height: 100%;
}

.media-type-icons-debug__card-content {
	display: flex;
	align-items: center;
	gap: 1rem;
	min-height: 100px;
}

.media-type-icons-debug__label {
	font-weight: 600;
	word-break: break-word;
}
</style>
