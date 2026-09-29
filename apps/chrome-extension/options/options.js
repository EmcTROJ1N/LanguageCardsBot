import { getSettings, saveSettings, getCustomProviders, saveCustomProviders, getTokens } from "../shared/config.js";

// --- Settings form ---

const form = document.querySelector("#settings");
const apiBaseUrl = document.querySelector("#apiBaseUrl");
const targetLanguage = document.querySelector("#targetLanguage");
const status = document.querySelector("#status");
const authStatus = document.querySelector("#authStatus");

loadSettings();
refreshAuthStatus();
form.addEventListener("submit", handleSubmit);
chrome.storage.onChanged.addListener((changes, area) => {
  if (area === "local" && (changes.authToken || changes.refreshToken)) {
    refreshAuthStatus();
  }
});

async function loadSettings() {
  const settings = await getSettings();
  apiBaseUrl.value = settings.apiBaseUrl;
  targetLanguage.value = settings.targetLanguage;
}

async function handleSubmit(event) {
  event.preventDefault();

  const settings = await saveSettings({
    apiBaseUrl: apiBaseUrl.value,
    targetLanguage: targetLanguage.value
  });

  apiBaseUrl.value = settings.apiBaseUrl;
  targetLanguage.value = settings.targetLanguage;
  status.textContent = "Saved.";
  refreshAuthStatus();
}

async function refreshAuthStatus() {
  const { authToken } = await getTokens();
  const { apiBaseUrl: url } = await getSettings();
  if (authToken) {
    authStatus.textContent = `Signed in via web app at ${url}.`;
    authStatus.classList.remove("error");
  } else {
    authStatus.textContent = `Not signed in. Open ${url} in a tab and log in.`;
    authStatus.classList.add("error");
  }
}

// --- Custom providers ---

const providersBody = document.querySelector("#providersBody");
const providerHostname = document.querySelector("#providerHostname");
const providerSelector = document.querySelector("#providerSelector");
const addProviderBtn = document.querySelector("#addProvider");
const providersStatus = document.querySelector("#providersStatus");

loadProviders();
addProviderBtn.addEventListener("click", handleAddProvider);

async function loadProviders() {
  const providers = await getCustomProviders();
  renderProviders(providers);
}

function renderProviders(providers) {
  if (providers.length === 0) {
    providersBody.innerHTML = '<tr><td colspan="3" class="providers-empty">No custom providers yet.</td></tr>';
    return;
  }

  providersBody.innerHTML = "";
  providers.forEach((p, i) => {
    const row = document.createElement("tr");
    row.innerHTML = `
      <td>${escHtml(p.hostname)}</td>
      <td><code>${escHtml(p.selector)}</code></td>
      <td><button type="button" class="delete-btn" data-index="${i}">Remove</button></td>
    `;
    row.querySelector(".delete-btn").addEventListener("click", () => handleDeleteProvider(i));
    providersBody.appendChild(row);
  });
}

async function handleAddProvider() {
  const hostname = providerHostname.value.trim()
    .replace(/^https?:\/\//, "")
    .replace(/\/.*$/, "");
  const selector = providerSelector.value.trim();

  if (!hostname || !selector) {
    providersStatus.textContent = "Both domain and selector are required.";
    return;
  }

  const providers = await getCustomProviders();
  const existing = providers.find(p => p.hostname === hostname);
  if (existing) {
    existing.selector = selector;
  } else {
    providers.push({ hostname, selector });
  }

  await saveCustomProviders(providers);
  renderProviders(providers);
  providerHostname.value = "";
  providerSelector.value = "";
  providersStatus.textContent = "Saved.";
  setTimeout(() => { providersStatus.textContent = ""; }, 2000);
}

async function handleDeleteProvider(index) {
  const providers = await getCustomProviders();
  providers.splice(index, 1);
  await saveCustomProviders(providers);
  renderProviders(providers);
}

function escHtml(str) {
  return String(str)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
}
