(function () {
  if (window.__lcSelectionTranslator) return;
  window.__lcSelectionTranslator = true;

  const ICON_ID = "lc-selection-icon";
  const MAX_SELECTION_LENGTH = 100;
  const MAX_CONTEXT_LENGTH = 500;

  document.addEventListener("mouseup", onMouseUp, true);
  document.addEventListener("mousedown", onMouseDown, true);
  document.addEventListener("scroll", removeIcon, true);
  window.addEventListener("resize", removeIcon);
  document.addEventListener("selectionchange", onSelectionChange);

  function onMouseUp() {
    setTimeout(tryShowIcon, 0);
  }

  function onMouseDown(e) {
    if (e.target.closest(`#${ICON_ID}`)) return;
    if (e.target.closest("#lc-tooltip")) return;
    removeIcon();
  }

  function onSelectionChange() {
    const selection = window.document.getSelection();
    if (!selection || selection.isCollapsed) removeIcon();
  }

  function tryShowIcon() {
    const selection = window.getSelection();
    if (!selection || selection.isCollapsed || selection.rangeCount === 0) return;

    const term = selection.toString().trim();
    if (!term || term.length > MAX_SELECTION_LENGTH) return;

    const range = selection.getRangeAt(0);
    if (!isSelectableRange(range)) return;

    const rect = range.getBoundingClientRect();
    if (rect.width === 0 && rect.height === 0) return;

    showIcon(rect, term);
  }

  function isSelectableRange(range) {
    const container = range.commonAncestorContainer;
    const el = container.nodeType === Node.TEXT_NODE ? container.parentElement : container;
    if (!el) return false;
    if (el.closest("input, textarea")) return false;
    if (el.closest("[contenteditable=''], [contenteditable='true']")) return false;
    if (el.closest(`#${ICON_ID}, #lc-tooltip`)) return false;
    return true;
  }

  function showIcon(rect, term) {
    removeIcon();
    const icon = document.createElement("div");
    icon.id = ICON_ID;
    icon.title = "Перевести и сохранить";
    icon.innerHTML = `
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path fill="currentColor" d="M12.87 15.07l-2.54-2.51.03-.03A17.52 17.52 0 0 0 14.07 6H17V4h-7V2H8v2H1v2h11.17C11.5 7.92 10.44 9.75 9 11.35 8.07 10.32 7.3 9.19 6.69 8h-2c.73 1.63 1.73 3.17 2.98 4.56l-5.09 5.02L4 19l5-5 3.11 3.11.76-2.04zM18.5 10h-2L12 22h2l1.12-3h4.75L21 22h2l-4.5-12zm-2.62 7l1.62-4.33L19.12 17h-3.24z"/>
      </svg>`;

    const ICON_SIZE = 28;
    const MARGIN = 4;
    let top = rect.top - ICON_SIZE - MARGIN;
    if (top < MARGIN) top = rect.bottom + MARGIN;
    let left = rect.right - ICON_SIZE / 2;
    left = Math.max(MARGIN, Math.min(left, window.innerWidth - ICON_SIZE - MARGIN));
    icon.style.top = `${top}px`;
    icon.style.left = `${left}px`;
    document.body.appendChild(icon);

    icon.addEventListener("mousedown", e => {
      e.preventDefault();
      e.stopPropagation();
    });
    icon.addEventListener("click", e => {
      e.stopPropagation();
      const context = extractSentence(term);
      const iconRect = icon.getBoundingClientRect();
      removeIcon();
      window.__lcTranslationTooltip?.show({
        term,
        anchorRect: iconRect,
        context
      });
    });
  }

  function removeIcon() {
    document.getElementById(ICON_ID)?.remove();
  }

  function extractSentence(term) {
    const selection = window.getSelection();
    if (!selection || selection.rangeCount === 0) return term;
    const range = selection.getRangeAt(0);
    const container = range.commonAncestorContainer;
    const block = findBlockAncestor(container);
    const blockText = (block.innerText || block.textContent || "").replace(/\s+/g, " ").trim();
    if (!blockText) return term;

    const idx = blockText.indexOf(term);
    if (idx < 0) return truncate(blockText, MAX_CONTEXT_LENGTH);

    const boundary = /[.!?…]/;
    let start = idx;
    while (start > 0 && !boundary.test(blockText[start - 1])) start--;
    let end = idx + term.length;
    while (end < blockText.length && !boundary.test(blockText[end])) end++;
    if (end < blockText.length) end++;
    const sentence = blockText.slice(start, end).trim();
    return truncate(sentence || blockText, MAX_CONTEXT_LENGTH);
  }

  function findBlockAncestor(node) {
    let el = node.nodeType === Node.TEXT_NODE ? node.parentElement : node;
    while (el && el !== document.body) {
      const display = getComputedStyle(el).display;
      if (display && !display.startsWith("inline")) return el;
      el = el.parentElement;
    }
    return el || document.body;
  }

  function truncate(text, max) {
    return text.length <= max ? text : text.slice(0, max).trim() + "…";
  }
})();
