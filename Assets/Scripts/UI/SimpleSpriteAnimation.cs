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

        frames = new Sprite[spritePaths.Length];
        for (int i = 0; i < spritePaths.Length; i++)
        {
            frames[i] = RuntimeSpriteLoader.LoadProjectSprite(spritePaths[i], 100f);
        }
    }
}
