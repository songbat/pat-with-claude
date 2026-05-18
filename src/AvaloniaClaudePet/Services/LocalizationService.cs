namespace AvaloniaClaudePet.Services;

public enum Language { English, Chinese }

public class LocalizationService
{
    private Language _current = Language.Chinese;

    public Language Current
    {
        get => _current;
        set { _current = value; LanguageChanged?.Invoke(); }
    }

    public event Action? LanguageChanged;

    private static readonly Dictionary<string, (string En, string Zh)> StatusTexts = new()
    {
        ["analyzing"] = ("Analyzing...", "分析中..."),
        ["processing"] = ("Processing...", "处理中..."),
        ["delegating"] = ("Delegating...", "委派任务..."),
        ["done"] = ("Done!", "完成!"),
        ["failed"] = ("Failed!", "失败!"),
        ["waiting"] = ("Waiting for you...", "等你回来..."),
        ["approval"] = ("Need approval!", "需要审批!"),
        ["tool_prefix"] = ("", ""),
    };

    public string this[string key]
    {
        get
        {
            if (StatusTexts.TryGetValue(key, out var val))
                return _current == Language.English ? val.Item1 : val.Item2;
            return "";
        }
    }

    public string Toggle()
    {
        _current = _current == Language.English ? Language.Chinese : Language.English;
        LanguageChanged?.Invoke();
        return _current == Language.English ? "English" : "中文";
    }

    public string CurrentLabel => _current == Language.English ? "EN/中" : "中/EN";
}
