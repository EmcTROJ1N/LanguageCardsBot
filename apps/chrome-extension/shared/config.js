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

function normalizeBaseUrl(value) {
  return String(value ?? DEFAULT_SETTINGS.apiBaseUrl).trim().replace(/\/+$/, "");
}
