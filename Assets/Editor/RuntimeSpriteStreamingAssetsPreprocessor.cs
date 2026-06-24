using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class RuntimeSpriteStreamingAssetsPreprocessor : IPreprocessBuildWithReport
{
    private static readonly string[] RuntimeSpritePaths =
    {
        "Assets/Art/Sprites/coin_norm.png",
        "Assets/Art/Sprites/interface/button.PNG",
        "Assets/Art/Sprites/interface/pauseMenu.PNG",
        "Assets/Art/Sprites/interface/inventoryCell.PNG",
        "Assets/Art/Sprites/interface/Key.PNG",
        "Assets/Art/Sprites/interface/photo.PNG",
        "Assets/Art/Sprites/places/newSprites/3/photo.PNG",
        "Assets/Art/Sprites/places/newSprites/3/feed.png",
        "Assets/Art/Sprites/places/newSprites/3/box.PNG",
        "Assets/Art/Sprites/Maze/item_dog_food_bag.png",
        "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2821.PNG",
        "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2822.PNG",
        "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2823.PNG",
        "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2824.PNG",
        "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2825.PNG",
        "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2826.PNG",
        "Assets/Art/Sprites/places/newSprites/2/npc/IMG_2827.PNG"
    };

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        SyncRuntimeSprites();
    }

    [MenuItem("Tools/Losten Memory/Sync Runtime Sprites")]
    public static void SyncRuntimeSprites()
    {
        foreach (string sourcePath in RuntimeSpritePaths)
        {
            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning("Runtime sprite source is missing: " + sourcePath);
                continue;
            }

            string targetPath = Path.Combine("Assets/StreamingAssets", sourcePath);
            string targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Copy(sourcePath, targetPath, true);
        }

        AssetDatabase.Refresh();
    }
}
