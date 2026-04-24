# SagaCompensator
---

## What is SagaCompensator?

**SagaCompensator** is a lightweight .NET library that provides an in-memory orchestrator for managing compensating transactions in distributed workflows based on the Saga pattern.

When a saga step fails, all previously completed steps must be undone in reverse order. SagaCompensator handles this automatically using a LIFO (last-in, first-out) stack internally, ensuring that compensations are always executed in the correct order without any manual coordination.

It integrates natively with the .NET dependency injection system and supports structured logging through `Microsoft.Extensions.Logging`.

---

## Key Concepts

### Saga Pattern
A saga is a sequence of local transactions, each belonging to a different service or bounded context. If any step fails, the saga triggers compensating transactions for all previously succeeded steps to restore consistency.

### Compensation
A compensation is the business-level "undo" operation for a completed step. It is not a technical database rollback — it is a new forward action that reverses the observable effect of a previous one (for example, cancelling a reservation that was already confirmed, or issuing a refund for a charge already processed).

### Orchestrator
In an orchestrated saga, a central coordinator controls the flow: it calls each step in sequence and decides what to do on success or failure. SagaCompensator integrates into this coordinator layer, providing it with the tooling needed to register, track, and execute compensations cleanly.

---

## How It Works

1. Before executing the saga, you obtain an `ISagaOrchestrator` instance (via dependency injection or directly).
2. As each step completes successfully, you register its corresponding compensation action using `AddCompensation` or automatically via `ExecuteStepAsync`.
3. If any step fails, you call `RollbackAsync()` and the library executes all registered compensations in reverse order.
4. If a compensation itself throws an error, it is logged and execution continues with the remaining compensations — ensuring the rollback process is always completed.
5. `RollbackAsync()` is idempotent: calling it multiple times has no additional effect.

---

## Installation

SagaCompensator is distributed as a NuGet package targeting **.NET 10.0**.

### .NET CLI

dotnet add package SagaCompensator
