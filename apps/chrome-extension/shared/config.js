const DEFAULT_SETTINGS = {
  apiBaseUrl: "http://localhost:8081",
  userId: ""
};

export async function getSettings() {
  const stored = await chrome.storage.local.get(DEFAULT_SETTINGS);
  return {
    apiBaseUrl: normalizeBaseUrl(stored.apiBaseUrl),
    userId: String(stored.userId ?? "").trim()
  };
}

export async function saveSettings(settings) {
  const nextSettings = {
    apiBaseUrl: normalizeBaseUrl(settings.apiBaseUrl || DEFAULT_SETTINGS.apiBaseUrl),
    userId: String(settings.userId ?? "").trim()
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

function normalizeBaseUrl(value) {
  return String(value ?? DEFAULT_SETTINGS.apiBaseUrl).trim().replace(/\/+$/, "");
}
