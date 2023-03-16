// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
    devServer: {
        port: 3001,
    },
    modules: [
        'nuxt-quasar-ui'
    ],
    quasar: {
        // Plugins: https://quasar.dev/quasar-plugins
        plugins: [],
        // Truthy values requires `sass@1.32.12`.
        sassVariables: false,
        // Requires `@quasar/extras` package
        extras: {
            // string | null: Auto-import roboto font. https://quasar.dev/style/typography#default-font
            font: null,
            // string[]: Auto-import webfont icons. Usage: https://quasar.dev/vue-components/icon#webfont-usage
            fontIcons: [],
            // string[]: Auto-import svg icon collections. Usage: https://quasar.dev/vue-components/icon#svg-usage
            svgIcons: [],
            // string[]: Auto-import animations from 'animate.css'. Usage: https://quasar.dev/options/animations#usage
            animations: [],
        }
    },
    typescript: {
        // Doc: https://typescript.nuxtjs.org/guide/setup.html#configuration
        // Packages,  @types/node, vue-tsc and typescript are required
        typeCheck: true,
    }
})
