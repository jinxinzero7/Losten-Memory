using UnityEngine;

[CreateAssetMenu(fileName = "NewMemory", menuName = "Lost Memories/Memory Piece")]
public class MemoryPiece : ScriptableObject
{
    public string memoryTitle;
    [TextArea] public string memoryDescription;
    public Sprite memoryImage;
    public int id;
}