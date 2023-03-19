<template>
	<div>
		<q-card>
			<q-card-section>
				<h1 class="custom-class-test">Text</h1>
			</q-card-section>
		</q-card>
		<div class="q-pa-md q-gutter-sm">
			<q-btn color="white" text-color="black" label="Standard" />
			<q-btn color="primary" label="Primary" />
			<q-btn color="secondary" label="Secondary" />
			<q-btn color="amber" glossy label="Amber" />
			<q-btn color="brown-5" label="Brown 5" />
			<q-btn color="deep-orange" glossy label="Deep Orange" />
			<q-btn color="purple" label="Purple" />
			<q-btn color="black" label="Black" />
		</div>
		<div style="padding: 100px">
			<ButtonsDebug />
		</div>
	</div>
</template>

<script setup lang="ts"></script>
