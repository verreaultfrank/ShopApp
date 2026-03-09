---
name: Minimize Terminal Usage
description: Prohibits the AI from using the command line directly to save tokens. Instead, the AI must ask the user to execute commands and report the results.
---

# Minimize Terminal Usage & Token Optimization

This skill is designed to reduce token consumption by preventing the AI from executing long-running or high-output commands (like `dotnet build`, `npm install`, or `git status`) directly via the `run_command` tool.

## Core Mandate
**NEVER** use the `run_command`, `command_status`, or `send_command_input` tools while this skill is active, unless absolutely necessary for a non-output-generating task (like creating a directory).

## Workflow for Commands
Whenever a command needs to be executed to progress the task, follow this procedure:

1.  **Identify the Command**: Determine the exact command(s) that need to be run (e.g., `dotnet build`, `ef migrations add`, `npm start`).
2.  **Request User Execution**: Instead of running it yourself, present the command to the user in a clear code block.
3.  **Explain the Purpose**: Briefly explain why this command is necessary.
4.  **Wait for Feedback**: Ask the user to run the command in their own terminal and provide:
    -   Whether it succeeded or failed.
    -   Any relevant error messages or specific output lines (ask them to be concise).
5.  **Process Output**: Once the user provides the feedback, continue with the task as if you had run the command yourself.

## Benefits
-  **Significant Token Savings**: Prevents large build logs or diagnostic outputs from being added to the conversation history.
-  **User Control**: Ensures the user is aware of every command being run on their system.
-  **Efficiency**: Focuses the AI's context on code and logic rather than parsing terminal noise.

## Exceptions
- Creating directories (`mkdir`) or very simple file operations that have minimal output.
- **MCP Tools**: Tools from MCP servers (e.g., `sql-server`) are **EXEMPT** because they return structured data rather than raw terminal logs, and thus do not cause significant token bloat.
- If the user explicitly overrides this skill for a specific step.

