import Vue from 'vue';
import Log, { LogLevel } from 'consola';
import { Context } from '@nuxt/types';
import { GlobalService } from '@service';
import IAppConfig from '@class/IAppConfig';

export default (ctx: Context): void => {
	// Setup Config
	Log.info(`Nuxt Environment: ${ctx.$config.nodeEnv}`);
	GlobalService.setupServices(ctx);
};

export function setLogConfig(config: IAppConfig) {
	// Setup logging
	Vue.config.devtools = true;
	Vue.config.productionTip = false;
	Log.level = config.isProduction ? LogLevel.Debug : LogLevel.Debug;
}
