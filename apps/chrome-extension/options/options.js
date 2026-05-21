import { getSettings, saveSettings } from "../shared/config.js";

const form = document.querySelector("#settings");
const apiBaseUrl = document.querySelector("#apiBaseUrl");
const userId = document.querySelector("#userId");
const status = document.querySelector("#status");

loadSettings();
form.addEventListener("submit", handleSubmit);

async function loadSettings() {
  const settings = await getSettings();
  apiBaseUrl.value = settings.apiBaseUrl;
  userId.value = settings.userId;
}

async function handleSubmit(event) {
  event.preventDefault();

  const settings = await saveSettings({
    apiBaseUrl: apiBaseUrl.value,
    userId: userId.value
  });

  apiBaseUrl.value = settings.apiBaseUrl;
  userId.value = settings.userId;
  status.textContent = "Saved.";
}
