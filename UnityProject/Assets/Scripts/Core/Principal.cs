using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


/// <summary>
/// Controls main menu, team selection, stage selection, and transition to combat.
/// </summary>
public class Principal : MonoBehaviour
{
    

    
    private MenuButtons buttonScript;
    private Save saveScript;

    
    private int[] selectedCharacters1, selectedCharacters2; 
    private int selectedStage, character1, character2; 

    
    [SerializeField] 
    private GameObject fighterSprite1, healerSprite1, mageSprite1, protectorSprite1, 
                       fighterSprite2, healerSprite2, mageSprite2, protectorSprite2;
    private GameObject[] characterSpriteList1, characterSpriteList2; 
    private GameObject[] teamPlayer1, teamPlayer2; 

    
    [SerializeField] 
    private GameObject stageCharacterSprite; 
    private bool moving; 
    
    private GameObject stageSprite0, stageSprite1, stageSprite2; 
    private GameObject[] stageSpriteList; 
    [SerializeField] private int unlockedStage; 
    

    
    [SerializeField] 
    private Button pvpButton, 
                   pvmButton, 
                   menuButton, 
                   nextPlayerButton, 
                   startPvPFightButton, 
                   choseStageButton, 
                   stageButton, 
                   startPvMFightButton, 
                   leftButton1, 
                   rightButton1, 
                   leftButton2, 
                   rightButton2, 
                   resetButton,
                   resetYesButton,
                   resetNoButton,
                   applicationQuitButton,
                   muteMusicButton;
    [SerializeField] private GameObject resetPanel;
    [SerializeField] private GameObject[] characterBasePrefabs; 
    [SerializeField] private GameObject[] monsterPrefabs; 
    [SerializeField] private GameObject leftInfoPanel, rightInfoPanel;
    [SerializeField] private GameObject leftNameObj, leftBioObj, rightNameObj, rightBioObj;
    [SerializeField] private GameObject stageInfoPanel; 
    [SerializeField] private GameObject stageEnemyListObj; 
    [SerializeField] private GameObject[] listMapName; 
    [SerializeField] private GameObject[] titles ; 
    [SerializeField] private GameObject[] blockedSpriteList;
    [SerializeField] private GameObject instructionPanel; 
    
    
    public void setSelectedCharacters1(int[] list){ selectedCharacters1 = list; }
    public void setSelectedCharacters2(int[] list){ selectedCharacters2 = list; }
    public void setSelectedStage(int s){ selectedStage = s; }
    public void setCharacter1(int s){ character1 = s; }
    public void setCharacter2(int s){ character2 = s; }
    public void setMoving(bool b){ moving = b; }
    public void setUnlockedStage(int s){ unlockedStage = s; }

    
    public int[] getSelectedCharacters1(){ return selectedCharacters1; }
    public int[] getSelectedCharacters2(){ return selectedCharacters2; }
    public int getSelectedStage(){ return selectedStage; }
    public int getCharacter1(){ return character1; }
    public int getCharacter2(){ return character2; }
    public bool getMoving(){ return moving; }
    public GameObject[] getcharacterSpriteList1(){ return characterSpriteList1; }
    public GameObject[] getcharacterSpriteList2(){ return characterSpriteList2; }
    public GameObject[] getStageList(){ return stageSpriteList; }
    public int getUnlockedStage(){ return unlockedStage; }
    

    
    /// <summary>
    /// Initializes menu dependencies, cached references, and default UI state.
    /// </summary>
    void Awake(){
        
        buttonScript = this.gameObject.AddComponent<MenuButtons>(); 
        saveScript = this.gameObject.AddComponent<Save>();


        
        

        
        

        
        
        characterSpriteList1 = new GameObject[] {fighterSprite1, healerSprite1, mageSprite1, protectorSprite1};
        characterSpriteList2 = new GameObject[] {fighterSprite2, healerSprite2, mageSprite2, protectorSprite2};


        
        stageSprite0 = GameObject.Find("StageSprite0");
        stageSprite1 = GameObject.Find("StageSprite1");
        stageSprite2 = GameObject.Find("StageSprite2");
        stageCharacterSprite = GameObject.Find("StageCharacterSprite");
        
        stageSpriteList = new GameObject[] {stageSprite0, stageSprite1, stageSprite2};
        saveScript.loadFile();
        menu();
    }

    void Reset(){
        Awake();
    }


    

    

    

    /// <summary>
    /// Moves stage selection to the previous available stage.
    /// </summary>
    public void leftArrow(InputAction.CallbackContext ctx){
        if (ctx.started){ 
            if (!moving){ 
                if (selectedStage > 0)
                    selectedStage -= 1; 
                    UpdateStageInfo(); 
                StartCoroutine(moveCharacter()); 
            }
        }
    }

    /// <summary>
    /// Moves stage selection to the next available stage.
    /// </summary>
    public void rightArrow(InputAction.CallbackContext ctx){
        if (ctx.started){ 
            if (!moving){ 
                if (selectedStage < unlockedStage) 
                    selectedStage += 1; 
                    UpdateStageInfo(); 
                StartCoroutine(moveCharacter()); 
            }
        }
    }
    private void UpdateDescription(int characterIndex, GameObject nameObj, GameObject bioObj) {
        if (characterBasePrefabs == null || characterIndex >= characterBasePrefabs.Length) return;

        Character data = characterBasePrefabs[characterIndex].GetComponent<Character>();
        
        if (data != null) {
            nameObj.GetComponent<UnityEngine.Component>().SendMessage("set_text", data.characterName);

            string details = data.characterDescription + "\n\n<b>SORTS :</b>\n";
            for (int i = 0; i < data.skillNames.Length; i++) {
                details += "<b>" + data.skillNames[i] + "</b>" + "\n";
            }
            
            bioObj.GetComponent<UnityEngine.Component>().SendMessage("set_text", details);
        }
    }
    /// <summary>
    /// Refreshes stage information and enemy preview in the UI.
    /// </summary>
    public void UpdateStageInfo() {
    if (stageInfoPanel == null) return;
    
    stageInfoPanel.SetActive(true);
    string enemyText = "<b>MONSTRES À COMBATTRE :</b>\n";
    int[] enemiesToShow;

    
    switch (selectedStage) {
        case 0:
            enemiesToShow = new int[] { 0, 1 };
            break;
        case 1:
            enemiesToShow = new int[] { 2, 3, 4 }; 
            break;
        case 2:
            enemiesToShow = new int[] { 5 };
            break;
        default:
            enemiesToShow = new int[] { };
            break;
    }


    foreach (int index in enemiesToShow) {
        if (index < monsterPrefabs.Length) {
        
            Enemy data = monsterPrefabs[index].GetComponent<Enemy>();
            if (data != null) {
                enemyText += "- " + data.enemyName + "\n"+ data.enemyDescription + "\n";
            }
        }
    }

    
    stageEnemyListObj.GetComponent<UnityEngine.Component>().SendMessage("set_text", enemyText);
    }

    

    /// <summary>
    /// Displays the main menu and resets selection-related UI state.
    /// </summary>
    public void menu(){
        
        moving = true; 
        hideAll(); 

        
        

        
        pvmButton.gameObject.SetActive(true); 
        pvpButton.gameObject.SetActive(true); 
        resetButton.gameObject.SetActive(true); 
        applicationQuitButton.gameObject.SetActive(true); 
        foreach(GameObject s in titles){
            s.gameObject.SetActive(true);
        }
        muteMusicButton.gameObject.SetActive(true); 
        titles[3].gameObject.SetActive(false);

    }

    
    /// <summary>
    /// Opens player one character selection screen.
    /// </summary>
    public void characterSelection1(){

        hideAll();

        
        

        
        showCharacterSelectionButtons(); 
        titles[3].gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true); 
        nextPlayerButton.gameObject.SetActive(true); 
    }

    /// <summary>
    /// Opens player two character selection screen for PvP.
    /// </summary>
    public void characterSelection2(){

        selectedCharacters1 = new int[]{character1, character2}; 
        

        hideAll();

        
        

        
        showCharacterSelectionButtons(); 
        titles[3].gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true); 
        startPvPFightButton.gameObject.SetActive(true); 
    }

    /// <summary>
    /// Stores PvP selections and loads the combat scene.
    /// </summary>
    public void startPvPFight(){
        
        selectedCharacters2 = new int[]{character1, character2}; 
        SelectionData.Instance.team1 = selectedCharacters1;
        SelectionData.Instance.team2 = selectedCharacters2;
        SelectionData.Instance.isPvP = true;
        SelectionData.Instance.selectedStage = 0;
        SceneManager.LoadScene("Combat_Scene");
    }
    
    
    /// <summary>
    /// Opens stage selection flow for PvM mode.
    /// </summary>
    public void stageSelection(){

        moving = false; 
        hideAll();

        
        stageCharacterSprite.transform.position = stageSpriteList[selectedStage].transform.position;
        UpdateStageInfo();
        foreach(GameObject s in titles){
            s.gameObject.SetActive(false);
        }
        titles[3].gameObject.SetActive(true);
        
        menuButton.gameObject.SetActive(true); 
        choseStageButton.gameObject.SetActive(true); 
        stageCharacterSprite.gameObject.SetActive(true); 
        stageInfoPanel.SetActive(true); 
        instructionPanel.SetActive(true); 
        foreach (GameObject s in stageSpriteList){ 
            s.gameObject.SetActive(true);
        }
        foreach(GameObject s in listMapName){
            s.gameObject.SetActive(true);
        }
        
        
        for (int i=0; i<stageSpriteList.Length; i++){
            if (i > unlockedStage){
                stageSpriteList[i].GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.7f);
                blockedSpriteList[i].SetActive(true); 
                 
            } else {
                stageSpriteList[i].GetComponent<Image>().color = Color.white;
                blockedSpriteList[i].SetActive(false); 
            }
        }
        
    }

    /// <summary>
    /// Animates the stage cursor to the currently selected stage.
    /// </summary>
    public IEnumerator moveCharacter() {
        moving = true;
        float duration = 0.5f; 
        float elapsedTime = 0f;
        Vector3 startingPos = stageCharacterSprite.transform.position;
        Vector3 targetPosition = stageSpriteList[selectedStage].transform.position;

        Vector3 characterScale = stageCharacterSprite.transform.localScale;
        characterScale.x = (targetPosition.x < startingPos.x) ? 1 : -1;
        stageCharacterSprite.transform.localScale = characterScale;

        while (elapsedTime < duration) {
            stageCharacterSprite.transform.position = Vector3.Lerp(startingPos, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        stageCharacterSprite.transform.position = targetPosition; 
        moving = false;
    }
    

    /// <summary>
    /// Opens PvM character selection after stage selection.
    /// </summary>
    public void characterSelection(){

        moving = true; 
        hideAll();

        
        

        
        showCharacterSelectionButtons(); 
        titles[3].gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true); 
        stageButton.gameObject.SetActive(true); 
        startPvMFightButton.gameObject.SetActive(true); 
    }

    /// <summary>
    /// Stores PvM selections and loads the combat scene.
    /// </summary>
    public void startPvMFight(){

        saveScript.currentStage = selectedStage;
        saveScript.save();

        selectedCharacters1 = new int[]{character1, character2}; 
        SelectionData.Instance.team1 = selectedCharacters1;
        SelectionData.Instance.isPvP = false;
        SelectionData.Instance.selectedStage = selectedStage + 1; 
        SceneManager.LoadScene("Combat_Scene");
        
    }

    
    private void showCharacterSelectionButtons(){
        
        character1 = 0;
        character2 = 1;
        
        leftInfoPanel.SetActive(true);
        rightInfoPanel.SetActive(true);

        
        leftButton1.gameObject.SetActive(true);
        rightButton1.gameObject.SetActive(true);
        leftButton2.gameObject.SetActive(true);
        rightButton2.gameObject.SetActive(true);

        
        switchCharacter1(character1);
        switchCharacter2(character2);

        
    }

    /// <summary>
    /// Changes selected character for player one preview.
    /// </summary>
    public void switchCharacter1(int newCharacter){
        
        characterSpriteList1[character1].gameObject.SetActive(false);
        characterSpriteList1[newCharacter].gameObject.SetActive(true);
        character1 = newCharacter;
        UpdateDescription(newCharacter, leftNameObj, leftBioObj);
    }

    /// <summary>
    /// Changes selected character for player two preview.
    /// </summary>
    public void switchCharacter2(int newCharacter){
        
        characterSpriteList2[character2].gameObject.SetActive(false);
        characterSpriteList2[newCharacter].gameObject.SetActive(true);
        character2 = newCharacter;
        UpdateDescription(newCharacter, rightNameObj, rightBioObj);
    }

    
    /// <summary>
    /// Resets save progression and refreshes menu state.
    /// </summary>
    public void resetGame(){
        saveScript.ResetSave();
    }

    
    /// <summary>
    /// Hides all menu panels and selection widgets before switching view.
    /// </summary>
    private void hideAll(){
        
        if (pvpButton != null) pvpButton.gameObject.SetActive(false);
        if (pvmButton != null) pvmButton.gameObject.SetActive(false);
        if (menuButton != null) menuButton.gameObject.SetActive(false);
        if (nextPlayerButton != null) nextPlayerButton.gameObject.SetActive(false);
        if (startPvPFightButton != null) startPvPFightButton.gameObject.SetActive(false);
        if (startPvMFightButton != null) startPvMFightButton.gameObject.SetActive(false);
        if (leftButton1 != null) leftButton1.gameObject.SetActive(false);
        if (rightButton1 != null) rightButton1.gameObject.SetActive(false);
        if (leftButton2 != null) leftButton2.gameObject.SetActive(false);
        if (rightButton2 != null) rightButton2.gameObject.SetActive(false);
        if (stageButton != null) stageButton.gameObject.SetActive(false);
        if(muteMusicButton != null) muteMusicButton.gameObject.SetActive(false);
        if (choseStageButton != null) choseStageButton.gameObject.SetActive(false);
        if (stageCharacterSprite != null) stageCharacterSprite.gameObject.SetActive(false);
        if (resetButton != null) resetButton.gameObject.SetActive(false);
        if (resetPanel != null) resetPanel.gameObject.SetActive(false);
        if (applicationQuitButton != null) applicationQuitButton.gameObject.SetActive(false);
        if (leftInfoPanel != null) leftInfoPanel.SetActive(false);
        if (rightInfoPanel != null) rightInfoPanel.SetActive(false);
        if (stageInfoPanel != null) stageInfoPanel.SetActive(false);
        if(instructionPanel != null) instructionPanel.SetActive(false);
        foreach(GameObject s in titles){
            if(s != null){
                s.gameObject.SetActive(false);
            }
        }
        foreach(GameObject s in listMapName){
            if(s != titles[3] && s != null){ 
                s.gameObject.SetActive(false);
            }
        }
        foreach (GameObject s in blockedSpriteList){
            if (s != null) {
                s.gameObject.SetActive(false);
            }
        }
        foreach (GameObject s in stageSpriteList){
            if (s != null) {
                s.gameObject.SetActive(false);
            }
        }
        foreach (GameObject c in characterSpriteList1){
            if (c != null) {
                c.gameObject.SetActive(false);
            }
        }
        foreach (GameObject c in characterSpriteList2){
            if (c != null) {
                c.gameObject.SetActive(false);
            }
        }
    }
}

