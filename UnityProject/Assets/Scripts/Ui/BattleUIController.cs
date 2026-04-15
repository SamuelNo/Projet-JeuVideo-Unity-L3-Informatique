using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor;


public class BattleUIController : MonoBehaviour
{
    // --------------- Attributes ---------------
    // scripts
    private Combat combatScript;

    // buttons
    public CombatButton simpleAttackButton, skillLvl1Button, skillLvl2Button, skillLvl3Button, endTurnButton;
    public GameObject attackerNameObject;
    public GameObject battleResultsPanel;
    public GameObject instructionTextObject;
    public GameObject warningTextObject;
    public GameObject infoTextObject;
    public GameObject announcementTextObject;
    public GameObject resultTextObject;
    private BattlePhase currentPhase;
  


    // --------------- Initialisation ---------------
    void Start(){
        combatScript = Object.FindAnyObjectByType<Combat>(FindObjectsInactive.Exclude);
        if(SelectionData.Instance.isPvP){
            combatScript.startPvPFight();
        } else {
            combatScript.startPvMFight();
        }
        battleResultsPanel.SetActive(false);
    
    }
    // --------------- Methods --------------- 
    public Combat getCombatScript(){ return combatScript; }
    private IEnumerator ClearTextAfterDelay(float delay, GameObject textObject) 
    {
        yield return new WaitForSeconds(delay);
        textObject.GetComponent<UnityEngine.Component>().SendMessage("set_text", ""); // Clear the text after the delay 
    }
    public void setAttackerName(string name){
        ///<summary> sets the name of the current attacker in the UI </summary>
        attackerNameObject.GetComponent<UnityEngine.Component>().SendMessage("set_text",name);
    }
    public void setInstructionText(string text){
        ///<summary> sets the instruction text in the UI </summary>
        instructionTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
    }
    public void setWarningText(string text){
        ///<summary> sets the warning text in the UI </summary>
        warningTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
        StartCoroutine(ClearTextAfterDelay(3.0f, warningTextObject));
    }
    public void setInfoText(string text){
        ///<summary> sets the info text in the UI </summary>
        infoTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
        StartCoroutine(ClearTextAfterDelay(5.0f, infoTextObject));
    }
    public void setAnnouncementText(string text){
        ///<summary> sets the announcement text in the UI (for example, to announce the end of the fight and the winner) </summary>
        announcementTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
        StartCoroutine(ClearTextAfterDelay(3.0f, announcementTextObject));
    }
    public void setResultText(string text){
        ///<summary> sets the result text in the UI </summary>
        resultTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
    }
    public void DisplayEndGame(string message) 
    {
        battleResultsPanel.SetActive(true);
        setResultText(message);
    } 

    public void GoToMenu() 
    {
        SceneManager.LoadScene("Menu_Scene");
    }
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
        setInstructionText("Vous avez sélectionné l'attaque basique. Veuillez choisir une cible.");

        combatScript.setSelectedSkill(0); // informs the Combat class that the selected skill is the basic attack
        combatScript.setCurrentPhase(BattlePhase.SELECT_TARGET);
        ButtonAccess();
        simpleAttackButton.SetState(ButtonState.BLOCKED);

        combatScript.automaticTargetSelection(); // assigns targets to selectedTargets if necessary
    }

    public void OnClickSkillLvl1(){
        ///<summary> assigns the selected skill (level 1 skill) to the Combat class </summary>

        Debug.Log("Vous avez sélectionné la compétence niveau 1. Veuillez choisir une cible.");
        setInstructionText("Vous avez sélectionné la compétence niveau 1. Veuillez choisir une cible.");

        combatScript.setSelectedSkill(1); // informs the Combat class that the selected skill is the lvl 1 skill

        combatScript.setCurrentPhase(BattlePhase.SELECT_TARGET);
        ButtonAccess();
        skillLvl1Button.SetState(ButtonState.BLOCKED);

        combatScript.automaticTargetSelection(); // assigns targets sto electedTargets if necessary
    }

    public void OnClickSkillLvl2(){
        ///<summary> assigns the selected skill (level 2 skill) to the Combat class </summary>

        Debug.Log("Vous avez sélectionné la compétence niveau 2. Veuillez choisir une cible.");
        setInstructionText("Vous avez sélectionné la compétence niveau 2. Veuillez choisir une cible.");

        combatScript.setSelectedSkill(2); // informs the Combat class that the selected skill is the lvl 2 skill

        combatScript.setCurrentPhase(BattlePhase.SELECT_TARGET);
        ButtonAccess();
        skillLvl2Button.SetState(ButtonState.BLOCKED);
        combatScript.automaticTargetSelection(); // assigns targets to selectedTargets if necessary
    }
    
    public void OnClickSkillLvl3(){
        ///<summary> assigns the selected skill (level 3 skill) to the Combat class </summary>

        Debug.Log("Vous avez sélectionné la compétence niveau 3. Veuillez choisir une cible.");
        setInstructionText("Vous avez sélectionné la compétence niveau 3. Veuillez choisir une cible.");
        combatScript.setSelectedSkill(3); // informs the Combat class that the selected skill is the lvl 3 skill

        combatScript.setCurrentPhase(BattlePhase.SELECT_TARGET);
        ButtonAccess();
        skillLvl3Button.SetState(ButtonState.BLOCKED);

        combatScript.automaticTargetSelection(); // assigns targets to selectedTargets if necessary
    }

    public void OnClickEndTurn(){
        ///<summary> informs the Combat class that the player clicked on "Fin de tour" </summary>

        Debug.Log("Vous avez cliqué sur 'Fin de Tour'.");
        setInstructionText("Vous avez cliqué sur 'Fin de Tour'.");

        combatScript.setFinishedTurn(true); 
    }
}