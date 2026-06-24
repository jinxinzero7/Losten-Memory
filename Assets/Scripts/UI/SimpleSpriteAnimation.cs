using UnityEngine;

public class SimpleSpriteAnimation : MonoBehaviour
{
    public string[] spritePaths;
    public float frameDuration = 0.28f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private int frameIndex;
    private float timer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        LoadFrames();
    }

    private void Update()
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer < frameDuration) return;

        timer = 0f;
        frameIndex = (frameIndex + 1) % frames.Length;
        spriteRenderer.sprite = frames[frameIndex];
    }

    private void LoadFrames()
    {
        if (spritePaths == null || spritePaths.Length == 0) return;

        System.Collections.Generic.List<Sprite> loadedFrames = new System.Collections.Generic.List<Sprite>();
        for (int i = 0; i < spritePaths.Length; i++)
        {
            Sprite frame = RuntimeSpriteLoader.LoadProjectSprite(spritePaths[i], 100f);
            if (frame != null)
            {
                loadedFrames.Add(frame);
            }
        }

        frames = loadedFrames.ToArray();
        if (spriteRenderer != null && frames.Length > 0)
        {
            spriteRenderer.sprite = frames[0];
        }
    }
}
