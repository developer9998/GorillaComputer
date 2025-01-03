using GorillaComputer.Utilities;
using GorillaNetworking;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace GorillaComputer.Tools
{
    internal static class AssetLoader
    {
        public static string WallpaperPath => Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "Wallpaper.png");

        private static bool _bundleLoaded;
        private static AssetBundle _storedBundle;

        private static Task _loadingTask = null;
        private static readonly Dictionary<string, Object> _assetCache = [];

        private static async Task LoadBundle()
        {
            Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream("GorillaComputer.Content.terminalbundle");
            var bundleLoadRequest = AssetBundle.LoadFromStreamAsync(stream);

            // AssetBundleCreateRequest is a YieldInstruction !!
            await YieldUtils.YieldInstructionAsync(bundleLoadRequest);

            _storedBundle = bundleLoadRequest.assetBundle;
            _bundleLoaded = true;
        }

        public static async Task<T> LoadAsset<T>(string name) where T : Object
        {
            if (_assetCache.TryGetValue(name, out var _loadedObject)) return _loadedObject as T;

            if (!_bundleLoaded)
            {
                _loadingTask ??= LoadBundle();
                await _loadingTask;
            }

            var assetLoadRequest = _storedBundle.LoadAssetAsync<T>(name);

            // AssetBundleRequest is a YieldInstruction !!
            await YieldUtils.YieldInstructionAsync(assetLoadRequest);

            var asset = assetLoadRequest.asset as T;
            _assetCache.AddOrUpdate(name, asset);
            return asset;
        }

        public static async Task<Texture2D> GetWallpaperTexture()
        {
            Texture2D wallpaperTex;

            if (!File.Exists(WallpaperPath))
            {
                wallpaperTex = new Texture2D(1280, 720);

                Color backgroundColour = new Color32(57, 57, 67, 255);

                wallpaperTex.SetPixels(Enumerable.Repeat(backgroundColour, wallpaperTex.width * wallpaperTex.height).ToArray());

                wallpaperTex.Apply();

                await File.WriteAllBytesAsync(WallpaperPath, wallpaperTex.EncodeToPNG());

                return wallpaperTex;
            }

            UnityWebRequest fileRequest = UnityWebRequest.Get(WallpaperPath);

            await YieldUtils.YieldWebRequestAsync(fileRequest);

            if (fileRequest.result != UnityWebRequest.Result.Success)
            {
                Logging.Fatal("Wallpaper could not yield successful result");
                Logging.Error(fileRequest.downloadHandler.error);
                return null;
            }

            wallpaperTex = new Texture2D(1280, 720);
            wallpaperTex.LoadImage(fileRequest.downloadHandler.data);
            wallpaperTex.Apply();

            return wallpaperTex;
        }
    }
}
