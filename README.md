# AzureTestApp

## Scope

This project intentionally focuses on the minimal end-to-end flow:

ASP.NET Core Web API -> Cosmos DB -> Azure Queue Storage -> Azure Function -> Cosmos DB update.

It does not implement production concerns such as authentication, idempotency, custom retry policies, dead-letter handling, distributed tracing, or advanced resilience patterns. The goal is to keep the sample small and focused on local orchestration with .NET Aspire and a basic asynchronous processing workflow.

```text
        POST /messages
             |
             v
+-------------------------+
| ASP.NET Core Web API    |
| - validates request     |
| - creates Cosmos doc    |
| - enqueues message id   |
+-----------+-------------+
            |
            | save document
            v
+-------------------------+
| Cosmos DB               |
| id, message, createdAt  |
| processedAt = null      |
+-------------------------+

            |
            | send id
            v
+-------------------------+
| Azure Queue Storage     |
| message id              |
+-----------+-------------+
            |
            | queue trigger
            v
+-------------------------+
| Azure Function          |
| - load doc by id        |
| - simulate work         |
| - set processedAtUtc    |
+-----------+-------------+
            |
            | update document
            v
+-------------------------+
| Cosmos DB               |
| processedAtUtc != null  |
+-------------------------+

        GET /messages/{id}
             |
             v
   Processing / Processed
   
```
