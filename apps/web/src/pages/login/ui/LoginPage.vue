<script setup lang="ts">
import { AuthForm } from '@/features/auth'
</script>

<template>
  <div class="scene">
    <div class="cloud" aria-hidden="true">
      <span class="w da" style="top:4%;left:2%;font-size:120px;animation-delay:0s">語</span>
      <span class="w db" style="top:6%;right:3%;font-size:95px;animation-delay:3.2s">词</span>
      <span class="w dc" style="bottom:3%;left:5%;font-size:105px;animation-delay:1.4s">言</span>
      <span class="w da" style="top:18%;left:4%;font-size:12px;animation-delay:2.1s">palabra</span>
      <span class="w db" style="top:11%;right:9%;font-size:13px;animation-delay:0.8s">слово</span>
      <span class="w dc" style="top:38%;right:5%;font-size:12px;animation-delay:2.7s">Wort</span>
      <span class="w da" style="top:58%;left:3%;font-size:11px;animation-delay:0.4s">mot</span>
      <span class="w db" style="bottom:28%;right:7%;font-size:12px;animation-delay:1.9s">parola</span>
      <span class="w dc" style="bottom:18%;left:11%;font-size:13px;animation-delay:3.4s">λέξη</span>
      <span class="w da" style="bottom:10%;right:17%;font-size:11px;animation-delay:0.9s">ord</span>
      <span class="w db" style="top:72%;right:4%;font-size:12px;animation-delay:2.3s">szó</span>
      <span class="w dc" style="bottom:7%;left:28%;font-size:11px;animation-delay:1.1s">sözcük</span>
      <span class="w da" style="top:45%;left:4%;font-size:11px;animation-delay:3.0s">keel</span>
      <span class="w db" style="top:25%;left:1%;font-size:11px;animation-delay:1.6s">langue</span>
      <span class="w dc" style="bottom:38%;right:2%;font-size:10px;animation-delay:2.5s">taal</span>
      <span class="w da" style="top:82%;left:18%;font-size:10px;animation-delay:0.2s">dil</span>
    </div>
    <div class="orb orb-warm" aria-hidden="true"></div>
    <div class="orb orb-cool" aria-hidden="true"></div>
    <AuthForm mode="login" />
  </div>
</template>

<style scoped>
.scene {
  position: relative;
  min-height: 100dvh;
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background: var(--paper-sunk);
}

.cloud {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.w {
  position: absolute;
  font-family: var(--serif);
  font-weight: 900;
  color: var(--ink);
  opacity: 0.05;
  line-height: 1;
  user-select: none;
  animation-duration: 9s;
  animation-timing-function: ease-in-out;
  animation-iteration-count: infinite;
  animation-fill-mode: both;
}

.da { animation-name: drift-a; }
.db { animation-name: drift-b; }
.dc { animation-name: drift-c; }

@keyframes drift-a {
  0%, 100% { transform: translate(0, 0); }
  35% { transform: translate(5px, -7px); }
  70% { transform: translate(-4px, 5px); }
}
@keyframes drift-b {
  0%, 100% { transform: translate(0, 0); }
  40% { transform: translate(-6px, 4px); }
  75% { transform: translate(5px, -5px); }
}
@keyframes drift-c {
  0%, 100% { transform: translate(0, 0); }
  50% { transform: translate(4px, 7px); }
}

.orb {
  position: absolute;
  border-radius: 50%;
  pointer-events: none;
  filter: blur(80px);
}
.orb-warm {
  width: 520px;
  height: 420px;
  background: radial-gradient(circle, rgba(198, 143, 75, 0.18) 0%, transparent 70%);
  top: -140px;
  right: -120px;
}
.orb-cool {
  width: 420px;
  height: 360px;
  background: radial-gradient(circle, rgba(90, 160, 168, 0.11) 0%, transparent 70%);
  bottom: -90px;
  left: -90px;
}
</style>
