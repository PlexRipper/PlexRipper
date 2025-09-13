export function setupBackgroundEffect() {
	// Check if WebGL is available.
	const canvas = document.createElement('canvas');
	const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
	window.waveEffect = null;

	if (gl instanceof WebGLRenderingContext) {
		if (document.getElementsByClassName('vanta-canvas').length > 0) {
			return;
		}

		window.waveEffect = VANTA.WAVES({
			el: 'body',
			mouseControls: true,
			touchControls: true,
			gyroControls: false,
			minHeight: 1000.0,
			minWidth: 1200.0,
			scale: 1.0,
			scaleMobile: 1.0,
			color: 0x880000,
			shininess: 43.0,
			waveHeight: 4.0,
			waveSpeed: 1.25,
			zoom: 0.65,
		});
	}
}

export function destroyBackgroundEffect() {
	if (window.waveEffect) {
		window.waveEffect.destroy();
	}
	for (const elementsByClassNameElement of document.getElementsByClassName('vanta-canvas')) {
		elementsByClassNameElement.remove();
	}
}

setupBackgroundEffect();
