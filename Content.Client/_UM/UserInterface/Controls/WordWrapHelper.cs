using System.Linq;
using System.Text.RegularExpressions;

namespace Content.Client._UM.UserInterface.Controls;

public sealed class WordWrapHelper
{
    private const int DefaultLineWidth = 80;

    /// <summary>
    /// Wordwraps a markdown string, rebuilding tags on each line to maintain proper formatting.
    /// Tags are not counted toward the line width limit.
    /// </summary>
    public static List<string> Wordwrap(string input, int lineWidth = DefaultLineWidth)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        var tokens = ParseTokens(input);

        var lines = new List<string>();
        var currentLine = new List<string>();
        var openTags = new Stack<string>();
        int contentLength = 0;

        foreach (var token in tokens)
        {
            if (token.IsTag)
            {
                // Handle opening/closing tags
                if (token.IsClosingTag)
                {
                    openTags.Pop();
                }
                else
                {
                    openTags.Push(token.Content);
                }

                currentLine.Add(token.Content);
            }
            else
            {
                int wordLength = token.Content.Length;

                if (contentLength + wordLength + (contentLength > 0 ? 1 : 0) > lineWidth && currentLine.Count > 0)
                {
                    var closingTags = CloseAllTags(openTags);

                    foreach (var tag in closingTags)
                    {
                        currentLine.Add(tag);
                    }

                    lines.Add(string.Join("", currentLine));
                    currentLine.Clear();
                    contentLength = 0;

                    foreach (var tag in openTags)
                    {
                        currentLine.Add(tag);
                    }
                }

                if (contentLength > 0)
                {
                    currentLine.Add(" ");
                    contentLength += 1;
                }

                currentLine.Add(token.Content);
                contentLength += wordLength;
            }
        }

        if (currentLine.Count > 0)
        {
            var closingTags = CloseAllTags(openTags);
            foreach (var tag in closingTags)
            {
                currentLine.Add(tag);
            }

            lines.Add(string.Join("", currentLine));
        }

        return lines;
    }

    private static List<Token> ParseTokens(string input)
    {
        var tokens = new List<Token>();
        var pattern = @"(\[/?[^\]]+\]|[^\[\s]+|\s+)";
        var matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            var content = match.Value;

            if (content.StartsWith("[") && content.EndsWith("]"))
            {
                var isClosing = content.StartsWith("[/");
                tokens.Add(new Token
                {
                    Content = content,
                    IsTag = true,
                    IsClosingTag = isClosing
                });
            }
            else if (!string.IsNullOrWhiteSpace(content))
            {
                tokens.Add(new Token { Content = content.Trim(), IsTag = false });
            }
        }

        return tokens;
    }

    private static List<string> CloseAllTags(Stack<string> openTags)
    {
        var closingTags = new List<string>();
        foreach (var tag in openTags.ToList())
        {
            var tagName = ExtractTagName(tag);
            closingTags.Add($"[/{tagName}]");
        }
        return closingTags;
    }

    private static string ExtractTagName(string tag)
    {
        // Extract tag name from [tagname] or [tagname=value]
        var match = Regex.Match(tag, @"\[(/?)([^\]=\s]+)");
        return match.Success ? match.Groups[2].Value : "";
    }

    private sealed class Token
    {
        public required string Content { get; set; }
        public bool IsTag { get; set; }
        public bool IsClosingTag { get; set; }
    }
}
