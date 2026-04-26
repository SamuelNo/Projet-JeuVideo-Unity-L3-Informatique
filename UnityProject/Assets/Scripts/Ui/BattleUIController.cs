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
    [SerializeField] private CombatButton simpleAttackButton, skillLvl1Button, skillLvl2Button, skillLvl3Button, endTurnButton;
    [SerializeField] private GameObject attackerNameObject;
    [SerializeField] private GameObject battleResultsPanel;
    [SerializeField] private GameObject instructionTextObject;
    [SerializeField] private GameObject warningTextObject;
    [SerializeField] private GameObject infoTextObject;
    [SerializeField] private GameObject announcementTextObject;
    [SerializeField] private GameObject resultTextObject;
    [SerializeField] private GameObject endGameTextObject;
    [SerializeField] private GameObject choicePanel;
    private BattlePhase currentPhase;
    [SerializeField] private GameObject statBarPrefab;
    [SerializeField] private Transform uiCanvasTransform;
    private List<StatBarHandler> activeBars = new List<StatBarHandler>();
    public GameObject skillTooltip; 
    public GameObject titleTextObject;
    public GameObject descriptionTextObject;


    // --------------- Initialisation ---------------
    void Start(){
        combatScript = Object.FindAnyObjectByType<Combat>(FindObjectsInactive.Exclude);
        if(SelectionData.Instance.isPvP){
            combatScript.startPvPFight();
        } else {
            combatScript.startPvMFight();
        }
        battleResultsPanel.SetActive(false);
        choicePanel.SetActive(false);
    
    }
    // --------------- Methods --------------- 
    public Combat getCombatScript(){ return combatScript; }
    public CombatButton getButtonEndTurn(){ return endTurnButton; }
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
        StartCoroutine(ClearTextAfterDelay(4.0f, announcementTextObject));
    }
    public void setResultText(string text){
        ///<summary> sets the result text in the UI </summary>
        resultTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
    }
    public void setInfoEntityText(GameObject entity, string text){
        ///<summary> sets the info text about the selected entity in the UI </summary>
        entity.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
    }
    public void setEndGameText(string text){
        ///<summary> sets the end game text in the UI (for example, to announce the winner) </summary>
        endGameTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text",text);
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

    public StatBarHandler CreateStatBar(Transform unitTransform) {
        GameObject barGo = Instantiate(statBarPrefab, uiCanvasTransform);
        StatBarHandler handler = barGo.GetComponent<StatBarHandler>();
        handler.target = unitTransform;
        activeBars.Add(handler);
        return handler;
    }

    public void ClearAllBars() {
        foreach (var bar in activeBars) {
            if (bar != null) Destroy(bar.gameObject);
        }
        activeBars.Clear();
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
        if(combatScript.getCurrentPhase() == BattlePhase.WAITING){
            if(clickedObject.GetComponent<Character>() != null){
                    clickedObject.GetComponent<Character>().Deselect();
                    clickedObject.GetComponent<Character>().getSelectionCircle().SetActive(false);
            }else if(clickedObject.GetComponent<Enemy>() != null){
                    clickedObject.GetComponent<Enemy>().Deselect();
                    clickedObject.GetComponent<Enemy>().GetSelectionCircle().SetActive(false);
            }
            return;
        }
        if (combatScript.getCurrentTeam() != -1){
            combatScript.select(clickedObject);
        }
        else {
                if(clickedObject.GetComponent<Character>() != null){
                    clickedObject.GetComponent<Character>().Deselect();
                    clickedObject.GetComponent<Character>().getSelectionCircle().SetActive(false);
                }
                else if(clickedObject.GetComponent<Enemy>() != null){
                    clickedObject.GetComponent<Enemy>().Deselect();
                    clickedObject.GetComponent<Enemy>().GetSelectionCircle().SetActive(false);
                }
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
    public void OnClickEndGame(){
        ///<summary> informs the Combat class that the player clicked on "Fin de la partie" (after the end of the fight) </summary>
        choicePanel.SetActive(true);
        setEndGameText("Voulez-vous arrêter la partie ?"); // asks the player if they want to return to the main menu
        combatScript.setIsBattleOver(true);
        simpleAttackButton.SetState(ButtonState.BLOCKED);
        skillLvl1Button.SetState(ButtonState.BLOCKED);
        skillLvl2Button.SetState(ButtonState.BLOCKED);
        skillLvl3Button.SetState(ButtonState.BLOCKED);
        endTurnButton.SetState(ButtonState.BLOCKED);
    }
    public void OnClickYesEndGameChoice()
    {
        Debug.Log("Vous avez cliqué sur 'Fin de la partie'. Retour au menu.");
        setInstructionText("Vous avez cliqué sur 'Fin de la partie'. Retour au menu.");
        choicePanel.SetActive(false);
        combatScript.setFinishedGame(true);
    }
    public void OnClickNoEndGameChoice()
    {
        Debug.Log("La partie continue");
        setInstructionText("La partie continue");
        choicePanel.SetActive(false);
        combatScript.setIsBattleOver(false);
        combatScript.setCurrentPhase(combatScript.getCurrentPhase()); 
        ButtonAccess();
    }
    public void ShowSkillTooltip(int index) {
        GameObject selected = combatScript.getSelectedCharacter();
        if (selected == null) return;

        Character data = selected.GetComponent<Character>();
        
        if (data != null && index < data.skillNames.Length) {
            skillTooltip.SetActive(true);
            
            titleTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text", data.skillNames[index]);
            descriptionTextObject.GetComponent<UnityEngine.Component>().SendMessage("set_text", data.skillDescriptions[index]);
        }
    }

    public void HideSkillTooltip() {
        if(skillTooltip != null)
            skillTooltip.SetActive(false);
    }
    public void SetSkillUI(bool visible) {
    if (skillTooltip != null) {
        skillTooltip.SetActive(visible);
    }
}
}