using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using log4net;

namespace Plus.HabboHotel.Camera
{
    public class CameraManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CameraManager));

        private readonly ConcurrentDictionary<int, PendingPhoto> _pendingPhotos = new();
        private readonly ConcurrentDictionary<int, long> _lastRender = new();
        private readonly ConcurrentDictionary<int, int> _lastPublish = new();

        #region Settings accessors

        private static string Setting(string key, string fallback)
        {
            return PlusEnvironment.GetSettingsManager().ContainsKey(key)
                ? PlusEnvironment.GetSettingsManager().TryGetValue(key)
                : fallback;
        }

        private static int SettingInt(string key, int fallback)
        {
            return int.TryParse(Setting(key, fallback.ToString()), out int value) ? value : fallback;
        }

        public bool IsEnabled => Setting("camera.enabled", "1") == "1";
        public int PurchasePrice => Math.Max(0, SettingInt("camera.price.credits", 10));
        public int PublishPrice => Math.Max(0, SettingInt("camera.price.credits.publish", 10));
        public int PublishCooldownSeconds => Math.Max(0, SettingInt("camera.publish.cooldown", 60));
        public int RenderCooldownMs => Math.Max(0, SettingInt("camera.render.cooldown.ms", 2000));
        public int MaxPhotoBytes => Math.Clamp(SettingInt("camera.photo.maxbytes", 320000), 1024, 450000);
        public int MaxPhotoDimension => Math.Clamp(SettingInt("camera.photo.maxdimension", 1024), 32, 4096);
        public int PhotoSize => Math.Clamp(SettingInt("camera.photo.size", 320), 32, 1024);
        public int PurchaseCooldownMs => Math.Max(0, SettingInt("camera.purchase.cooldown.ms", 1000));
        public int PhotoItemId => SettingInt("camera.item.id", 0);

        public string PhotoPath => Setting("camera.path", Path.Combine(AppContext.BaseDirectory, "camera"));
        public string ThumbnailPath => Setting("camera.thumbnail.path", Path.Combine(AppContext.BaseDirectory, "camera", "thumbnails"));
        public string PhotoUrl => Setting("camera.url", "").TrimEnd('/');

        #endregion

        #region Rate limiting / cooldowns

        private readonly object _renderLock = new();

        public bool TryConsumeRenderSlot(int userId)
        {
            lock (_renderLock) {
                long now = Environment.TickCount64;

                if (_lastRender.TryGetValue(userId, out long last) && now - last < RenderCooldownMs)
                    return false;

                _lastRender[userId] = now;
                return true;
            }
        }

        private readonly ConcurrentDictionary<int, long> _lastPurchase = new();
        private readonly object _purchaseLock = new();

        public bool TryConsumePurchaseSlot(int userId)
        {
            lock (_purchaseLock) {
                long now = Environment.TickCount64;

                if (_lastPurchase.TryGetValue(userId, out long last) && now - last < PurchaseCooldownMs)
                    return false;

                _lastPurchase[userId] = now;
                return true;
            }
        }

        public int GetPublishWait(int userId)
        {
            if (!_lastPublish.TryGetValue(userId, out int last))
                return 0;

            int elapsed = (int)PlusEnvironment.GetUnixTimestamp() - last;
            return elapsed >= PublishCooldownSeconds ? 0 : PublishCooldownSeconds - elapsed;
        }

        public void StampPublish(int userId)
        {
            _lastPublish[userId] = (int)PlusEnvironment.GetUnixTimestamp();
        }

        #endregion

        #region Pending photo state

        public void SetPendingPhoto(PendingPhoto photo)
        {
            _pendingPhotos[photo.UserId] = photo;
        }

        public PendingPhoto GetPendingPhoto(int userId)
        {
            return _pendingPhotos.TryGetValue(userId, out PendingPhoto photo) ? photo : null;
        }

        public void ClearUser(int userId)
        {
            _pendingPhotos.TryRemove(userId, out _);
            _lastRender.TryRemove(userId, out _);
        }

        #endregion

        #region Disk IO

        public static string GenerateToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();
        }

        public static bool SavePng(string directory, string filename, byte[] pngBytes)
        {
            try {
                Directory.CreateDirectory(directory);

                string fullDir = Path.GetFullPath(directory);
                string fullPath = Path.GetFullPath(Path.Combine(fullDir, filename));

                if (!fullPath.StartsWith(fullDir + Path.DirectorySeparatorChar, StringComparison.Ordinal)) {
                    Log.Warn("[Camera] Refused to write outside the camera directory: " + fullPath);
                    return false;
                }

                File.WriteAllBytes(fullPath, pngBytes);
                return true;
            } catch (Exception e) {
                Log.Error("[Camera] Failed to save png: " + e.Message);
                return false;
            }
        }

        #endregion
    }
}