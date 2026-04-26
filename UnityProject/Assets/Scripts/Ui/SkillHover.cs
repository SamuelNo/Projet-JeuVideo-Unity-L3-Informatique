using UnityEngine;
using UnityEngine.EventSystems;

public class SkillHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int skillIndex; 
    private BattleUIController uiController;

    void Start() {
        uiController = Object.FindAnyObjectByType<BattleUIController>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (uiController.getCombatScript() == null || uiController.getCombatScript().getCurrentTeam() == -1 || uiController.getCombatScript().getCurrentPhase() == BattlePhase.WAITING) {
            return;
        }
        uiController.ShowSkillTooltip(skillIndex);
    }

    public void OnPointerExit(PointerEventData eventData) {
        uiController.HideSkillTooltip();
    }
}