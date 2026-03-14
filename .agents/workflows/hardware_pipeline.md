---
description: The Hardware Integration Pipeline (Adding a New Machine)
---

**Trigger:** The user asks to integrate a new CNC machine, robotic arm, or sensor array (e.g., "Add support for a Haas UMC-500" or "Integrate a Universal Robot UR10e").

1.  **Domain Layer (Contract & State):**
    *   Define the physical capabilities using interfaces (e.g., `ICncMillingMachine`, `IRoboticArm`) inheriting from the base `IEquipment`.
    *   Define the exact State Machine (using `Stateless`) for this equipment (e.g., `Idle` -> `WarmingUp` -> `ExecutingToolpath` -> `Inspection` -> `Idle`).
    *   Define the `record struct` types for the machine's specific telemetry.
2.  **Application Layer (Use Cases):**
    *   Create MediatR Commands for hardware actions (e.g., `LoadToolCommand`, `ExecuteGCodeCommand`).
    *   Define the `IMachineCommunicator<TMachine>` interface required to talk to it.
3.  **Infrastructure Layer (The Bridge):**
    *   Implement the communicator using the required industrial protocol (OPC UA, MQTT, MTConnect, or ROS2 via gRPC).
    *   Wrap all network I/O in **Polly** resilience policies (Retries, Circuit Breakers) to handle shop-floor network drops safely.
4.  **Presentation Layer (MAUI UI):**
    *   Create the MVVM ViewModel for the machine's dashboard.
    *   Ensure the UI observes the machine's status without blocking the main thread (using `Observable` or `Dispatcher.DispatchAsync`).