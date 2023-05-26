// This can be directly added to any of your `.ts` files like `router.ts`
// It can also be added to a `.d.ts` file, in which case you will need to add an export
// to ensure it is treated as a module
import 'vue-router';

export {};

declare module 'vue-router' {
	interface RouteMeta {
		// is optional
		scrollPos?: {
			top: number;
			left: number;
		};
	}
}
