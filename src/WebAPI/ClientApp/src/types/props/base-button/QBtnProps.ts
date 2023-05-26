// TODO We can't import interfaces and construct prop definitions from them so we have to manually define them here
// See https://github.com/vuejs/core/issues/4294
export interface QBtnProps {
	/**
	 * Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
	 */
	size?: string | undefined;
	/**
	 * 1) Define the button native type attribute (submit, reset, button) or 2) render component with <a> tag so you can access events even if disable or 3) Use 'href' prop and specify 'type' as a media tag
	 * Default value: button
	 */
	type?: string | undefined;
	/**
	 * Equivalent to Vue Router <router-link> 'to' property; Superseded by 'href' prop if used
	 */
	to?: string | any | undefined;
	/**
	 * Equivalent to Vue Router <router-link> 'replace' property; Superseded by 'href' prop if used
	 */
	replace?: boolean | undefined;
	/**
	 * Native <a> link href attribute; Has priority over the 'to' and 'replace' props
	 */
	href?: string | undefined;
	/**
	 * Native <a> link target attribute; Use it only with 'to' or 'href' props
	 */
	target?: string | undefined;
	/**
	 * The text that will be shown on the button
	 */
	label?: string | number | undefined;
	/**
	 * Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
	 */
	icon?: string | undefined;
	/**
	 * Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
	 */
	iconRight?: string | undefined;
	/**
	 * Use 'outline' design
	 */
	outline?: boolean | undefined;
	/**
	 * Use 'flat' design
	 */
	flat?: boolean | undefined;
	/**
	 * Remove shadow
	 */
	unelevated?: boolean | undefined;
	/**
	 * Applies a more prominent border-radius for a squared shape button
	 */
	rounded?: boolean | undefined;
	/**
	 * Use 'push' design
	 */
	push?: boolean | undefined;
	/**
	 * Removes border-radius so borders are squared
	 */
	square?: boolean | undefined;
	/**
	 * Applies a glossy effect
	 */
	glossy?: boolean | undefined;
	/**
	 * Makes button size and shape to fit a Floating Action Button
	 */
	fab?: boolean | undefined;
	/**
	 * Makes button size and shape to fit a small Floating Action Button
	 */
	fabMini?: boolean | undefined;
	/**
	 * Apply custom padding (vertical [horizontal]); Size in CSS units, including unit name or standard size name (none|xs|sm|md|lg|xl); Also removes the min width and height when set
	 */
	padding?: string | undefined;
	/**
	 * Color name for component from the Quasar Color Palette
	 */
	color?: string | undefined;
	/**
	 * Overrides text color (if needed); Color name from the Quasar Color Palette
	 */
	textColor?: string | undefined;
	/**
	 * Avoid turning label text into caps (which happens by default)
	 */
	noCaps?: boolean | undefined;
	/**
	 * Avoid label text wrapping
	 */
	noWrap?: boolean | undefined;
	/**
	 * Dense mode; occupies less space
	 */
	dense?: boolean | undefined;
	/**
	 * Configure material ripple (disable it by setting it to 'false' or supply a config object)
	 * Default value: true
	 */
	ripple?: boolean | any | undefined;
	/**
	 * Tabindex HTML attribute value
	 */
	tabindex?: number | string | undefined;
	/**
	 * Label or content alignment
	 * Default value: center
	 */
	align?: 'left' | 'right' | 'center' | 'around' | 'between' | 'evenly' | undefined;
	/**
	 * Stack icon and label vertically instead of on same line (like it is by default)
	 */
	stack?: boolean | undefined;
	/**
	 * When used on flexbox parent, button will stretch to parent's height
	 */
	stretch?: boolean | undefined;
	/**
	 * Put button into loading state (displays a QSpinner -- can be overridden by using a 'loading' slot)
	 */
	loading?: boolean | undefined;
	/**
	 * Put component in disabled mode
	 */
	disable?: boolean | undefined;
	/**
	 * Makes a circle shaped button
	 */
	round?: boolean | undefined;
	/**
	 * Percentage (0.0 < x < 100.0); To be used along 'loading' prop; Display a progress bar on the background
	 */
	percentage?: number | undefined;
	/**
	 * Progress bar on the background should have dark color; To be used along with 'percentage' and 'loading' props
	 */
	darkPercentage?: boolean | undefined;
}
