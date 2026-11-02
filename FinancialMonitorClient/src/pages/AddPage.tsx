import { useState } from "react";
import { transactionService } from "../services/transactionService";
import type { TransactionStatus } from "../types/transaction";

function AddPage() {
  const [amount, setAmount] = useState<string>("");
  const [currency, setCurrency] = useState<string>("USD");
  const [status, setStatus] = useState<TransactionStatus>("Pending");

  const [loading, setLoading] = useState<boolean>(false);
  const [message, setMessage] = useState<string>("");

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    setLoading(true);
    setMessage("");

    try {
      const createdTransaction = await transactionService.create({
        amount: Number(amount),
        currency,
        status,
      });

      setMessage(
        `Transaction ${createdTransaction.transactionId} created successfully!`
      );

      setAmount("");
    } catch (error) {
      setMessage("Failed to create transaction. Please try again.");
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1>Add Transaction</h1>

      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="amount">Amount:</label>
          <input
            id="amount"
            type="number"
            min="0.01"
            step="0.01"
            value={amount}
            onChange={(event) => setAmount(event.target.value)}
            required
          />
        </div>

        <div>
          <label htmlFor="currency">Currency:</label>
          <select
            id="currency"
            value={currency}
            onChange={(event) => setCurrency(event.target.value)}
          >
            <option value="USD">USD</option>
            <option value="EUR">EUR</option>
            <option value="ILS">ILS</option>
          </select>
        </div>

        <div>
          <label htmlFor="status">Status:</label>
          <select
            id="status"
            value={status}
            onChange={(event) =>
              setStatus(event.target.value as TransactionStatus)
            }
          >
            <option value="Pending">Pending</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
          </select>
        </div>

        <button type="submit" disabled={loading}>
          {loading ? "Creating..." : "Create Transaction"}
        </button>
      </form>

      {message && <p>{message}</p>}
    </div>
  );
}

export default AddPage;