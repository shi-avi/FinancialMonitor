import type { Transaction } from "../types/transaction";
import { API_URL } from "../config/apiConfig";

export const transactionService = {
  async getAll(): Promise<Transaction[]> {
    const response = await fetch(API_URL);

    if (!response.ok) {
      throw new Error("Failed to fetch transactions");
    }

    return response.json();
  },

  async create(
    transaction: Omit<Transaction, "transactionId" | "timestamp">
  ): Promise<Transaction> {
    const response = await fetch(API_URL, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(transaction),
    });

    if (!response.ok) {
      throw new Error("Failed to create transaction");
    }

    return response.json();
  },
};