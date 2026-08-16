<script setup lang="ts">
import { AuthForm } from '@/features/auth'
</script>

<template>
  <div class="auth">
    <div class="auth__left">
      <div class="brand">
        <span class="serif">Language</span>
        <span>Cards Cabinet</span>
      </div>
      <blockquote class="serif epigraph">
        <span class="mono">Ep.</span>
        Every word is a small door. If you walk through it enough times, it stays open.
      </blockquote>
      <div class="epigraph__attr mono">— Cabinet manifesto, 2026</div>
      <div class="marks">
        <span class="mono muted">v0.1.0 · mockup edition</span>
      </div>
    </div>
    <AuthForm />
  </div>
</template>

<style scoped>
.auth {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 100vh;
  width: 100%;
  max-width: 1200px;
  background: var(--paper);
  border: 1px solid var(--rule);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lifted);
  overflow: hidden;
}
@media (max-width: 900px) { .auth { grid-template-columns: 1fr; } .auth__left { display: none; } }
.auth__left {
  padding: 56px 48px;
  background: var(--ink);
  color: var(--paper);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 32px;
  position: relative;
}
.auth__left::before {
  content: '';
  position: absolute;
  inset: 40px 40px auto auto;
  width: 100px;
  height: 100px;
  background: radial-gradient(circle at 25% 25%, var(--ochre) 0%, transparent 65%);
  opacity: 0.35;
}
.brand { font-family: var(--serif); font-size: 22px; line-height: 1.1; color: var(--paper); }
.brand span:first-child { color: var(--ochre-deep); display: block; }
.epigraph { font-family: var(--serif); font-size: 28px; line-height: 1.3; color: var(--paper); margin: 0; border-left: 3px solid var(--ochre); padding-left: 20px; font-weight: 300; font-variation-settings: 'opsz' 100, 'SOFT' 60; }
.epigraph .mono { color: var(--ochre-deep); font-size: 12px; display: block; margin-bottom: 8px; }
.epigraph__attr { color: var(--ochre-deep); font-size: 12px; margin-top: -20px; }
.marks .muted { color: rgba(20,23,30,0.55); font-size: 11px; }
</style>
