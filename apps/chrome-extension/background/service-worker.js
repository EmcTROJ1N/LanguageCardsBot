import { CardsApiClient } from "../shared/api-client.js";
import { getSettings } from "../shared/config.js";

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
      return client.translate(requireText(message.term, "term"));

    case "cards.add":
      return client.addCard({
        ...message.card,
        userId: requireUserId(settings.userId)
      });

    case "settings.get":
      return settings;

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
          js: ["content/subtitle-clicker.js"],
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
          files: ["content/subtitle-clicker.js"]
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

function requireText(value, name) {
  const text = String(value ?? "").trim();
  if (!text) throw new Error(`${name} is required.`);
  return text;
}

function requireUserId(value) {
  const userId = Number.parseInt(value, 10);
  if (!Number.isInteger(userId) || userId <= 0) {
    throw new Error("Set a numeric LanguageCardsBot user id in extension options.");
  }
  return userId;
}
