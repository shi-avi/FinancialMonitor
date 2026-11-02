
import { useEffect, useState } from "react";
import { transactionService } from "../services/transactionService";
import { signalRService } from "../services/signalRService";
import type { Transaction } from "../types/transaction";
import "./style/MonitorPage.css"

function MonitorPage() {
    const [transactions, setTransactions] = useState<Transaction[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string>("");

    useEffect(() => {
        let isActive = true;

        const handleNewTransaction = (transaction: Transaction) => {
            if (!isActive) return;

            setTransactions((currentTransactions) => {
                const alreadyExists = currentTransactions.some(
                    (item) => item.transactionId === transaction.transactionId
                );

                if (alreadyExists) return currentTransactions;

                return [transaction, ...currentTransactions];
            });
        };

        const initialize = async () => {
            try {
                // First connect to SignalR and start listening for new transactions.
                await signalRService.connect(handleNewTransaction);

                // Then load transactions that already exist in the database.
                const data = await transactionService.getAll();

                if (!isActive) return;

                // Merge existing transactions with any received in real time.
                setTransactions((currentTransactions) => {
                    const existingIds = new Set(
                        currentTransactions.map((item) => item.transactionId)
                    );

                    return [
                        ...currentTransactions,
                        ...data.filter((item) => !existingIds.has(item.transactionId)),
                    ];
                });
            } catch (error) {
                console.error(error);

                if (isActive) {
                    setError("Failed to connect or load transactions.");
                }
            } finally {
                if (isActive) {
                    setLoading(false);
                }
            }
        };

        initialize();

        return () => {
            isActive = false;
            signalRService.unsubscribe();
        };
    }, []);

    if (loading) {
        return <p>Loading transactions...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <div className="monitor-page">

            <header className="monitor-header">
                <div>
                    <h1>Transaction Monitor</h1>
                    <p>Real-time transaction activity</p>
                </div>

                <div className="live-indicator">
                    <span className="live-dot"></span>
                    LIVE CONNECTED
                </div>
            </header>

            <section className="monitor-stats">
                <div className="stat-card">
                    <div className="stat-label">TOTAL TRANSACTIONS</div>
                    <div className="stat-value">{transactions.length}</div>
                    <div className="stat-description">All transactions</div>
                </div>

                <div className="stat-card">
                    <div className="stat-label">TOTAL VOLUME</div>
                    <div className="stat-value">
                        {transactions.reduce(
                            (sum, item) => sum + Number(item.amount), 0
                        ).toLocaleString()}
                    </div>
                    <div className="stat-description">Combined amount</div>
                </div>

                <div className="stat-card">
                    <div className="stat-label">COMPLETED</div>
                    <div className="stat-value">
                        {transactions.filter(t =>
                            t.status.toLowerCase() === "completed"
                        ).length}
                    </div>
                    <div className="stat-description">Completed transactions</div>
                </div>

                <div className="stat-card">
                    <div className="stat-label">PENDING</div>
                    <div className="stat-value">
                        {transactions.filter(t =>
                            t.status.toLowerCase() === "pending"
                        ).length}
                    </div>
                    <div className="stat-description">Pending transactions</div>
                </div>

                <div className="stat-card">
                    <div className="stat-label">FAILED</div>
                    <div className="stat-value">
                        {transactions.filter(t =>
                            t.status.toLowerCase() === "failed"
                        ).length}
                    </div>
                    <div className="stat-description">Failed transactions</div>
                </div>
            </section>

            <section className="transactions-container">

                <div className="transactions-header">
                    <h2>Recent Transactions</h2>
                    <span className="transactions-count">
                        {transactions.length} Records
                    </span>
                </div>

                {transactions.length === 0 ? (
                    <div className="monitor-message">
                        No transactions found.
                    </div>
                ) : (
                    <table className="transactions-table">
                        <thead>
                            <tr>
                                <th>Transaction ID</th>
                                <th>Amount</th>
                                <th>Currency</th>
                                <th>Status</th>
                                <th>Timestamp</th>
                            </tr>
                        </thead>

                        <tbody>
                            {transactions.map((transaction) => (
                                <tr key={transaction.transactionId}>
                                    <td className="transaction-id">
                                        {transaction.transactionId}
                                    </td>
                                    <td>{transaction.amount}</td>
                                    <td>{transaction.currency}</td>
                                    <td>
                                        <span className={`status-badge status-${transaction.status.toLowerCase()}`}>
                                            {transaction.status}
                                        </span>
                                    </td>
                                    <td>
                                        {new Date(transaction.timestamp).toLocaleString()}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}

            </section>
        </div>
    );

}

export default MonitorPage;
