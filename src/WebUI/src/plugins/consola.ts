import Vue from 'vue';
import log from 'consola';

export default (): void => {
	Vue.config.productionTip = false;
	Vue.config.devtools = false;

	const isProduction = process.env.NODE_ENV === 'production';

	log.level = isProduction ? 1 : 5;
};
