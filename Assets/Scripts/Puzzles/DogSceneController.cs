using UnityEngine;

public class DogSceneController : MonoBehaviour
{
    public GameObject portal;

    private void Update()
    {
        if (portal != null)
        {
            portal.SetActive(DemoQuest.IsMemoryUnlocked("dog_memory"));
        }
    }
}
