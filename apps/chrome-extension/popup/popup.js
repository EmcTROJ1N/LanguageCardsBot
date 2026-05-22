const elements = {
  term: document.querySelector("#term"),
  translation: document.querySelector("#translation"),
  transcription: document.querySelector("#transcription"),
  example: document.querySelector("#example"),
  status: document.querySelector("#status"),
  translate: document.querySelector("#translate"),
  save: document.querySelector("#save"),
  subtitleToggle: document.querySelector("#subtitleToggle")
};

elements.translate.addEventListener("click", translateTerm);
elements.save.addEventListener("click", saveCard);
elements.subtitleToggle.addEventListener("click", toggleSubtitleClicker);

let currentHostname = null;
initSubtitleToggle();

async function initSubtitleToggle() {
  try {
    const [tab] = await new Promise(resolve =>
      chrome.tabs.query({ active: true, currentWindow: true }, resolve)
    );
    if (!tab?.url || !tab.url.startsWith("http")) {
      elements.subtitleToggle.disabled = true;
      return;
    }
    currentHostname = new URL(tab.url).hostname;
    const response = await sendMessage({ type: "subtitles.getStatus", hostname: currentHostname });
    setToggleState(response.enabled);
  } catch {
    elements.subtitleToggle.disabled = true;
  }
}

async function toggleSubtitleClicker() {
  if (!currentHostname) return;
  const isOn = elements.subtitleToggle.getAttribute("aria-pressed") === "true";
  await runAction(async () => {
    if (isOn) {
      await sendMessage({ type: "subtitles.disable", hostname: currentHostname });
      setToggleState(false);
    } else {
      await sendMessage({ type: "subtitles.enable", hostname: currentHostname });
      setToggleState(true);
    }
  });
}

function setToggleState(enabled) {
  elements.subtitleToggle.setAttribute("aria-pressed", String(enabled));
  elements.subtitleToggle.textContent = enabled ? "On" : "Off";
  elements.subtitleToggle.classList.toggle("is-on", enabled);
}

async function translateTerm() {
  await runAction(async () => {
    const response = await sendMessage({
      type: "cards.translate",
      term: elements.term.value
    });

    const result = response.result;
    elements.translation.value = result.translation ?? "";
    elements.transcription.value = result.transcription ?? "";
    elements.example.value = result.example ?? "";
    setStatus("Translation loaded.");
  });
}

async function saveCard() {
  await runAction(async () => {
    await sendMessage({
      type: "cards.add",
      card: {
        term: elements.term.value.trim(),
        translation: elements.translation.value.trim(),
        transcription: elements.transcription.value.trim(),
        example: elements.example.value.trim() || null
      }
    });

    setStatus("Card saved.");
  });
}

async function runAction(action) {
  setBusy(true);
  setStatus("");

  try {
    await action();
  } catch (error) {
    setStatus(error.message, true);
  } finally {
    setBusy(false);
  }
}

function sendMessage(message) {
  return new Promise((resolve, reject) => {
    chrome.runtime.sendMessage(message, (response) => {
      if (chrome.runtime.lastError) {
        reject(new Error(chrome.runtime.lastError.message));
        return;
      }

      if (!response?.ok) {
        reject(new Error(response?.error ?? "Extension request failed."));
        return;
      }

      resolve(response.data);
    });
  });
}

function setBusy(isBusy) {
  elements.translate.disabled = isBusy;
  elements.save.disabled = isBusy;
}

function setStatus(message, isError = false) {
  elements.status.textContent = message;
  elements.status.classList.toggle("error", isError);
}
