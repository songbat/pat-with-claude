using System.Text.Json;
using System.Text.Json.Nodes;

namespace AvaloniaClaudePet.Services;

public class HookConfigService
{
    private readonly int _port;
    private readonly string _settingsPath;

    public HookConfigService(int port)
    {
        _port = port;
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var claudeDir = Path.Combine(homeDir, ".claude");
        Directory.CreateDirectory(claudeDir);
        _settingsPath = Path.Combine(claudeDir, "settings.json");
    }

    public bool IsConfigured()
    {
        if (!File.Exists(_settingsPath)) return false;

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var node = JsonNode.Parse(json);
            var hooks = node?["hooks"];
            return hooks != null && hooks.AsObject().Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public void ConfigureHooks()
    {
        JsonObject root;

        if (File.Exists(_settingsPath))
        {
            var existing = File.ReadAllText(_settingsPath);
            root = JsonNode.Parse(existing)?.AsObject() ?? new JsonObject();
        }
        else
        {
            root = new JsonObject();
        }

        var hooks = GenerateHooksConfig();
        root["hooks"] = hooks;

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_settingsPath, root.ToJsonString(options));
    }

    private JsonObject GenerateHooksConfig()
    {
        var baseUrl = $"http://localhost:{_port}";
        var isWindows = OperatingSystem.IsWindows();
        var curlCmd = isWindows
            ? $"powershell -Command \"Invoke-WebRequest -Uri '{baseUrl}/hooks/session-start' -Method POST -ContentType 'application/json' -Body ('{{\\\"session_id\\\":\\\"' + $env:CLAUDE_SESSION_ID + '\\\"}}')\""
            : $"curl -s -X POST {baseUrl}/hooks/session-start -H 'Content-Type: application/json' -d '{{\"session_id\":\"$CLAUDE_SESSION_ID\"}}'";

        return new JsonObject
        {
            ["SessionStart"] = new JsonArray
            {
                new JsonObject
                {
                    ["hooks"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "command",
                            ["command"] = curlCmd
                        }
                    }
                }
            },
            ["UserPromptSubmit"] = CreateHttpHookArray($"{baseUrl}/hooks/prompt-submit"),
            ["PreToolUse"] = CreateHttpHookArray($"{baseUrl}/hooks/pre-tool-use"),
            ["PostToolUse"] = CreateHttpHookArray($"{baseUrl}/hooks/post-tool-use"),
            ["PostToolUseFailure"] = CreateHttpHookArray($"{baseUrl}/hooks/tool-failure"),
            ["Notification"] = new JsonArray
            {
                new JsonObject
                {
                    ["matcher"] = "idle_prompt",
                    ["hooks"] = new JsonArray
                    {
                        new JsonObject { ["type"] = "http", ["url"] = $"{baseUrl}/hooks/notification/idle" }
                    }
                },
                new JsonObject
                {
                    ["matcher"] = "permission_prompt",
                    ["hooks"] = new JsonArray
                    {
                        new JsonObject { ["type"] = "http", ["url"] = $"{baseUrl}/hooks/notification/permission" }
                    }
                }
            },
            ["Stop"] = CreateHttpHookArray($"{baseUrl}/hooks/stop"),
            ["StopFailure"] = CreateHttpHookArray($"{baseUrl}/hooks/stop-failure"),
            ["SubagentStart"] = CreateHttpHookArray($"{baseUrl}/hooks/subagent-start"),
            ["SubagentStop"] = CreateHttpHookArray($"{baseUrl}/hooks/subagent-stop"),
            ["SessionEnd"] = CreateHttpHookArray($"{baseUrl}/hooks/session-end")
        };
    }

    private static JsonArray CreateHttpHookArray(string url) => new()
    {
        new JsonObject
        {
            ["hooks"] = new JsonArray
            {
                new JsonObject { ["type"] = "http", ["url"] = url }
            }
        }
    };
}
