(function () {
  if (window.__lcTokenSync) return;
  window.__lcTokenSync = true;

  sync();
  window.addEventListener("storage", event => {
    if (event.key === "auth_token" || event.key === "refresh_token" || event.key === null) {
      sync();
    }
  });
  window.addEventListener("focus", sync);

  function sync() {
    try {
      const authToken = localStorage.getItem("auth_token") || "";
      const refreshToken = localStorage.getItem("refresh_token") || "";
      chrome.runtime.sendMessage(
        { type: "auth.sync", authToken, refreshToken },
        () => void chrome.runtime.lastError
      );
    } catch {
      // localStorage may be blocked (rare); ignore.
    }
  }
})();
