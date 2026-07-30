using Newtonsoft.Json;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class LiveFeedService
    {
        // One shared client: a new HttpClient per webhook call leaks sockets (TIME_WAIT
        // exhaustion) under kill/crime spam.
        private static readonly HttpClient WebhookClient = new();

        public static void LiveFeed(string message, string type = "blue")
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            SaveToRedis(message, type);
            BubbleAlert(message, type);
        }

        public static string Name(string username, string color = "lightblue") =>
            "<b><font color=\"" + color + "\">" + username + "</font></b>";
        private static void SaveToRedis(string message, string type)
        {
            var redis = PlusEnvironment.GetRedis();
            if (redis == null || !redis.IsConnected)
                return;

            var entry = new
            {
                color = type,
                message = message,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string json = JsonConvert.SerializeObject(entry);
            // Fire-and-forget: a Redis hiccup must never stall the game thread that emitted
            // the feed event (kills/crimes come from the room cycle).
            redis.ListPushFireAndForget("livefeed", json);
        }

        private static void BubbleAlert(string message, string type)
        {
            // Emojis are stripped from the in-client bubble (the client font can't render them),
            // but the ORIGINAL message — emojis intact — is forwarded to Discord.
            string bubbleMessage = StripEmojis(message);

            BubbleAlertComposer bubbleAlertComposer = new("livefeed",
                new Dictionary<string, string>
                {
                    { "display", "BUBBLE" },
                    { "message", bubbleMessage },
                    { "image", "livefeed" }
                });

            foreach (RoomUser user in PlusEnvironment.GetGame().GetRoomManager().GetAllRoomUsers()) {
                if (user.IsBot)
                    continue;

                GameClient client = user.GetClient();
                if (client == null)
                    continue;

                // Respect the per-user "disable livefeed" setting.
                if (client.GetHabbo()?.GetRpSettings()?.Livefeed == false)
                    continue;

                client.SendPacket(bubbleAlertComposer);
            }

            _ = DiscordEmbed(message, type);
        }

        private static string StripEmojis(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            string cleaned = EmojiRegex.Replace(message, string.Empty);
            return Regex.Replace(cleaned, @"\s{2,}", " ").Trim();
        }

        // Surrogate pairs (astral-plane emoji like 💀), other symbols, symbol modifiers, the
        // variation selector + keycap combiner, arrows, dingbats and misc pictographs.
        private static readonly Regex EmojiRegex = new(
            "[\\p{Cs}\\p{So}\\p{Sk}️⃣←-⇿☀-➿⬀-⯿]",
            RegexOptions.Compiled);

        private static async Task DiscordEmbed(string message, string type)
        {
            try {
                string webhookUrl = "https://discord.com/api/webhooks/1522299501128192031/pz-zpxt761N6BshyiZ3_50G1nXNkgHRZS2FE3SpRCpOIhyKFOe3IqFRL1x_kDLn90B6f";
                string markdown = ConvertHtmlToDiscordMarkdown(message);

                int embedColor = type.ToLower() switch
                {
                    "blue" => 0x03A9F4,
                    "red" => 0xFF0000,
                    "green" => 0x00FF00,
                    "orange" => 0xFFA500,
                    "yellow" => 0xFFD700,
                    "purple" => 0x9C27B0,
                    "pink" => 0xE91E63,
                    "cyan" => 0x00BCD4,
                    "teal" => 0x009688,
                    "lime" => 0x8BC34A,
                    "indigo" => 0x3F51B5,
                    "gold" => 0xFFC107,
                    "brown" => 0x795548,
                    "gray" => 0x607D8B,
                    "black" => 0x000000,
                    "white" => 0xFFFFFF,

                    _ => 0x03A9F4
                };

                var payload = new
                {
                    allowed_mentions = new
                    {
                        parse = Array.Empty<string>()
                    },
                    embeds = new[]
                    {
                        new
                        {
                            description = markdown,
                            color = embedColor,
                            footer = new
                            {
                                text = $"Wave Plus • {DateTime.Now:MMM d, yyyy, h:mm tt}"
                            }
                        }
                }
                };

                string json = JsonConvert.SerializeObject(payload);

                await WebhookClient.PostAsync(webhookUrl, new StringContent(json, Encoding.UTF8, "application/json"));
            } catch (Exception ex) {
                Console.WriteLine($"Error while sending live feed webhook message: {ex}");
            }
        }

        private static string ConvertHtmlToDiscordMarkdown(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            html = Regex.Replace(html, "<span style=\"color: (#[0-9a-fA-F]{6});\">(.*?)</span>",
                match =>
                {
                    string color = match.Groups[1].Value.ToLower();
                    string text = match.Groups[2].Value;

                    return color switch
                    {
                        "#00ff00" => $"**{text}**",
                        "#0099ff" => $"*{text}*",
                        "#ff0000" => $"~~{text}~~",
                        _ => text
                    };
                }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            html = Regex.Replace(html, "<b>(.*?)</b>", "**$1**",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            html = Regex.Replace(html, "<i>(.*?)</i>", "*$1*",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            html = Regex.Replace(html, "<u>(.*?)</u>", "__$1__",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            html = Regex.Replace(html, "<(s|strike)>(.*?)</(s|strike)>", "~~$2~~",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            html = Regex.Replace(html, "<br\\s*/?>", "\n",
                RegexOptions.IgnoreCase);

            html = System.Net.WebUtility.HtmlDecode(html);

            html = Regex.Replace(html, "<.*?>", "");

            return html.Trim();
        }
    }
}