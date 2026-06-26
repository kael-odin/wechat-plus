using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public sealed class QuickReplyRepository
    {
        private readonly string _dataRoot;
        private readonly string _categoriesPath;
        private readonly string _repliesPath;
        private readonly JavaScriptSerializer _serializer;

        public QuickReplyRepository(string dataRoot)
        {
            _dataRoot = dataRoot;
            _categoriesPath = Path.Combine(_dataRoot, "reply_categories.json");
            _repliesPath = Path.Combine(_dataRoot, "quick_replies.json");
            _serializer = new JavaScriptSerializer();
        }

        public void EnsureSeedData()
        {
            AppPaths.EnsureDirectory(_dataRoot);

            if (!File.Exists(_categoriesPath))
            {
                SaveCategories(CreateDefaultCategories());
            }

            if (!File.Exists(_repliesPath))
            {
                SaveReplies(CreateDefaultReplies());
            }
        }

        public ReplyCategory[] GetCategories()
        {
            EnsureSeedData();
            return LoadArray<ReplyCategory>(_categoriesPath);
        }

        public QuickReply[] GetAllReplies()
        {
            EnsureSeedData();
            return LoadArray<QuickReply>(_repliesPath);
        }

        public QuickReply[] Search(string keyword)
        {
            QuickReply[] replies = GetAllReplies();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return replies.OrderBy(x => x.SortOrder).ThenBy(x => x.Title).ToArray();
            }

            string normalized = keyword.Trim();
            return replies
                .Where(x => Contains(x.Title, normalized) || Contains(x.Content, normalized) || Contains(x.Tags, normalized))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Title)
                .ToArray();
        }

        public void SaveReply(QuickReply reply)
        {
            if (reply == null)
            {
                throw new ArgumentNullException("reply");
            }

            List<QuickReply> replies = new List<QuickReply>(GetAllReplies());
            QuickReply existing = replies.FirstOrDefault(x => string.Equals(x.Id, reply.Id, StringComparison.OrdinalIgnoreCase));
            DateTime now = DateTime.UtcNow;

            if (existing == null)
            {
                if (string.IsNullOrEmpty(reply.Id))
                {
                    reply.Id = Guid.NewGuid().ToString("N");
                }
                reply.CreatedAtUtc = now;
                reply.UpdatedAtUtc = now;
                replies.Add(reply);
            }
            else
            {
                existing.Title = reply.Title;
                existing.Content = reply.Content;
                existing.CategoryId = reply.CategoryId;
                existing.Tags = reply.Tags;
                existing.SortOrder = reply.SortOrder;
                existing.IsFavorite = reply.IsFavorite;
                existing.UpdatedAtUtc = now;
            }

            SaveReplies(replies.ToArray());
        }

        public string ExportJson()
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["categories"] = GetCategories();
            payload["quickReplies"] = GetAllReplies();
            return _serializer.Serialize(payload);
        }

        public void ImportReplies(QuickReply[] importedReplies)
        {
            if (importedReplies == null)
            {
                return;
            }

            foreach (QuickReply reply in importedReplies)
            {
                SaveReply(reply);
            }
        }

        public int ImportJson(string json, bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return 0;
            }

            Dictionary<string, object> payload = _serializer.Deserialize<Dictionary<string, object>>(json);
            if (payload == null || !payload.ContainsKey("quickReplies"))
            {
                return 0;
            }

            string repliesJson = _serializer.Serialize(payload["quickReplies"]);
            QuickReply[] replies = _serializer.Deserialize<QuickReply[]>(repliesJson) ?? new QuickReply[0];
            return ImportReplies(replies, overwrite);
        }

        public int ImportCsv(string csv, bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                return 0;
            }

            string[] lines = csv.Replace("\r\n", "\n").Replace('\r', '\n').Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1)
            {
                return 0;
            }

            List<QuickReply> imported = new List<QuickReply>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = SplitCsvLine(lines[i]);
                if (columns.Length < 2)
                {
                    continue;
                }

                QuickReply reply = new QuickReply();
                reply.Id = Guid.NewGuid().ToString("N");
                reply.Title = columns[0];
                reply.Content = columns[1];
                reply.CategoryId = columns.Length > 2 && !string.IsNullOrEmpty(columns[2]) ? columns[2] : "common";
                reply.Tags = columns.Length > 3 ? columns[3] : columns[0];
                reply.SortOrder = 500 + imported.Count;
                imported.Add(reply);
            }

            return ImportReplies(imported.ToArray(), overwrite);
        }

        private int ImportReplies(QuickReply[] importedReplies, bool overwrite)
        {
            if (importedReplies == null || importedReplies.Length == 0)
            {
                return 0;
            }

            List<QuickReply> existing = new List<QuickReply>(GetAllReplies());
            int changed = 0;
            DateTime now = DateTime.UtcNow;

            foreach (QuickReply imported in importedReplies)
            {
                if (imported == null || string.IsNullOrWhiteSpace(imported.Title))
                {
                    continue;
                }

                QuickReply match = existing.FirstOrDefault(x => string.Equals(x.Title, imported.Title, StringComparison.CurrentCultureIgnoreCase));
                if (match != null && !overwrite)
                {
                    continue;
                }

                if (match == null)
                {
                    imported.Id = string.IsNullOrEmpty(imported.Id) ? Guid.NewGuid().ToString("N") : imported.Id;
                    imported.CreatedAtUtc = now;
                    imported.UpdatedAtUtc = now;
                    existing.Add(imported);
                }
                else
                {
                    match.Content = imported.Content;
                    match.CategoryId = string.IsNullOrEmpty(imported.CategoryId) ? match.CategoryId : imported.CategoryId;
                    match.Tags = imported.Tags;
                    match.SortOrder = imported.SortOrder;
                    match.IsFavorite = imported.IsFavorite;
                    match.UpdatedAtUtc = now;
                }
                changed++;
            }

            SaveReplies(existing.ToArray());
            return changed;
        }

        private ReplyCategory[] CreateDefaultCategories()
        {
            return new[]
            {
                Category("common", "常用", "text", 1),
                Category("welcome", "欢迎", "text", 2),
                Category("product", "商品推荐", "text", 3),
                Category("comfort", "用户安抚", "text", 4),
                Category("review", "邀约好评", "text", 5),
                Category("closing", "结束语", "text", 6),
                Category("favorite", "收藏", "text", 7)
            };
        }

        private QuickReply[] CreateDefaultReplies()
        {
            DateTime now = DateTime.UtcNow;
            return new[]
            {
                Reply("welcome-1", "自我介绍", "您好，欢迎咨询！我是客服，很高兴为您服务。", "welcome", "欢迎,自我介绍", 1, now),
                Reply("common-1", "稍等", "请稍等，我马上帮您确认。", "common", "常用,等待", 2, now),
                Reply("product-1", "商品推荐", "根据您提供的信息，我推荐您先看这款方案。", "product", "商品,推荐", 3, now),
                Reply("comfort-1", "用户安抚", "非常抱歉给您带来不便，我会尽快协助处理。", "comfort", "售后,安抚", 4, now),
                Reply("review-1", "邀约好评", "如果这次服务对您有帮助，欢迎给我们一个好评。", "review", "好评,邀约", 5, now),
                Reply("closing-1", "结束语", "感谢您的咨询，后续有需要可以随时联系我。", "closing", "结束,感谢", 6, now)
            };
        }

        private static ReplyCategory Category(string id, string name, string type, int sortOrder)
        {
            return new ReplyCategory { Id = id, Name = name, Type = type, SortOrder = sortOrder };
        }

        private static QuickReply Reply(string id, string title, string content, string categoryId, string tags, int sortOrder, DateTime now)
        {
            return new QuickReply
            {
                Id = id,
                Title = title,
                Content = content,
                CategoryId = categoryId,
                Tags = tags,
                SortOrder = sortOrder,
                IsFavorite = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
        }

        private T[] LoadArray<T>(string path)
        {
            if (!File.Exists(path))
            {
                return new T[0];
            }
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new T[0];
            }
            return _serializer.Deserialize<T[]>(json) ?? new T[0];
        }

        private void SaveCategories(ReplyCategory[] categories)
        {
            File.WriteAllText(_categoriesPath, _serializer.Serialize(categories));
        }

        private void SaveReplies(QuickReply[] replies)
        {
            File.WriteAllText(_repliesPath, _serializer.Serialize(replies));
        }

        private static bool Contains(string value, string keyword)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static string[] SplitCsvLine(string line)
        {
            List<string> values = new List<string>();
            bool inQuotes = false;
            System.Text.StringBuilder current = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Length = 0;
                }
                else
                {
                    current.Append(ch);
                }
            }

            values.Add(current.ToString());
            return values.ToArray();
        }
    }
}
