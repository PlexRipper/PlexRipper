<template>
	<q-markup-table dark>
		<tbody>
			<tr>
				<td :style="{ width: `${headerWidth}%` }">
					<HelpIcon
						:label="label"
						:title="title"
						:text="text"
					/>
				</td>
				<td>
					<slot />
				</td>
			</tr>
		</tbody>
	</q-markup-table>
</template>

<script setup lang="ts">
import type { IHelp } from '@interfaces';

withDefaults(defineProps<IHelp & { headerWidth?: number }>(), {
	headerWidth: 30,
});
</script>
