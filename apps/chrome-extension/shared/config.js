const DEFAULT_SETTINGS = {
  apiBaseUrl: "http://localhost:5050",
  targetLanguage: "ru"
};

export async function getSettings() {
  const stored = await chrome.storage.local.get(DEFAULT_SETTINGS);
  return {
    apiBaseUrl: normalizeBaseUrl(stored.apiBaseUrl),
    targetLanguage: normalizeLanguage(stored.targetLanguage)
  };
}

export async function saveSettings(settings) {
  const nextSettings = {
    apiBaseUrl: normalizeBaseUrl(settings.apiBaseUrl || DEFAULT_SETTINGS.apiBaseUrl),
    targetLanguage: normalizeLanguage(settings.targetLanguage || DEFAULT_SETTINGS.targetLanguage)
  };

  await chrome.storage.local.set(nextSettings);
  return nextSettings;
}

export async function getCustomProviders() {
  const result = await chrome.storage.local.get({ customProviders: [] });
  return result.customProviders;
}

export async function saveCustomProviders(providers) {
  await chrome.storage.local.set({ customProviders: providers });
}

export async function getTokens() {
  const stored = await chrome.storage.local.get({ authToken: "", refreshToken: "" });
  return {
    authToken: String(stored.authToken ?? "").trim(),
    refreshToken: String(stored.refreshToken ?? "").trim()
  };
}

export async function saveTokens({ authToken, refreshToken }) {
  await chrome.storage.local.set({
    authToken: String(authToken ?? "").trim(),
    refreshToken: String(refreshToken ?? "").trim()
  });
}

export async function clearTokens() {
  await chrome.storage.local.remove(["authToken", "refreshToken"]);
}

function normalizeBaseUrl(value) {
  return String(value ?? DEFAULT_SETTINGS.apiBaseUrl).trim().replace(/\/+$/, "");
}

function normalizeLanguage(value) {
  const lang = String(value ?? DEFAULT_SETTINGS.targetLanguage).trim().toLowerCase();
  return lang || DEFAULT_SETTINGS.targetLanguage;
}
