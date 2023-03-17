import Log from "consola";
import {Composer, VueI18n, LocaleMessageObject, LocaleMessageValue} from 'vue-i18n';
import {defineNuxtPlugin} from '#app'
import objectPath from "object-path";

export default defineNuxtPlugin(nuxtApp => {
    // Doc: https://i18n.nuxtjs.org/

    const ctx = nuxtApp.$i18n as VueI18n | Composer;

    function messages(ctx: VueI18n | Composer) {
        return ctx.messages[ctx.locale.toString()];
    }

    return {
        provide: {
            'messages': (): LocaleMessageObject => messages(ctx),
            //'ts': (path: string, values?: MessageType[]): string => ctx.app.i18n.t(path, values).toString(),
            'getMessage': (path: string): any => objectPath.get(messages(ctx), path)
        }
    }
})


