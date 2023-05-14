import Log from 'consola';

const highlightActiveClass = 'highlight-border-box-active';

export function triggerBoxHighlight(element: HTMLElement | null) {
	// Needs to keep a copy because the scrollTargetElement can be changed before the timeout
	if (!element) {
		Log.error('HTMLElement is null, could not display highlight');
		return;
	}

	// Check if already highlighted
	if (element.classList.contains(highlightActiveClass)) {
		return;
	}
	element.classList.add(highlightActiveClass);
	setTimeout(() => {
		element.classList.remove(highlightActiveClass);
		// This is the same number in animations.scss => .highlight-border-box --animation-speed:
	}, 1600);
}
