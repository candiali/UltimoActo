using System;
using UnityEngine;
using UltimoActo.Core;
using UltimoActo.Interaction;
using UltimoActo.Network;

namespace UltimoActo.Puzzles
{
    public enum CoopCooperationType
    {
        /// <summary>
        /// Tipo 1: Pista en una época ("EraOnlyVisibility") y acción/mecanismo en la otra época.
        /// </summary>
        ClueInOneEraActionInOther = 1,

        /// <summary>
        /// Tipo 2: Acción simultánea entre dos interruptores/palancas (uno en 1929 y otro en Ruinas)
        /// validada por el Host con ventana de tolerancia, o mantenida por ayudante-eco/temporizador en Modo Solo.
        /// </summary>
        SimultaneousAction = 2,

        /// <summary>
        /// Tipo 3: Secuencia por turnos donde cada jugador acciona una parte (uno toca notas y el otro valida pasos).
        /// </summary>
        TurnBasedSequence = 3,

        /// <summary>
        /// Tipo 4: Dos mitades de una clave (cada jugador encuentra su mitad en su época y deben juntarla en un dial/cerradura).
        /// </summary>
        SplitKeyTwoHalves = 4,

        /// <summary>
        /// Tipo 5: Efecto espejo entre épocas (un cambio en 1929 despeja o revela un camino/objeto en las Ruinas).
        /// </summary>
        CrossEraMirrorEffect = 5
    }

    /// <summary>
    /// BLOQUE B — Clase base reutilizable para todos los Puzles Cooperativos entre épocas (1929 ↔ Ruinas).
    /// - Hereda de PuzzleBase sin romper ningún puzle existente.
    /// - Autoridad del Host y sincronización automática con SharedWorldStateSync.
    /// - Registra automáticamente sus pistas (con etiqueta "Solo visible en 1929" o "Solo visible en Ruinas"),
    ///   su solución final y los logros cooperativos ("Voz en la oscuridad", "Sincronía", "Dos tiempos")
    ///   en el Diario Compartido (SharedEvidenceJournalManager).
    /// - Soporta Modo Solo al 100%: detecta cuando el jugador está en solitario (CanLocalPlayerToggleEraWithQ)
    ///   para permitir alternar con [Q] o activar un temporizador/eco de retención en acciones simultáneas.
    /// </summary>
    public abstract class CoopPuzzleBase : PuzzleBase
    {
        public static event Action<string, string> OnCoopPuzzleNetworkCommand;

        [Header("Configuración Cooperativa (1929 ↔ Ruinas)")]
        [SerializeField] protected CoopCooperationType cooperationType = CoopCooperationType.SplitKeyTwoHalves;
        [SerializeField] protected string puzzleDisplayTitle = "Mecanismo Cooperativo Vesper";
        [SerializeField] protected WorldDoubleDoor doubleDoorToUnlock;
        [SerializeField] protected bool openDoorsImmediatelyOnSolve = true;

        [Header("Logro Cooperativo Asociado")]
        [SerializeField] protected string achievementId = "";
        [SerializeField] protected string achievementTitle = "";
        [SerializeField] protected string achievementDescription = "";

        public CoopCooperationType CooperationType => cooperationType;
        public string PuzzleDisplayTitle => puzzleDisplayTitle;

        /// <summary>
        /// Devuelve true si el jugador está jugando en Modo Solo (o como Host 1/2 esperando)
        /// y por tanto puede resolver el puzle alternando de época con [Q] o usando el eco-temporizador.
        /// </summary>
        public bool IsSoloFallbackActive =>
            LanSessionManager.Instance == null || LanSessionManager.Instance.ConnectedPlayerCount < 2;

        /// <summary>
        /// Devuelve la época visual que está viendo actualmente el jugador local.
        /// </summary>
        public EraVisual CurrentLocalEra =>
            EraVisualManager.Instance != null ? EraVisualManager.Instance.CurrentEra : EraVisual.Glory1929;

        /// <summary>
        /// Verifica si el jugador local puede interactuar con un elemento restringido a una época específica.
        /// </summary>
        public bool CanInteractInEra(EraVisibilityMode requiredEra, out string deniedReason)
        {
            deniedReason = string.Empty;
            if (requiredEra == EraVisibilityMode.Both) return true;

            EraVisual localEra = CurrentLocalEra;
            if (requiredEra == EraVisibilityMode.Only1929 && localEra != EraVisual.Glory1929)
            {
                deniedReason = IsSoloFallbackActive
                    ? "Este mecanismo pertenece a 1929 — Pulsa [Q] para cambiar a la Época 1929"
                    : "Este mecanismo solo puede accionarlo el investigador de la Época 1929";
                return false;
            }

            if (requiredEra == EraVisibilityMode.OnlyRuins && localEra != EraVisual.Ruins)
            {
                deniedReason = IsSoloFallbackActive
                    ? "Este mecanismo pertenece a las Ruinas — Pulsa [Q] para cambiar a la Época Ruinas"
                    : "Este mecanismo solo puede accionarlo el investigador de la Época Ruinas";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Registra una pista del puzle en el Diario Compartido ([J]/[TAB]) indicando claramente en qué época es visible.
        /// </summary>
        public void RegisterCoopClueInJournal(
            string clueSubId,
            string clueTitle,
            string clueBody,
            EraVisibilityMode visibilityRule,
            JournalTabCategory category = JournalTabCategory.PuzzleClues)
        {
            if (SharedEvidenceJournalManager.Instance == null) return;

            string fullId = $"coop_clue_{puzzleId}_{clueSubId}";
            string sub = visibilityRule == EraVisibilityMode.Only1929
                ? $"Pista Cooperativa ({puzzleDisplayTitle}) — Solo visible en 1929"
                : (visibilityRule == EraVisibilityMode.OnlyRuins
                    ? $"Pista Cooperativa ({puzzleDisplayTitle}) — Solo visible en Ruinas"
                    : $"Pista Cooperativa ({puzzleDisplayTitle}) — Visible en ambas épocas");

            SharedEvidenceJournalManager.Instance.RecordClueOrDocument(
                entryId: fullId,
                title: clueTitle,
                subtitle: sub,
                bodyContent: clueBody,
                category: category,
                visibilityRule: visibilityRule);
        }

        protected void DispatchCoopSubStateToNetwork(string subPayload)
        {
            OnCoopPuzzleNetworkCommand?.Invoke(puzzleId, subPayload);
        }

        public virtual void CompleteCoopPuzzle(string solutionJournalSummary, bool broadcastToNetwork = true)
        {
            if (state == PuzzleState.Solved) return;

            base.Solve();

            if (doorToUnlock != null)
            {
                doorToUnlock.UnlockDoor(openDoorsImmediatelyOnSolve);
            }

            if (doubleDoorToUnlock != null)
            {
                doubleDoorToUnlock.SetLockState(false);
                if (openDoorsImmediatelyOnSolve && !doubleDoorToUnlock.IsOpen)
                {
                    doubleDoorToUnlock.ToggleDoor();
                }
            }

            if (SharedEvidenceJournalManager.Instance != null && !string.IsNullOrEmpty(solutionJournalSummary))
            {
                SharedEvidenceJournalManager.Instance.RecordPuzzleSolution(
                    puzzleId: $"sol_coop_{puzzleId}",
                    puzzleTitle: $"[SOLUCIÓN COOPERATIVA] {puzzleDisplayTitle}",
                    solutionSummary: solutionJournalSummary,
                    visibilityRule: EraVisibilityMode.Both);

                if (!string.IsNullOrEmpty(achievementId))
                {
                    SharedEvidenceJournalManager.Instance.UnlockCoopAchievement(
                        achievementId,
                        achievementTitle,
                        achievementDescription);
                }
            }

            if (NightManager.Instance != null)
            {
                NightManager.Instance.RegisterEvidence($"coop_solved_{puzzleId}");
            }

            if (broadcastToNetwork)
            {
                DispatchCoopSubStateToNetwork("SOLVED");
            }
        }

        /// <summary>
        /// Aplica un comando o estado replicado desde la red (Host -> Client o Client -> Host).
        /// </summary>
        public abstract void ApplyNetworkCommand(string payload);

        /// <summary>
        /// Devuelve una cadena serializada del estado actual del puzle para enviarla en el snapshot completo.
        /// </summary>
        public abstract string GetNetworkSnapshotPayload();
    }
}
