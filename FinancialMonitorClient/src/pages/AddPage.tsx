import { useState } from "react";
import { transactionService } from "../services/transactionService";
import type { TransactionStatus } from "../types/transaction";
import "./style/AddPage.css";

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
    <div className="add-page">
      <div className="add-header">
        <div className="header-icon">↗</div>
        <div>
          <h1>Add Transaction</h1>
          <p>Create a new financial transaction</p>
        </div>
      </div>

      <div className="transaction-card">
        <div className="card-header">
          <h2>Transaction Details</h2>
          <p>Enter the transaction information below</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="amount">Amount</label>
            <div className="input-wrapper">
              <span className="input-icon">$</span>
              <input
                id="amount"
                type="number"
                min="0.01"
                step="0.01"
                placeholder="0.00"
                value={amount}
                onChange={(event) => setAmount(event.target.value)}
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="currency">Currency</label>
            <select
              id="currency"
              value={currency}
              onChange={(event) => setCurrency(event.target.value)}
            >
              <option value="USD">USD - US Dollar</option>
              <option value="EUR">EUR - Euro</option>
              <option value="ILS">ILS - Israeli Shekel</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="status">Transaction Status</label>
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
            {!loading && <span>→</span>}
          </button>
        </form>

        {message && (
          <div
            className={`form-message ${
              message.startsWith("Transaction") ? "success" : "error"
            }`}
          >
            {message}
          </div>
        )}
      </div>

      <p className="page-footer">
        Financial Monitor · Transaction Management
      </p>
    </div>
  );
}

export default AddPage;