const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

if (!API_BASE_URL) {
  throw new Error("VITE_API_BASE_URL is not defined");
}

export const API_URL = `${API_BASE_URL}/api/transactions`;

export const HUB_URL = `${API_BASE_URL}/hubs/transactions`;