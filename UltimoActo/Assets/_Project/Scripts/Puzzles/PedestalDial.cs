using System;
using System.Collections;
using UnityEngine;
using UltimoActo.Interaction;

namespace UltimoActo.Puzzles
{
    public class PedestalDial : MonoBehaviour, IInteractable
    {
        [Header("Identificación")]
        [SerializeField] private string dialId = "dial_1";
        [SerializeField] private string pedestalName = "Pilar del Cisne";

        [Header("Rotación y Estados")]
        [Tooltip("Transform que rotará (el cilindro o estatua superior)")]
        [SerializeField] private Transform rotatingPart;
        [SerializeField] private int totalSteps = 4; // 0, 1, 2, 3 (90 grados cada uno)
        [SerializeField] private int currentStep = 0;
        [SerializeField] private float rotationDuration = 0.4f;

        [Header("Símbolos de Ballet")]
        [SerializeField] private string[] stepNames = new string[]
        {
            "Cisne Blanco (Norte)",
            "Telón Teatral (Este)",
            "Máscara Trágica (Sur)",
            "Cisne Negro (Oeste)"
        };

        public event Action<PedestalDial, int> OnStepChanged;

        private Coroutine rotateCoroutine;
        private Quaternion baseRotation;

        public string Id => dialId;
        public int CurrentStep => currentStep;
        public string CurrentSymbolName => (currentStep >= 0 && currentStep < stepNames.Length) ? stepNames[currentStep] : $"Paso {currentStep}";

        public string Prompt => $"[E] Girar {pedestalName} [{CurrentSymbolName}]";

        private void Awake()
        {
            if (rotatingPart == null) rotatingPart = transform;
            baseRotation = rotatingPart.localRotation;
            UpdateRotationImmediate();
        }

        public bool CanInteract(GameObject player) => rotateCoroutine == null;

        public void Interact(GameObject player)
        {
            if (rotateCoroutine != null) return;

            currentStep = (currentStep + 1) % totalSteps;
            RotateToStep(currentStep);
            OnStepChanged?.Invoke(this, currentStep);
        }

        public void SetStep(int step, bool animate = true)
        {
            currentStep = Mathf.Clamp(step, 0, totalSteps - 1);
            if (animate && gameObject.activeInHierarchy)
            {
                RotateToStep(currentStep);
            }
            else
            {
                UpdateRotationImmediate();
            }
            OnStepChanged?.Invoke(this, currentStep);
        }

        private void RotateToStep(int step)
        {
            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            float targetAngle = step * (360f / totalSteps);
            Quaternion targetRot = baseRotation * Quaternion.Euler(0f, targetAngle, 0f);
            rotateCoroutine = StartCoroutine(AnimateRotation(targetRot));
        }

        private IEnumerator AnimateRotation(Quaternion targetRot)
        {
            Quaternion startRot = rotatingPart.localRotation;
            float elapsed = 0f;

            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / rotationDuration);
                rotatingPart.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            rotatingPart.localRotation = targetRot;
            rotateCoroutine = null;
        }

        private void UpdateRotationImmediate()
        {
            float targetAngle = currentStep * (360f / totalSteps);
            if (rotatingPart != null)
            {
                rotatingPart.localRotation = baseRotation * Quaternion.Euler(0f, targetAngle, 0f);
            }
        }
    }
}
