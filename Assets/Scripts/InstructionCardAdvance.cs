using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Templates.AR;

/// <summary>
/// Makes an onboarding instruction card behave like a clear "continue" button.
/// </summary>
public class InstructionCardAdvance : MonoBehaviour, IPointerClickHandler
{
    GoalManager goalManager;

    void Awake()
    {
        goalManager = FindAnyObjectByType<GoalManager>();

        TMP_Text textoPrincipal = GetComponentsInChildren<TMP_Text>(true)
            .OrderByDescending(texto => texto.text.Length)
            .FirstOrDefault();

        if (textoPrincipal != null && !textoPrincipal.text.Contains("Toca esta tarjeta"))
            textoPrincipal.text += "\n\nToca esta tarjeta para continuar";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !gameObject.activeInHierarchy)
            return;

        if (goalManager == null)
            goalManager = FindAnyObjectByType<GoalManager>();

        goalManager?.ForceCompleteGoal();
    }
}
