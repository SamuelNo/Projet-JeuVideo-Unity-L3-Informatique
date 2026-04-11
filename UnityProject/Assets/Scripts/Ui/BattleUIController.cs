using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class BattleUIController : MonoBehaviour
{
    // --------------- Attributes ---------------
    // scripts
    private Combat combatScript;

    // buttons
    public CombatButton simpleAttackButton, skillLvl1Button, skillLvl2Button, skillLvl3Button, endTurnButton;

    // data
    private BattlePhase currentPhase;

    // test
    public GameObject perso1, perso2, perso3, perso4;
    private GameObject[] team1, team2;


    // --------------- Initialisation ---------------
    void Start(){
        combatScript = Object.FindAnyObjectByType<Combat>(FindObjectsInactive.Exclude);
        team1 = new GameObject[] {perso1, perso2};
        team2 = new GameObject[] {perso3, perso4};
        combatScript.startPvPFight(team1, team2);
    }
    
    // --------------- Methods --------------- 
    public void ButtonAccess(){
        ///<summary> updates the combat buttons based on the current battle state </summary>
        
        GameObject selectedCharacter = combatScript.getSelectedCharacter();
        currentPhase = combatScript.getCurrentPhase();

        endTurnButton.SetState(currentPhase == BattlePhase.WAITING ? ButtonState.BLOCKED : ButtonState.SHOWN);
        if (currentPhase == BattlePhase.WAITING || selectedCharacter == null){

            simpleAttackButton.SetState(ButtonState.BLOCKED);
            skillLvl1Button.SetState(ButtonState.BLOCKED);
            skillLvl2Button.SetState(ButtonState.BLOCKED);
            skillLvl3Button.SetState(ButtonState.BLOCKED);

        } else {
            Character attackerStats = selectedCharacter.GetComponent<Character>();

            simpleAttackButton.SetState(ButtonState.SHOWN);
            skillLvl1Button.SetState(attackerStats.getCurrentMP() >= attackerStats.getMpCostSkillLvl1() ? ButtonState.SHOWN : ButtonState.BLOCKED);
            skillLvl2Button.SetState(attackerStats.getCurrentMP() >= attackerStats.getMpCostSkillLvl2() ? ButtonState.SHOWN : ButtonState.BLOCKED);
            skillLvl3Button.SetState(attackerStats.getCurrentMP() >= attackerStats.getMpCostSkillLvl3() ? ButtonState.SHOWN : ButtonState.BLOCKED);
        }
    }

    public void HandleSelection(GameObject clickedObject){
        ///<summary> makes Combat handle the character and target selection </summary>

        if (combatScript.getCurrentPhase() != BattlePhase.WAITING){
            combatScript.select(clickedObject);
        }
    }

    public void OnClickSimpleAttack(){
        ///<summary> assigns the selected skill (base attack) to the Combat class </summary>

        Debug.Log("Vous avez sélectionné l'attaque basique. Veuillez choisir une cible.");

        combatScript.setSelectedSkill(0); // informs the Combat class that the selected skill is the basic attack

        combatScript.setCurrentPhase(BattlePhase.SELECT_TARGET);
        ButtonAccess();

        combatScript.automaticTargetSelection(); // assigns targets to selectedTargets if necessary
    }

    public void OnClickSkillLvl1(){
        ///<summary> assigns the selected skill (level 1 skill) to the Combat class </summary>

        Debug.Log("Vous avez sélectionné la compétence niveau 1. Veuillez choisir une cible.");

        combatScript.setSelectedSkill(1); // informs the Combat class that the selected skill is the lvl 1 skill

        combatScript.setCurrentPhase(BattlePhase.SELECT_TARGET);
        ButtonAccess();

        combatScript.automaticTargetSelection(); // assigns targets sto electedTargets if necessary
    }

    public void OnClickSkillLvl2(){
        ///<summary> assigns the selected skill (level 2 skill) to the Combat class </summary>

        Debug.Log("Vous avez sélectionné la compétence niveau 2. Veuillez choisir une cible.");

        combatScript.setSelectedSkill(2); // informs the Combat class that the selected skill is the lvl 2 skill

        combatScript.setCurrentPhase(BattlePhase.SELECT_TARGET);
        ButtonAccess();

        combatScript.automaticTargetSelection(); // assigns targets to selectedTargets if necessary
    }
    
    public void OnClickSkillLvl3(){
        ///<summary> assigns the selected skill (level 3 skill) to the Combat class </summary>

        Debug.Log("Vous avez sélectionné la compétence niveau 3. Veuillez choisir une cible.");

        combatScript.setSelectedSkill(3); // informs the Combat class that the selected skill is the lvl 3 skill

        combatScript.setCurrentPhase(BattlePhase.SELECT_TARGET);
        ButtonAccess();

        combatScript.automaticTargetSelection(); // assigns targets to selectedTargets if necessary
    }

    public void OnClickEndTurn(){
        ///<summary> informs the Combat class that the player clicked on "Fin de tour" </summary>

        Debug.Log("Vous avez cliqué sur 'Fin de Tour'.");

        combatScript.setFinishedTurn(true); 
    }
}