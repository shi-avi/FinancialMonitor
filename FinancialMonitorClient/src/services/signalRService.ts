
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";

import type { Transaction } from "../types/transaction";

const HUB_URL = "https://localhost:7000/hubs/transactions";

let connection: HubConnection | null = null;

let transactionHandler:
  | ((transaction: Transaction) => void)
  | null = null;

let startPromise: Promise<void> | null = null;

export const signalRService = {
  async connect(
    onNewTransaction: (transaction: Transaction) => void
  ): Promise<void> {

    // Create the connection only once
    if (!connection) {
      connection = new HubConnectionBuilder()
        .withUrl(HUB_URL)
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();
    }

    // Remove the previous listener, if one exists
    if (transactionHandler) {
      connection.off("NewTransaction", transactionHandler);
    }

    // Register the new listener
    transactionHandler = onNewTransaction;

    connection.on("NewTransaction", transactionHandler);

    // No need to start an already active connection
    if (
      connection.state === HubConnectionState.Connected ||
      connection.state === HubConnectionState.Connecting ||
      connection.state === HubConnectionState.Reconnecting
    ) {
      return;
    }

    // If a connection attempt is already running, wait for it
    if (startPromise) {
      await startPromise;
      return;
    }

    const activeConnection = connection;

    // Start the connection
    startPromise = activeConnection
      .start()
      .catch((error) => {
        // Clean up after a failed initial connection
        activeConnection.off("NewTransaction", transactionHandler!);

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

  async disconnect(): Promise<void> {
    if (!connection) {
      return;
    }

    const activeConnection = connection;

    // Remove the listener
    if (transactionHandler) {
      activeConnection.off("NewTransaction", transactionHandler);
    }

    transactionHandler = null;
    connection = null;

    // Wait for an ongoing connection attempt
    if (startPromise) {
      try {
        await startPromise;
      } catch {
        // The connection attempt may have failed
      }
    }

    // Stop the connection
    await activeConnection.stop();
  },
};
