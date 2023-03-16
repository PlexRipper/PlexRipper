import Log from 'consola';
import {GlobalService} from "@service";

export default defineNuxtPlugin(() => {
    Log.info('Setup script ran')

    GlobalService.setupServices();
})