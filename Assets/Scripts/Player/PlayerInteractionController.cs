using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    public float horizontalRadius = 1.75f;
    public float verticalRadius = 2.75f;
    public float refreshInterval = 0.05f;

    private readonly List<IWorldInteractable> candidates = new List<IWorldInteractable>();
    private IWorldInteractable activeInteractable;
    private float nextRefreshTime;

    private void OnEnable()
    {
        RefreshCandidates();
        nextRefreshTime = 0f;
    }

    private void Update()
    {
        if (Time.time >= nextRefreshTime)
        {
            RefreshCandidates();
            nextRefreshTime = Time.time + refreshInterval;
        }

        IWorldInteractable nextActive = FindBestInteractable();
        if (!ReferenceEquals(activeInteractable, nextActive))
        {
            if (IsAlive(activeInteractable))
            {
                activeInteractable.SetInteractionHighlighted(false);
            }

            activeInteractable = nextActive;
            activeInteractable?.SetInteractionHighlighted(true);
        }

        if (IsAlive(activeInteractable) && GameInput.InteractPressed)
        {
            IWorldInteractable interacted = activeInteractable;
            interacted.Interact();
            if (ReferenceEquals(activeInteractable, interacted) && IsAlive(interacted))
            {
                interacted.SetInteractionHighlighted(false);
            }

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
            if (IsAlive(activeInteractable))
            {
                activeInteractable.SetInteractionHighlighted(false);
            }

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
            if (!IsAlive(candidate) || candidate.InteractionTransform == null)
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

            Vector2 delta = candidate.GetInteractionPoint(playerPosition) - playerPosition;
            float normalizedDistance = Mathf.Sqrt(
                Mathf.Pow(delta.x / horizontalRadius, 2f) +
                Mathf.Pow(delta.y / verticalRadius, 2f));
            if (normalizedDistance > 1f) continue;

            int priority = candidate.InteractionPriority;
            bool isBetter = normalizedDistance < bestDistance - 0.001f
                || (Mathf.Abs(normalizedDistance - bestDistance) <= 0.001f && priority > bestPriority);

            if (!isBetter) continue;

            best = candidate;
            bestDistance = normalizedDistance;
            bestPriority = priority;
        }

        return best;
    }

    private void RefreshCandidates()
    {
        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude))
        {
            if (behaviour is IWorldInteractable interactable)
            {
                Register(interactable);
            }
        }
    }

    private static bool IsAlive(IWorldInteractable interactable)
    {
        return interactable is Object unityObject ? unityObject != null : interactable != null;
    }
}
