using UnityEngine;

public interface IWorldInteractable
{
    Transform InteractionTransform { get; }
    int InteractionPriority { get; }
    bool CanInteract { get; }
    Vector2 GetInteractionPoint(Vector2 playerPosition);
    void SetInteractionHighlighted(bool highlighted);
    void Interact();
}
