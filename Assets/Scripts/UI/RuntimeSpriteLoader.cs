using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RuntimeSpriteLoader
{
    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static Sprite LoadProjectSprite(string assetPath, float pixelsPerUnit = 100f)
    {
        return LoadProjectSprite(assetPath, Rect.zero, pixelsPerUnit);
    }

    public static Sprite LoadProjectSprite(string assetPath, Rect sourceRect, float pixelsPerUnit = 100f)
    {
        if (string.IsNullOrWhiteSpace(assetPath)) return null;

        string cacheKey = $"{assetPath}|{sourceRect.x}|{sourceRect.y}|{sourceRect.width}|{sourceRect.height}|{pixelsPerUnit}";
        if (Cache.TryGetValue(cacheKey, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        string fullPath = ResolveAssetPath(assetPath);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            Object.Destroy(texture);
            return null;
        }

        texture.filterMode = FilterMode.Point;
        Rect rect = sourceRect.width > 0f && sourceRect.height > 0f
            ? sourceRect
            : new Rect(0f, 0f, texture.width, texture.height);

        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
        Cache[cacheKey] = sprite;
        return sprite;
    }

    private static string ResolveAssetPath(string assetPath)
    {
        string normalizedPath = assetPath.Replace('\\', '/').TrimStart('/');
        string projectPath = Path.Combine(Application.dataPath, normalizedPath.StartsWith("Assets/")
            ? normalizedPath.Substring("Assets/".Length)
            : normalizedPath);

        if (File.Exists(projectPath))
        {
            return projectPath;
        }

        string siblingAssetsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", normalizedPath));
        return File.Exists(siblingAssetsPath) ? siblingAssetsPath : null;
    }
}
