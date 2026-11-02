
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";

import type { Transaction } from "../types/transaction";
import { HUB_URL } from "../config/apiConfig";

let connection: HubConnection | null = null;

let transactionHandler:
  | ((transaction: Transaction) => void)
  | null = null;

let startPromise: Promise<void> | null = null;

export const signalRService = {
  async connect(
    onNewTransaction: (transaction: Transaction) => void
  ): Promise<void> {
    if (!connection) {
      connection = new HubConnectionBuilder()
        .withUrl(HUB_URL)
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();
    }

    const activeConnection = connection;

    // Remove the previous event handler.
    if (transactionHandler) {
      activeConnection.off("NewTransaction", transactionHandler);
    }

    // Save and register the exact same function reference.
    transactionHandler = (transaction: Transaction) => {
      console.log("SignalR received NewTransaction:", transaction);
      onNewTransaction(transaction);
    };

    activeConnection.on("NewTransaction", transactionHandler);

    if (
      activeConnection.state === HubConnectionState.Connected ||
      activeConnection.state === HubConnectionState.Connecting ||
      activeConnection.state === HubConnectionState.Reconnecting
    ) {
      return;
    }

    if (startPromise) {
      await startPromise;
      return;
    }

    startPromise = activeConnection.start()
      .then(() => {
        console.log("SignalR connected successfully!");
      })
      .catch((error) => {
        console.error("SignalR connection failed:", error);

        if (transactionHandler) {
          activeConnection.off("NewTransaction", transactionHandler);
        }

        if (connection === activeConnection) {
          connection = null;
          transactionHandler = null;
        }

        throw error;
      })
      .finally(() => {
        startPromise = null;
      });

    await startPromise;
  },

  // Remove the event listener when the Monitor page unmounts.
  // Keep the shared connection alive for the application.
  unsubscribe(): void {
    if (!connection || !transactionHandler) {
      return;
    }

    connection.off("NewTransaction", transactionHandler);
    transactionHandler = null;
  },
};
