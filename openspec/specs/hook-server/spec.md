---
name: hook-server
status: proposed
created: 2026-05-18
---

# Spec: Embedded Hook HTTP Server

## Capability

A lightweight HTTP server embedded in the Pet App that receives webhook POST requests from Claude Code's hooks system and routes them to the pet state machine and notification system.

## Server Configuration

| Property    | Value                           |
|-------------|---------------------------------|
| Host        | localhost (127.0.0.1)          |
| Default Port| 12345                           |
| Fallback    | Try 12346-12350 if port busy    |
| Protocol    | HTTP (no TLS needed for localhost) |
| Framework   | ASP.NET Core Minimal API (Kestrel) |

## Endpoints

| Method | Path                                | Maps To              |
|--------|-------------------------------------|----------------------|
| POST   | `/hooks/session-start`              | SessionStart         |
| POST   | `/hooks/prompt-submit`              | UserPromptSubmit     |
| POST   | `/hooks/pre-tool-use`               | PreToolUse           |
| POST   | `/hooks/post-tool-use`              | PostToolUse          |
| POST   | `/hooks/tool-failure`               | PostToolUseFailure   |
| POST   | `/hooks/notification/idle`          | Notification(idle)   |
| POST   | `/hooks/notification/permission`    | Notification(perm)   |
| POST   | `/hooks/stop`                       | Stop                 |
| POST   | `/hooks/stop-failure`               | StopFailure          |
| POST   | `/hooks/subagent-start`             | SubagentStart        |
| POST   | `/hooks/subagent-stop`              | SubagentStop         |
| POST   | `/hooks/session-end`                | SessionEnd           |
| GET    | `/health`                           | Health check         |

## Request / Response Contract

### Request

```
POST /hooks/prompt-submit HTTP/1.1
Content-Type: application/json

{
  "session_id": "...",
  "hook_event_name": "UserPromptSubmit",
  "input": { ... }
}
```

### Response

```
HTTP/1.1 200 OK
Content-Type: application/json

{ "status": "ok" }
```

- All endpoints return 200 on successful processing
- Invalid JSON returns 400
- Server errors return 500

## SessionStart Handling

SessionStart hooks only support `command` and `mcp_tool` types, not `http`. Solution:

1. Register a `command` hook that sends HTTP request to the server:
   ```json
   {
     "type": "command",
     "command": "curl -s -X POST http://localhost:12345/hooks/session-start -H 'Content-Type: application/json' -d '{\"session_id\":\"$CLAUDE_SESSION_ID\"}'"
   }
   ```
2. On Windows, fall back to `Invoke-WebRequest` if `curl` is not available
3. Server handles the POST the same way as other endpoints

## Port Discovery

1. On startup, attempt to bind to port 12345
2. If port is in use, try 12346, 12347, ..., 12350
3. Store the selected port in a local config file
4. `HookConfigService` reads the port and generates correct hook URLs
5. If hooks are already configured with a different port, update them

## Lifecycle

1. **App start**: HTTP server starts on background thread before UI shows
2. **App running**: Server processes requests, dispatches to UI thread
3. **App shutdown**: Graceful shutdown with 2s timeout for in-flight requests

## Health Check

`GET /health` returns `{ "status": "healthy", "port": 12345 }` for monitoring and debugging.

## Security Considerations

- Server binds to `localhost` only (not `0.0.0.0`) - no external access
- No authentication required (localhost-only trust boundary)
- Request body size limit: 1MB (hook payloads are small)
- Rate limiting: not needed for local single-user scenario
