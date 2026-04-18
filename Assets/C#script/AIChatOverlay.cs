using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// AI 对话覆盖层（运行时 OnGUI）。
/// - 优点：不依赖现有 Unity Scene 里是否已经有 TMP_InputField/对话框 Prefab
/// - 你只需要把这个脚本挂到任意一个场景物体上即可开始使用
/// </summary>
public class AIChatOverlay : MonoBehaviour
{
    [Header("后端 AI 接口（由你自行实现）")]
    [Tooltip("示例：POST 一个 JSON，请求体形如 {\"message\":\"...\"}，后端返回纯文本或 JSON。")]
    public string apiUrl = "http://localhost:8080/api/ai/chat";

    [Header("UI/交互")]
    [Tooltip("显示/隐藏覆盖层的热键。")]
    public KeyCode toggleKey = KeyCode.F1;
    public int maxMessages = 20;
    public int requestTimeoutSeconds = 15;

    private bool isVisible = true;
    private string inputText = "";
    private Vector2 scrollPos = Vector2.zero;

    // 简单消息记录（用于展示；请求时也可以一起发给后端）
    private readonly List<ChatMessage> messages = new List<ChatMessage>();
    private bool isSending = false;

    private void Start()
    {
        // 给一个提示，避免一片空白
        messages.Add(new ChatMessage("system", "AI 对话已就绪。输入内容并点击 Send（或按 Enter）。"));
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            isVisible = !isVisible;
    }

    private void OnGUI()
    {
        if (!isVisible) return;

        // 覆盖层整体布局
        const int padding = 10;
        const int w = 520;
        const int h = 420;

        Rect windowRect = new Rect(padding, padding, w, h);
        GUI.Box(windowRect, "AI Chat");

        // 让滚动区避开外框
        Rect innerRect = new Rect(windowRect.x + padding, windowRect.y + 35, windowRect.width - padding * 2, windowRect.height - 90);

        // 顶部：API URL（方便不进 Inspector 就能改）
        Rect urlRect = new Rect(windowRect.x + padding, windowRect.y + 22, windowRect.width - padding * 2, 20);
        apiUrl = GUI.TextField(urlRect, apiUrl);

        // 消息列表滚动区
        GUI.BeginGroup(innerRect);
        scrollPos = GUI.BeginScrollView(new Rect(0, 0, innerRect.width, innerRect.height), scrollPos, new Rect(0, 0, innerRect.width - 20, 1000));

        float y = 0f;
        for (int i = 0; i < messages.Count; i++)
        {
            ChatMessage m = messages[i];
            string line = $"[{m.role}] {m.text}";

            // 用多行文本简化渲染（展示为纯文本）
            GUI.Label(new Rect(0, y, innerRect.width - 20, 22), line);
            y += 22f;
        }

        GUI.EndScrollView();
        GUI.EndGroup();

        // 底部：输入框 + 发送按钮
        Rect inputRect = new Rect(windowRect.x + padding, windowRect.y + windowRect.height - 45, windowRect.width - 120, 25);
        inputText = GUI.TextField(inputRect, inputText);

        // 按 Enter 发送（TextField 通常会吃掉 Enter，这里用一个兜底处理）
        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            TrySend();
        }

        Rect btnRect = new Rect(windowRect.x + windowRect.width - padding - 90, windowRect.y + windowRect.height - 45, 90, 25);
        bool enabled = !isSending;
        GUI.enabled = enabled;
        if (GUI.Button(btnRect, isSending ? "Sending..." : "Send"))
        {
            TrySend();
        }
        GUI.enabled = true;

        if (string.IsNullOrEmpty(apiUrl))
        {
            GUI.Label(new Rect(windowRect.x + padding, windowRect.y + windowRect.height - 20, windowRect.width - padding * 2, 20), "apiUrl 不能为空");
        }
    }

    private void TrySend()
    {
        if (isSending) return;
        string text = (inputText ?? "").Trim();
        if (string.IsNullOrEmpty(text)) return;

        inputText = "";
        messages.Add(new ChatMessage("user", text));
        TrimMessages();

        StartCoroutine(SendChatCoroutine(text));
    }

    private IEnumerator SendChatCoroutine(string userText)
    {
        if (string.IsNullOrEmpty(apiUrl))
        {
            messages.Add(new ChatMessage("system", "请先设置 apiUrl"));
            TrimMessages();
            yield break;
        }

        isSending = true;
        messages.Add(new ChatMessage("system", "请求中..."));
        TrimMessages();

        // 这里约定请求体最小字段：message
        // 你也可以在后端兼容 history 字段（如果你愿意的话）
        string requestJson = BuildRequestJson(userText);

        using (var req = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(requestJson);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            req.timeout = requestTimeoutSeconds;

            yield return req.SendWebRequest();

            string replyText;
            if (req.result != UnityWebRequest.Result.Success)
            {
                replyText = "请求失败：" + req.error;
            }
            else
            {
                replyText = ExtractReply(req.downloadHandler.text);
            }

            // 替换“请求中...”那条（简单做法：把最后一条 system 如果是请求中就删）
            if (messages.Count > 0 && messages[messages.Count - 1].role == "system" && messages[messages.Count - 1].text.Contains("请求中"))
                messages.RemoveAt(messages.Count - 1);

            messages.Add(new ChatMessage("ai", replyText));
            TrimMessages();
        }

        isSending = false;
    }

    private string BuildRequestJson(string userText)
    {
        // history 仅用于展示/增强上下文，你的后端可以忽略
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append("\"message\":\"").Append(EscapeJson(userText)).Append("\"");
        sb.Append(",\"history\":[");

        // 发送最近若干条历史，避免请求太长
        int start = Mathf.Max(0, messages.Count - 8);
        for (int i = start; i < messages.Count; i++)
        {
            ChatMessage m = messages[i];
            sb.Append("{\"role\":\"").Append(EscapeJson(m.role)).Append("\",\"text\":\"").Append(EscapeJson(m.text)).Append("\"}");
            if (i != messages.Count - 1) sb.Append(",");
        }

        sb.Append("]}");
        return sb.ToString();
    }

    private void TrimMessages()
    {
        // 保证不会无限增长
        while (messages.Count > maxMessages)
            messages.RemoveAt(0);
    }

    private string ExtractReply(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
            return "(空回复)";

        string t = responseText.Trim();

        // 如果返回的是纯文本（不是 JSON），直接用
        if (!t.StartsWith("{") && !t.StartsWith("["))
            return t;

        // JSON 情况：尝试提取常见字段
        // 兼容 { "reply": "..." } 或 { "message": "..." } 或 { "data": { "reply": "..." } }
        string reply = TryExtractJsonString(t, "reply");
        if (!string.IsNullOrEmpty(reply)) return reply;

        reply = TryExtractJsonString(t, "message");
        if (!string.IsNullOrEmpty(reply)) return reply;

        // 兜底：返回整个 JSON
        return t;
    }

    private string TryExtractJsonString(string json, string key)
    {
        // 简单 JSON 字符串提取（不做完整 JSON 解析，避免依赖第三方库）
        // 匹配形如 "key":"value"
        string pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"";
        var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return null;

        string raw = match.Groups[1].Value;
        return UnescapeJsonString(raw);
    }

    private string UnescapeJsonString(string raw)
    {
        // 常见转义处理：\n \t \" \\ 
        // 注意：这是“够用型”，不追求覆盖所有 JSON 转义边角情况
        return raw
            .Replace("\\n", "\n")
            .Replace("\\t", "\t")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\")
            .Replace("\\r", "\r");
    }

    private string EscapeJson(string s)
    {
        if (s == null) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private sealed class ChatMessage
    {
        public readonly string role;
        public readonly string text;

        public ChatMessage(string role, string text)
        {
            this.role = role;
            this.text = text;
        }
    }
}

