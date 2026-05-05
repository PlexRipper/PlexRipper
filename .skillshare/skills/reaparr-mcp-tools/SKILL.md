---
name: reaparr-mcp-tools
description: Use when working in Reaparr and any MCP server access, IDE access, GitHub access, browser automation, or external documentation lookup must go through mcpproxy-go. This skill is mandatory at the start of every task and defines the required discovery, routing, safety, and fallback workflow for proxied MCP tools.
---

# Reaparr MCP Tools

## Purpose

This skill is mandatory for every Reaparr task. Load `reaparr-mcp-tools` before using any MCP-backed capability, including Rider, WebStorm, GitHub, Playwright, Context7, or any other upstream server exposed through `mcpproxy-go`.

In this repo, assume all MCP servers are behind `mcpproxy-go`. Treat the proxy as the normal access path, not a fallback detail.

Default proxy endpoint assumption:

```text
http://127.0.0.1:8080/mcp
```

## Core Rules

1. Always use JetBrains MCP first (Rider for backend, WebStorm for frontend).
2. Do not guess MCP tool names.
3. Discover tools first with `mcp-proxy_retrieve_tools`.
4. Use the exact discovered tool name when calling it.
5. Respect the returned `call_with` guidance.
6. Always provide `intent_reason` and `intent_data_sensitivity` on every proxy tool call.
7. Treat sparse discovery results as a tooling limitation to retry around, not proof that the server is unavailable.

## Proxy Workflow

Use this flow for any proxied MCP action:

1. If server health matters, check `mcp-proxy_upstream_servers` first.
2. Discover tools with `mcp-proxy_retrieve_tools`.
3. Read the returned `call_with` field.
4. Invoke the exact discovered tool through the matching proxy call:
   - `mcp-proxy_call_tool_read`
   - `mcp-proxy_call_tool_write`
   - `mcp-proxy_call_tool_destructive`
5. If output is truncated, continue with `mcp-proxy_read_cache`.
6. If discovery is sparse or misleading, retry with a narrower query before falling back.

## Tool Routing

Use the discovered `call_with` field as the source of truth. When deciding how to think about the tool, use these naming heuristics:

| Variant | Typical names | Use for |
| --- | --- | --- |
| `mcp-proxy_call_tool_read` | `get`, `read`, `list`, `find`, `search`, `query`, `inspect`, `show`, `check` | Safe inspection, diagnostics, docs, health checks, file reads |
| `mcp-proxy_call_tool_write` | `create`, `update`, `edit`, `replace`, `patch`, `write`, `add`, `configure`, `rename` | State changes requested by the user |
| `mcp-proxy_call_tool_destructive` | `delete`, `remove`, `destroy`, `drop`, `revoke`, `clear`, `reset` | High-risk or irreversible actions |

Examples:

- `github:get_file_contents` -> `mcp-proxy_call_tool_read`
- `github:create_issue` -> `mcp-proxy_call_tool_write`
- `github:delete_repository` -> `mcp-proxy_call_tool_destructive`

## Intent Fields

Every proxy tool call must include:

- `intent_reason`: why this call is needed in the current task
- `intent_data_sensitivity`: one of `public`, `internal`, `private`, or `unknown`

Use the narrowest honest value.

Examples:

- Repo file inspection in Reaparr: `internal`
- Public upstream docs lookup: `public`
- User profile or secrets-related data: `private`

## IDE Policy

### Backend

Backend work must go through Rider behind `mcpproxy-go` first. Prefer these upstream servers:

- `rider-official`
- `rider-index`
- `rider-debugger`

### Frontend

Frontend work must go through WebStorm behind `mcpproxy-go` first. Prefer these upstream servers:

- `webstorm-official`
- `webstorm-index`
- 
#### Mandatory Project Path

Every Rider and WebStorm MCP call must include this exact argument:

```json
{
  "project_path": "/mnt/PROJECTS/Reaparr"
}
```

This applies to all JetBrains-backed calls (read, write, diagnostics, run/debug, search, refactor).

### Required behavior

1. Do not assume Rider or WebStorm is unavailable just because native tools are absent.
2. Check `mcp-proxy_upstream_servers` when availability is unclear.
3. If the relevant upstream is healthy, keep using the proxy path.
4. If `mcp-proxy_retrieve_tools` is sparse, retry with a narrower IDE-specific query.
5. Fall back to non-IDE file/search tooling only after the proxy + IDE path is confirmed unavailable or insufficient.
6. When falling back, state that the fallback is required because the IDE path could not provide the needed result.

## Backend Test Execution Rule

Backend tests should be executed either through Rider run configurations or terminal test commands, depending on what is available and most reliable in the current environment.

Required behavior:

1. Prefer Rider run configurations when available and healthy.
2. If Rider run configuration execution is unavailable, run backend tests via terminal commands.
3. Always capture and report concrete test output for verification before claiming success.

## Script and Run Execution Routing

### Frontend scripts

All frontend `bun run` scripts must be executed through the `webstorm-bun-scripts` MCP server.

This includes:

- `bun run dev`
- `bun run build`
- `bun run lint`
- `bun run lint:fix`
- `bun run typecheck`
- `bun run generate-ts`

Do not run frontend `bun run` scripts from terminal commands.

### Project run configurations and backend tests

Project run configurations should be executed through Rider when available. Backend tests may run through Rider run configurations or terminal commands.

Required behavior:

1. Prefer Rider run configurations for project execution when available.
2. Execute backend tests through Rider run configurations when practical, or via terminal commands when needed.
3. Ensure test command/configuration output is captured and checked before completion claims.

## Discovery Retry Rule

When discovery is weak, retry in smaller steps instead of broad natural language.

Good retry patterns:

- Search by server and action: `rider get file text by path`
- Search by exact concept: `github repository file contents`
- Search by narrower task: `webstorm search in files text`
- Search for one tool family at a time instead of a long mixed query

Do not conclude that a server is missing after one poor search result.

## Context7

For library or API documentation in Reaparr:

1. Use Context7 through mcpproxy.
2. Resolve the library ID first when needed.
3. Fetch the smallest targeted documentation needed.
4. Summarize the result instead of dumping large responses.

## GitHub and Remote API Safety

For GitHub and any other remote API exposed through the proxy:

1. Default to read-only operations.
2. If a write is requested, do a dry-run or read-first pass when possible.
3. Never perform destructive remote actions unless the user explicitly asks for them and the risk is clear.
4. Do not use a remote API write when a safe local workspace edit path is available and more appropriate.

## Truncation, Cache, and Quarantine

### Truncated responses

If a proxy response says it was truncated, continue with `mcp-proxy_read_cache` using the provided cache key.

### Quarantined servers or tools

If a needed server or tool is quarantined, inspect it with `mcp-proxy_quarantine_security` before assuming it is unavailable.

### Upstream status

Use `mcp-proxy_upstream_servers` for:

- checking connected servers
- confirming health
- confirming whether a relevant upstream exists

## Reaparr Examples

### Backend inspection example

1. Check `mcp-proxy_upstream_servers` for `rider-official` or `rider-index` health.
2. Run `mcp-proxy_retrieve_tools` with a narrow Rider query.
3. Use the discovered read tool through `mcp-proxy_call_tool_read`.
4. Use Rider diagnostics before claiming backend changes are valid.

### Frontend inspection example

1. Check `mcp-proxy_upstream_servers` for `webstorm-official` or `webstorm-index`.
2. Narrow discovery to a frontend-only query.
3. Use WebStorm-backed reads/diagnostics first.
4. Run broader frontend verification only if IDE diagnostics are insufficient.

### Backend test execution example

1. Execute tests through Rider run configurations when available.
2. If Rider execution is unavailable or unreliable, run backend test commands in the terminal.
3. Capture output and verify pass/fail results before reporting completion.

### Frontend script execution example

1. Use `webstorm-bun-scripts` MCP tools for all `bun run` scripts.
2. Run `dev`, `build`, `lint`, `lint:fix`, `typecheck`, and `generate-ts` through MCP, not terminal.

### GitHub read example

1. Discover GitHub tools with `mcp-proxy_retrieve_tools`.
2. Use `github:get_file_contents` through `mcp-proxy_call_tool_read` to inspect repo files.
3. Use `intent_reason` that explains the current repo task.

### Context7 example

1. Resolve the library ID.
2. Query only the specific API area needed.
3. Return the answer in a concise summary tied to the current code task.

## Common Mistakes

- Guessing tool names instead of discovering them
- Skipping `mcp-proxy_retrieve_tools`
- Using the wrong call variant after discovery
- Treating one sparse search result as proof the server is unavailable
- Falling back to grep, glob, read, or bash too early
- Forgetting `intent_reason` or `intent_data_sensitivity`
- Using Rider for frontend work or WebStorm for backend work
- Missing `project_path` in Rider/WebStorm calls
- Skipping Rider run configurations when they are available and appropriate
- Running frontend `bun run` scripts in terminal instead of `webstorm-bun-scripts`
- Claiming backend test success without captured output from Rider run configuration or terminal test commands
- Performing remote writes when a safer local workspace edit path is available
