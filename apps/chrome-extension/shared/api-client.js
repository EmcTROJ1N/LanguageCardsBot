import { getTokens, saveTokens, clearTokens } from "./config.js";

export class CardsApiClient {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
  }

  async addCard(card) {
    return this.#request("/api/cards/cards", {
      method: "POST",
      body: JSON.stringify({
        term: card.term,
        translation: card.translation,
        transcription: card.transcription,
        example: card.example ?? null
      })
    });
  }

  async #request(path, init = {}) {
    const url = `${this.baseUrl}${path}`;
    const send = async () => {
      const { authToken } = await getTokens();
      const headers = {
        Accept: "application/json",
        "Content-Type": "application/json",
        ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}),
        ...init.headers
      };
      return fetch(url, { ...init, headers });
    };

    let response = await send();
    if (response.status === 401) {
      const refreshed = await refreshTokens(this.baseUrl);
      if (!refreshed) {
        await clearTokens();
        throw new Error("Sign in via web app to use the extension.");
      }
      response = await send();
    }

    if (!response.ok) {
      const message = await readErrorMessage(response);
      throw new Error(message || `API request failed with status ${response.status}`);
    }

    if (response.status === 204) {
      return null;
    }

    return response.json();
  }
}

let refreshInFlight = null;

async function refreshTokens(baseUrl) {
  if (refreshInFlight) return refreshInFlight;
  refreshInFlight = (async () => {
    try {
      const { refreshToken } = await getTokens();
      if (!refreshToken) return false;
      const res = await fetch(`${baseUrl}/api/passport/v1/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(refreshToken)
      });
      if (!res.ok) return false;
      const tokens = await res.json();
      if (!tokens?.accessToken || !tokens?.refreshToken) return false;
      await saveTokens({
        authToken: tokens.accessToken,
        refreshToken: tokens.refreshToken
      });
      return true;
    } catch {
      return false;
    } finally {
      refreshInFlight = null;
    }
  })();
  return refreshInFlight;
}

async function readErrorMessage(response) {
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    const body = await response.json();
    return body.error ?? body.message ?? JSON.stringify(body);
  }

  return response.text();
}
