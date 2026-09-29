const TRANSLATE_URL = "https://translate.googleapis.com/translate_a/single";

export async function translateDirect(term, targetLanguage = "ru") {
  const url = `${TRANSLATE_URL}?client=gtx&sl=auto&tl=${encodeURIComponent(targetLanguage)}&dt=t&q=${encodeURIComponent(term)}`;
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`Translation failed: HTTP ${response.status}`);
  }

  const data = await response.json();
  const translation = parseTranslation(data);

  if (!translation) {
    throw new Error("Translation provider returned an empty result.");
  }

  return { translation, transcription: "", example: "" };
}

function parseTranslation(data) {
  if (!Array.isArray(data) || !Array.isArray(data[0])) return "";
  return data[0]
    .filter(seg => Array.isArray(seg) && typeof seg[0] === "string")
    .map(seg => seg[0])
    .join("")
    .trim();
}
