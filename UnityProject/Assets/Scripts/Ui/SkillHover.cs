using UnityEngine;
using UnityEngine.EventSystems;

public class SkillHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int skillIndex; // À régler dans l'Inspector : 0, 1, 2 ou 3
    private BattleUIController uiController;

    void Start() {
        uiController = Object.FindAnyObjectByType<BattleUIController>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        // On demande à l'UI d'afficher la bulle d'info
        uiController.ShowSkillTooltip(skillIndex);
    }

    public void OnPointerExit(PointerEventData eventData) {
        // On demande à l'UI de cacher la bulle d'info
        uiController.HideSkillTooltip();
    }
}