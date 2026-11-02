# Real-Time Financial Monitor

A full-stack financial transaction monitoring system built with **.NET 8, ASP.NET Core, SignalR, SQLite, React, and TypeScript**.

The application receives financial transactions through an HTTP API, stores them in a database, and broadcasts new transactions to connected clients in real time. Support agents can monitor incoming transactions through a live dashboard and filter them by status.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Real-Time Communication](#real-time-communication)
- [Testing](#testing)
- [Performance Testing](#performance-testing)
- [Architectural Decisions](#architectural-decisions)
- [Future Improvements](#future-improvements)

---

## Features

### Backend
- REST API for transaction ingestion and retrieval.
- Real-time transaction broadcasting using SignalR.
- SQLite persistence using Entity Framework Core.
- Server-generated transaction IDs and timestamps.
- Input validation.
- Asynchronous database operations.
- Dependency injection and repository/service separation.

### Frontend
- Transaction Simulator at `/add`.
- Live Dashboard at `/monitor`.
- Real-time updates through SignalR.
- Transaction status indicators.
- Client-side filtering by transaction status.
- Responsive dashboard layout.

### Testing
- Unit tests for transaction processing.
- Repository tests using SQLite in-memory storage.
- API integration tests.
- Concurrent transaction insertion test.
- Performance test script for sending multiple requests.

---

## Architecture

The application consists of two main components:

**1. Backend – FinancialMonitorApi**

Responsible for:
- Receiving and validating transactions.
- Generating transaction IDs and timestamps.
- Persisting transactions in SQLite.
- Broadcasting new transactions to connected clients through SignalR.
- Providing endpoints for retrieving transactions.

**2. Frontend – FinancialMonitorClient**

Responsible for:
- Sending transactions to the backend.
- Connecting to the SignalR hub.
- Displaying incoming transactions in real time.
- Filtering transactions by status.
- Presenting transaction statistics and details.

### Data Flow

1. A transaction is submitted through the Transaction Simulator.
2. The frontend sends an HTTP POST request to the backend.
3. The backend validates the request.
4. The backend generates a unique transaction ID and UTC timestamp.
5. The transaction is saved in SQLite.
6. The backend broadcasts the saved transaction through SignalR.
7. Connected dashboards receive and display the new transaction.

---

## Technology Stack

| Component | Technology |
|---|---|
| Backend | .NET 8 / ASP.NET Core |
| API | ASP.NET Core Web API |
| Real-Time Communication | SignalR |
| ORM | Entity Framework Core |
| Database | SQLite |
| Frontend | React |
| Language | TypeScript |
| Build Tool | Vite |
| Testing | xUnit |
| Integration Testing | WebApplicationFactory |
| Mocking | Moq |

---

## Project Structure

```text
FinancialMonitor/
│
├── FinancialMonitorApi/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Hubs/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
│
├── FinancialMonitorApi.Tests/
│   ├── Integration/
│   ├── Repositories/
│   └── Services/
│
├── FinancialMonitorClient/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── types/
│   │   └── config/
│   ├── .env.example
│   └── package.json
│
├── scripts/
│   └── performance-test.ps1
│
└── README.md
```

---

## Prerequisites

Before running the application, install:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (LTS)](https://nodejs.org/)
- npm (included with Node.js)
- [Git](https://git-scm.com/)

Verify the installations:

```powershell
dotnet --version
node --version
npm --version
git --version
```

---

## Getting Started

### 1. Clone the repository

```powershell
git clone https://github.com/shi-avi/FinancialMonitor.git
cd FinancialMonitor
```

### 2. Restore backend dependencies

From the project root:

```powershell
dotnet restore .\FinancialMonitorApi\FinancialMonitorApi.csproj
```

### 3. Install frontend dependencies

```powershell
cd .\FinancialMonitorClient
npm install
cd ..
```

---

## Configuration

The backend and frontend URLs may vary depending on the local environment and the ports available on each machine.

Before running the application, configure the URLs so that both projects can communicate correctly.

### 1. Start the Backend First

From the repository root, run:

```powershell
dotnet run --project .\FinancialMonitorApi\FinancialMonitorApi.csproj
```

Wait for ASP.NET Core to start. In the terminal output, look for the address shown in the `Now listening on:` line.

For example:

```text
Now listening on: http://localhost:5044
```

Copy the actual backend URL displayed in your terminal.

### 2. Configure the Frontend Backend URL

Inside `FinancialMonitorClient`, create a `.env` file using `.env.example` as a template:

```powershell
Copy-Item .env.example .env
```

Open `.env` and set `VITE_API_BASE_URL` to the backend URL you copied:

```env
VITE_API_BASE_URL=http://localhost:5044
```

Replace the example URL with the actual backend URL from your terminal.

The frontend uses this variable for both HTTP API requests and SignalR connections.

### 3. Start the Frontend

Open a separate terminal and run:

```powershell
cd .\FinancialMonitorClient
npm run dev
```

Wait for Vite to start. Copy the frontend address displayed in the terminal.

For example:

```text
Local: http://localhost:5173/
```

Use the actual address displayed on your machine.

### 4. Configure Backend CORS

Open:

`FinancialMonitorApi/appsettings.json`

Update `Frontend.Url` to match the frontend address displayed by Vite:

```json
{
  "Frontend": {
    "Url": "http://localhost:5173"
  }
}
```

Replace the example with the actual frontend URL.

This setting allows the backend to accept requests from the frontend origin through the configured CORS policy.

After changing backend configuration, restart the backend.

**Important:**
- Configure the frontend with the backend URL.
- Configure the backend CORS setting with the frontend URL.
- Use the exact URLs displayed in the terminals, including the correct scheme and port.
- If either URL changes, update the corresponding configuration.
- Restart the relevant development server after changing its configuration.
- Do not commit your local `.env` file. Keep `.env.example` as a safe template.

--- 

## Database Setup

The backend uses SQLite with Entity Framework Core.

The default connection string is:

```text
Data Source=financialmonitor.db
```

The database file is created locally and should not be committed to source control.

The repository includes EF Core migrations.

If the EF Core CLI is not installed, install it:

```powershell
dotnet tool install --global dotnet-ef
```

If it is already installed, update it if necessary:

```powershell
dotnet tool update --global dotnet-ef
```

Apply the migrations from the repository root:

```powershell
dotnet ef database update --project .\FinancialMonitorApi\FinancialMonitorApi.csproj
```

This creates or updates the local database schema.

---

## Running the Application

Run the backend and frontend in separate terminal windows.

### 1. Start the Backend

From the repository root:

```powershell
dotnet run --project .\FinancialMonitorApi\FinancialMonitorApi.csproj
```

Wait for the backend to start and copy the URL displayed in the `Now listening on:` line.

For example:

```text
Now listening on: http://localhost:5044
```

The actual URL and port may differ depending on your environment.

In Development mode, Swagger is available at:

`<Backend URL>/swagger`

### 2. Start the Frontend

Open another terminal:

```powershell
cd .\FinancialMonitorClient
npm run dev
```

Wait for Vite to start and copy the URL displayed next to `Local:`.

For example:

```text
Local: http://localhost:5173/
```

The actual URL and port may differ depending on your environment.

### 3. Open the Application

Use the frontend URL displayed in your terminal:

- **Transaction Simulator:** `<Frontend URL>/add`
- **Live Dashboard:** `<Frontend URL>/monitor`

Keep both development servers running while using the application.

---

## API Endpoints

The API base URL is the backend URL displayed in the terminal when the backend starts.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/transactions` | Create a new transaction |
| GET | `/api/transactions` | Retrieve all transactions |
| GET | `/api/transactions/{id}` | Retrieve a transaction by ID |

### Create Transaction

Example request:

```http
POST /api/transactions
Content-Type: application/json
```

```json
{
  "amount": 1500.50,
  "currency": "USD",
  "status": "Pending"
}
```

The backend validates the input, generates the transaction ID and timestamp, saves the transaction, and broadcasts it to connected clients.

### Transaction Response

A transaction returned by the API contains:

```json
{
  "transactionId": "generated-guid",
  "amount": 1500.50,
  "currency": "USD",
  "status": "Pending",
  "timestamp": "2024-01-15T10:00:00Z"
}
```

The exact timestamp and transaction ID are generated by the backend.

---

## Real-Time Communication

The application uses SignalR for real-time communication.

Hub endpoint:

```text
/hubs/transactions
```

When a transaction is successfully saved, the backend broadcasts an event named:

```text
NewTransaction
```

Connected clients receive the transaction and update the dashboard without requiring a page refresh.

SignalR manages multiple client connections and supports concurrent connected clients.

---

## Testing

The backend test project is:

`FinancialMonitorApi.Tests`

Run all tests from the repository root:

```powershell
dotnet test .\FinancialMonitorApi.Tests\FinancialMonitorApi.Tests.csproj
```

The tests cover:

- Transaction service validation and processing.
- Repository storage and retrieval.
- SQLite in-memory persistence.
- API integration: creating a transaction and retrieving it by ID.
- Concurrent insertion of multiple transactions.

The test project uses xUnit and ASP.NET Core WebApplicationFactory for integration testing.

---

## Performance Testing

A basic concurrency test was performed using PowerShell 5.1.

- Total requests: 100
- Concurrent workers: 20
- Successful requests: 100
- Failed requests: 0
- Total duration: 29.03 seconds
- Throughput: 3.44 requests/second

All 100 requests were processed successfully.

The test script is located at:

`scripts/performance-test.ps1`

Run the script from the repository root:

```powershell
.\scripts\performance-test.ps1
```

Make sure the backend is running and review the script's configuration before executing it.

---

## Architectural Decisions

### Server-Generated Transaction ID and Timestamp

The client submits only the business data required to create a transaction:

- Amount
- Currency
- Status

The backend generates:

- TransactionId – a new GUID.
- Timestamp – the current UTC time.

**Reasoning:**

The backend is the single source of truth for transaction identity and creation time.

Generating these values on the server:
- Prevents clients from submitting duplicate or arbitrary transaction IDs.
- Ensures timestamps are generated consistently.
- Reduces the amount of trusted data supplied by external clients.
- Simplifies the client-side simulator.

This is an intentional design decision for the MVP.

### Repository and Service Layers

The application separates responsibilities:

- Controllers handle HTTP requests and responses.
- Services contain transaction processing and business validation.
- Repositories handle data persistence.
- Entity Framework Core manages database access.

This separation improves testability and makes future changes easier.

### SQLite

SQLite was selected as a lightweight persistent database suitable for an MVP and local development.

It avoids requiring an external database server while still providing persistent storage.

### SignalR

SignalR was selected instead of implementing a raw WebSocket server.

It provides connection management, broadcasting, and reconnection support while reducing the amount of custom real-time infrastructure code.

---

## Future Improvements

Potential next steps include:

- Redis SignalR Backplane for broadcasting across multiple backend instances.
- Docker production images for the backend and frontend.
- Kubernetes Deployment and Service manifests.
- Distributed database configuration.
- Enhanced dashboard animations.
- Additional load and browser responsiveness testing.
- Authentication and authorization.
- Monitoring, structured logging, and health checks.

---

## Project Notes

This project was created as a Full Stack assessment demonstrating backend development, real-time communication, frontend integration, persistence, testing, and architectural decision-making.
