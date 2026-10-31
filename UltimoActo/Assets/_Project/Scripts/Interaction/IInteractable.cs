using UnityEngine;

namespace UltimoActo.Interaction
{
    /// <summary>
    /// Interfaz base para cualquier objeto con el que el jugador pueda interactuar.
    /// Diseñada para ser fácilmente trasladable a NetworkInteractable en la Fase 2 (Multijugador LAN).
    /// </summary>
    public interface IInteractable
    {
        string Id { get; }
        string Prompt { get; }
        bool CanInteract(GameObject player);
        void Interact(GameObject player);
    }
}
