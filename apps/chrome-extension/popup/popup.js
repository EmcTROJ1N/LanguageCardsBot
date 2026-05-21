const elements = {
  term: document.querySelector("#term"),
  translation: document.querySelector("#translation"),
  transcription: document.querySelector("#transcription"),
  example: document.querySelector("#example"),
  status: document.querySelector("#status"),
  translate: document.querySelector("#translate"),
  save: document.querySelector("#save")
};

elements.translate.addEventListener("click", translateTerm);
elements.save.addEventListener("click", saveCard);

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
