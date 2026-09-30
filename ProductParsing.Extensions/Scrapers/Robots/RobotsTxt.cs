using System.Text.RegularExpressions;

namespace ProductParsing.Extensions.Scrapers.Robots
{
    public class RobotsTxt
    {
        private readonly List<Rule> _rules;

        private RobotsTxt(List<Rule> rules)
        {
            _rules = rules;
        }

        public static RobotsTxt AllowAll { get; } = new([]);

        public static RobotsTxt DisallowAll { get; } = new([new Rule(false, "/")]);

        public static RobotsTxt Parse(string content, string userAgent)
        {
            var groups = ReadGroups(content);

            var own = groups.Where(g => g.Agents.Any(a => string.Equals(a, userAgent, StringComparison.OrdinalIgnoreCase))).ToList();
            var selected = own.Count > 0 ? own : groups.Where(g => g.Agents.Contains("*")).ToList();

            return new RobotsTxt(selected.SelectMany(g => g.Rules).ToList());
        }

        public bool IsAllowed(Uri url)
        {
            var path = url.PathAndQuery;

            if (path == "/robots.txt")
            {
                return true;
            }

            Rule? best = null;

            foreach (var rule in _rules.Where(r => r.Matches(path)))
            {
                if (best is null || rule.Pattern.Length > best.Pattern.Length || (rule.Pattern.Length == best.Pattern.Length && rule.Allow))
                {
                    best = rule;
                }
            }

            return best?.Allow ?? true;
        }

        private static List<Group> ReadGroups(string content)
        {
            var groups = new List<Group>();
            Group? current = null;
            var lastWasAgent = false;

            foreach (var rawLine in content.Split('\n'))
            {
                var line = rawLine.Split('#')[0].Trim();
                var colon = line.IndexOf(':');

                if (colon <= 0)
                {
                    continue;
                }

                var key = line[..colon].Trim().ToLowerInvariant();
                var value = line[(colon + 1)..].Trim();

                if (key == "user-agent")
                {
                    if (current is null || !lastWasAgent)
                    {
                        current = new Group();
                        groups.Add(current);
                    }

                    current.Agents.Add(value);
                    lastWasAgent = true;
                    continue;
                }

                lastWasAgent = false;

                if (current is not null && (key == "allow" || key == "disallow") && value.Length > 0)
                {
                    current.Rules.Add(new Rule(key == "allow", value));
                }
            }

            return groups;
        }

        private class Group
        {
            public List<string> Agents { get; } = [];
            public List<Rule> Rules { get; } = [];
        }

        private class Rule(bool allow, string pattern)
        {
            private readonly Regex _regex = ToRegex(pattern);

            public bool Allow { get; } = allow;
            public string Pattern { get; } = pattern;

            public bool Matches(string path)
            {
                return _regex.IsMatch(path);
            }

            private static Regex ToRegex(string pattern)
            {
                var endAnchor = pattern.EndsWith('$');
                var body = endAnchor ? pattern[..^1] : pattern;
                var regex = "^" + string.Join(".*", body.Split('*').Select(Regex.Escape)) + (endAnchor ? "$" : "");

                return new Regex(regex, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
