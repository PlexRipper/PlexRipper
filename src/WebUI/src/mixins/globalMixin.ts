import { Component, Vue } from 'nuxt-property-decorator';
import VueI18n from 'vue-i18n';
@Component
class GlobalMixin extends Vue {
	public $messages(): VueI18n.LocaleMessageObject {
		return ctx.app.i18n.messages[ctx.app.i18n.locale];
	}
}
export default CartMixin;
