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
    default:
      throw new Error("Unsupported extension message.");
  }
}

function requireText(value, name) {
  const text = String(value ?? "").trim();

  if (!text) {
    throw new Error(`${name} is required.`);
  }

  return text;
}

function requireUserId(value) {
  const userId = Number.parseInt(value, 10);

  if (!Number.isInteger(userId) || userId <= 0) {
    throw new Error("Set a numeric LanguageCardsBot user id in extension options.");
  }

  return userId;
}
