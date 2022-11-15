<template>
  <div id="background-effect">
    <div style="z-index: 1">
      <slot />
    </div>
  </div>
</template>

<script lang="ts">
import Log from "consola";
import { Component, Vue } from "vue-property-decorator";
import * as THREE from "three";
import WAVES from "vanta/dist/vanta.waves.min";
import WebGL from "@class/WebGL";

@Component
export default class Background extends Vue {
  vantaEffect: any;

  mounted(): void {
    if (WebGL.isWebGLAvailable()) {
      Log.info("Wave effect created!");
      this.vantaEffect = WAVES({
        THREE,
        el: "#background-effect",
        mouseControls: true,
        touchControls: true,
        gyroControls: false,
        minHeight: 200.0,
        minWidth: 200.0,
        scale: 1.0,
        scaleMobile: 1.0,
        color: 0x880000,
        shininess: 43.0,
        waveHeight: 4.0,
        waveSpeed: 1.25,
        zoom: 0.65
      });
    }
  }

  beforeDestroy(): void {
    if (this.vantaEffect) {
      Log.info("Wave effect destroyed!");
      this.vantaEffect.destroy();
    }
  }
}
</script>

<style lang="scss">
#background-effect {
  width: 100%;
  height: 100%;
  background-image: url('~assets/img/background/background.png');
}
</style>
