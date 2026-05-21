export class CardsApiClient {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
  }

  async translate(term) {
    return this.#request("/api/cards/v3/translation", {
      method: "POST",
      body: JSON.stringify({ term })
    });
  }

  async addCard(card) {
    return this.#request("/cards", {
      method: "POST",
      body: JSON.stringify(card)
    });
  }

  async #request(path, init = {}) {
    const response = await fetch(`${this.baseUrl}${path}`, {
      ...init,
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        ...init.headers
      }
    });

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

async function readErrorMessage(response) {
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    const body = await response.json();
    return body.error ?? body.message ?? JSON.stringify(body);
  }

  return response.text();
}
