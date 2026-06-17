using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Plus.Communication.Packets.Incoming
{
    internal class WebOverlayCallbackEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            // The overlay sends raw JSON: {"header":"edit_tv","data":{"itemId":5,"videoId":"dQw4w9WgXcQ"}}
            string payload = packet.PopString()
                .Replace("&#47;", "/"); // un-escape slashes

            if (string.IsNullOrWhiteSpace(payload))
                return;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(payload);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("header", out JsonElement headerEl))
                    return;

                string header = headerEl.GetString() ?? string.Empty;
                JsonElement data = root.TryGetProperty("data", out JsonElement d) ? d : default;

                DispatchMessage(header, data, session);
            }
            catch (Exception ex)
            {
                // Log and swallow — a malformed overlay message should never crash the emulator
                PlusEnvironment.GetGame().GetClientManager();
                // Optional: PlusEnvironment.GetLogging()?.LogException(ex);
                _ = ex;
            }
        }

        private static void DispatchMessage(string header, JsonElement data, GameClient session)
        {
            switch (header)
            {
                case "edit_tv":
                    HandleEditTv(data, session);
                    break;

                case "loaded":
                    HandleLoaded(session);
                    break;

                // ── Jukebox ───────────────────────────────────────────────
                case "add_song":
                    HandleAddSong(data, session);
                    break;

                case "next_song":
                    HandleNextSong(session);
                    break;

                case "prev_song":
                    HandlePrevSong(session);
                    break;

                case "play_stop":
                    HandlePlayStop(data, session);
                    break;

                case "remove_song":
                    HandleRemoveSong(data, session);
                    break;

                case "song_ended":
                    HandleSongEnded(session);
                    break;

                default:
                    // Unknown header – ignore silently
                    break;
            }
        }

        // ── Handler implementations ──────────────────────────────────────

        /// <summary>
        /// The user edited a YouTube TV furniture item via the overlay widget.
        /// Saves the new video ID against the furniture item and broadcasts to the room.
        /// Payload: { itemId: number, videoId: string }
        /// </summary>
        private static void HandleEditTv(JsonElement data, GameClient session)
        {
            if (session.GetHabbo()?.CurrentRoom == null) return;

            if (!data.TryGetProperty("itemId", out JsonElement itemIdEl)) return;
            if (!data.TryGetProperty("videoId", out JsonElement videoIdEl)) return;

            int itemId = itemIdEl.GetInt32();
            string videoId = videoIdEl.GetString() ?? string.Empty;

            var room = session.GetHabbo().CurrentRoom;
            var item = room.GetRoomItemHandler().GetItem(itemId);

            if (item == null) return;

            // Persist the new video and broadcast to everyone in the room so their
            // client-side TV furniture updates to the new video.
            item.ExtraData = videoId;
            item.UpdateState(true, true);
        }

        /// <summary>
        /// Overlay has finished loading. Send session data so the overlay can
        /// display the user's name, credits, etc.
        /// </summary>
        private static void HandleLoaded(GameClient session)
        {
            var habbo = session.GetHabbo();
            if (habbo == null) return;

            // Send session_data back to the overlay
            var payload = new
            {
                id = habbo.Id,
                username = habbo.Username,
                look = habbo.Look,
                credits = habbo.Credits
            };

            session.SendPacket(new Outgoing.Overlay.WebOverlayComposer("session_data", payload));
        }

        // ── Jukebox stubs (wire these up to your jukebox manager as needed) ──

        private static void HandleAddSong(JsonElement data, GameClient session)
        {
        }

        private static void HandleNextSong(GameClient session)
        {
        }

        private static void HandlePrevSong(GameClient session)
        {
        }

        private static void HandlePlayStop(JsonElement data, GameClient session)
        {
        }

        private static void HandleRemoveSong(JsonElement data, GameClient session)
        {
        }

        private static void HandleSongEnded(GameClient session)
        {
        }
    }
}
