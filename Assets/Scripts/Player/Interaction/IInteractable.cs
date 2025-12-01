using UnityEngine;

public interface IInteractable
{
    void Interact(Interactor interactor);

    void ChangeMaterialToInteractable();

    void ChangeMaterialToNormal();
}
