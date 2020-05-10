import { Module, VuexModule } from 'vuex-module-decorators';

// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
@Module({ name: 'navigationStore', namespaced: true, stateFactory: true })
export default class NavigationStore extends VuexModule {}
