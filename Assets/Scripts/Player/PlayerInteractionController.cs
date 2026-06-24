using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    public float interactionRadius = 1.15f;

    private readonly List<IWorldInteractable> candidates = new List<IWorldInteractable>();
    private IWorldInteractable activeInteractable;

    private void Update()
    {
        IWorldInteractable nextActive = FindBestInteractable();
        if (!ReferenceEquals(activeInteractable, nextActive))
        {
            activeInteractable?.SetInteractionHighlighted(false);
            activeInteractable = nextActive;
            activeInteractable?.SetInteractionHighlighted(true);
        }

        if (activeInteractable != null && GameInput.InteractPressed)
        {
            activeInteractable.Interact();
            activeInteractable.SetInteractionHighlighted(false);
            activeInteractable = FindBestInteractable();
            activeInteractable?.SetInteractionHighlighted(true);
        }
    }

    public void Register(IWorldInteractable interactable)
    {
        if (interactable == null || candidates.Contains(interactable)) return;

        candidates.Add(interactable);
    }

    public void Unregister(IWorldInteractable interactable)
    {
        if (interactable == null) return;

        candidates.Remove(interactable);
        if (ReferenceEquals(activeInteractable, interactable))
        {
            activeInteractable.SetInteractionHighlighted(false);
            activeInteractable = null;
        }
    }

    private IWorldInteractable FindBestInteractable()
    {
        Vector2 playerPosition = transform.position;
        IWorldInteractable best = null;
        float bestDistance = float.MaxValue;
        int bestPriority = int.MinValue;

        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            IWorldInteractable candidate = candidates[i];
            if (candidate == null || candidate.InteractionTransform == null)
            {
                candidates.RemoveAt(i);
                continue;
            }

            if (!candidate.CanInteract)
            {
                if (ReferenceEquals(activeInteractable, candidate))
                {
                    candidate.SetInteractionHighlighted(false);
                }

                continue;
            }

            float distance = Vector2.Distance(playerPosition, candidate.GetInteractionPoint(playerPosition));
            if (distance > interactionRadius) continue;

            int priority = candidate.InteractionPriority;
            bool isBetter = distance < bestDistance - 0.001f
                || (Mathf.Abs(distance - bestDistance) <= 0.001f && priority > bestPriority);

            if (!isBetter) continue;

            best = candidate;
            bestDistance = distance;
            bestPriority = priority;
        }

        return best;
    }
}
