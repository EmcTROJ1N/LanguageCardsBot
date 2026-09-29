import { CardsApiClient } from "../shared/api-client.js";
import { translateDirect } from "../shared/translator.js";
import { getSettings, getTokens, saveTokens, clearTokens } from "../shared/config.js";

const TOKEN_SYNC_ID = "token-sync";

chrome.runtime.onInstalled.addListener(() => { registerTokenSync().catch(logError); });
chrome.runtime.onStartup.addListener(() => { registerTokenSync().catch(logError); });
chrome.storage.onChanged.addListener((changes, area) => {
  if (area === "local" && changes.apiBaseUrl) {
    registerTokenSync().catch(logError);
  }
});

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  handleMessage(message)
    .then((data) => sendResponse({ ok: true, data }))
    .catch((error) => sendResponse({ ok: false, error: error.message }));

  return true;
});

async function handleMessage(message) {
  const settings = await getSettings();
  const client = new CardsApiClient(settings.apiBaseUrl);

  switch (message?.type) {
    case "cards.translate":
      return translateDirect(requireText(message.term, "term"), settings.targetLanguage);

    case "cards.add":
      return client.addCard(message.card);

    case "settings.get":
      return settings;

    case "auth.sync":
      if (message.authToken || message.refreshToken) {
        await saveTokens({
          authToken: message.authToken || "",
          refreshToken: message.refreshToken || ""
        });
      } else {
        await clearTokens();
      }
      return {};

    case "auth.status": {
      const { authToken } = await getTokens();
      return { signedIn: Boolean(authToken) };
    }

    case "subtitles.getStatus": {
      const hostname = message.hostname;
      const id = `subtitle-clicker-${hostname}`;
      const scripts = await chrome.scripting.getRegisteredContentScripts({ ids: [id] });
      return { enabled: scripts.length > 0 };
    }

    case "subtitles.enable": {
      const hostname = message.hostname;
      if (!hostname || !/^[a-zA-Z0-9.-]+$/.test(hostname)) {
        throw new Error(`Invalid hostname: ${hostname}`);
      }
      const id = `subtitle-clicker-${hostname}`;
      const pattern = `*://${hostname}/*`;
      const existing = await chrome.scripting.getRegisteredContentScripts({ ids: [id] });
      if (existing.length === 0) {
        await chrome.scripting.registerContentScripts([{
          id,
          matches: [pattern],
          js: [
            "content/translation-tooltip.js",
            "content/subtitle-clicker.js"
          ],
          css: ["content/tooltip.css"],
          runAt: "document_idle"
        }]);
      }
      const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
      if (tab?.id) {
        await chrome.scripting.insertCSS({
          target: { tabId: tab.id },
          files: ["content/tooltip.css"]
        }).catch(err => console.warn("[subtitle-clicker] CSS injection failed:", err.message));
        await chrome.scripting.executeScript({
          target: { tabId: tab.id },
          files: [
            "content/translation-tooltip.js",
            "content/subtitle-clicker.js"
          ]
        }).catch(err => console.warn("[subtitle-clicker] Script injection failed:", err.message));
      }
      return {};
    }

    case "subtitles.disable": {
      const hostname = message.hostname;
      const id = `subtitle-clicker-${hostname}`;
      const existing = await chrome.scripting.getRegisteredContentScripts({ ids: [id] });
      if (existing.length > 0) {
        await chrome.scripting.unregisterContentScripts({ ids: [id] });
      }
      return {};
    }

    default:
      throw new Error("Unsupported extension message.");
  }
}

async function registerTokenSync() {
  const { apiBaseUrl } = await getSettings();
  const pattern = toContentScriptPattern(apiBaseUrl);
  if (!pattern) return;

  const existing = await chrome.scripting.getRegisteredContentScripts({ ids: [TOKEN_SYNC_ID] });
  if (existing.length > 0) {
    await chrome.scripting.unregisterContentScripts({ ids: [TOKEN_SYNC_ID] });
  }
  await chrome.scripting.registerContentScripts([{
    id: TOKEN_SYNC_ID,
    matches: [pattern],
    js: ["content/token-sync.js"],
    runAt: "document_idle",
    allFrames: false
  }]);

  const matchUrl = new URL("/*", apiBaseUrl).toString().replace(/\*$/, "");
  const tabs = await chrome.tabs.query({ url: `${matchUrl}*` });
  for (const tab of tabs) {
    if (!tab.id) continue;
    await chrome.scripting.executeScript({
      target: { tabId: tab.id },
      files: ["content/token-sync.js"]
    }).catch(err => console.warn("[token-sync] Script injection failed:", err.message));
  }
}

function toContentScriptPattern(baseUrl) {
  try {
    const u = new URL(baseUrl);
    if (u.protocol !== "http:" && u.protocol !== "https:") return null;
    return `${u.protocol}//${u.host}/*`;
  } catch {
    return null;
  }
}

function requireText(value, name) {
  const text = String(value ?? "").trim();
  if (!text) throw new Error(`${name} is required.`);
  return text;
}

function logError(err) {
  console.warn("[background]", err?.message ?? err);
}
