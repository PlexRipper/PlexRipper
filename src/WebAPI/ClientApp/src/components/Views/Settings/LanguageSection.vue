<template>
	<QSection>
		<template #header>
			{{ $t('pages.settings.ui.language.header') }}
		</template>
		<HelpRow
			:label="$t('help.settings.ui.language.language-selection.label')"
			:title="$t('help.settings.ui.language.language-selection.title')"
			:text="$t('help.settings.ui.language.language-selection.text')">
			<LanguageSelect />
		</HelpRow>
	</QSection>
</template>
