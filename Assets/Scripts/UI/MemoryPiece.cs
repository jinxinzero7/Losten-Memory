using UnityEngine;

[CreateAssetMenu(fileName = "NewMemory", menuName = "Lost Memories/Memory Piece")]
public class MemoryPiece : ScriptableObject
{
    public string memoryKey;
    public string memoryTitle;
    [TextArea] public string memoryDescription;
    [TextArea] public string cutsceneText;
    public string unlockScene;
    public Sprite memoryImage;
    public int id;
}
