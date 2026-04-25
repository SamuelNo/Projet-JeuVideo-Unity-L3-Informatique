using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Principal : MonoBehaviour
{
    // ---------- Attributes ---------- //

    // script access
    private MenuButtons buttonScript;
    private Save saveScript;

    // player selection data
    private int[] selectedCharacters1, selectedCharacters2; // saved characters for player1 and player2 respectively
    private int selectedStage, character1, character2; 

    // character selection data
    //[SerializeField] 
    private GameObject fighterSprite1, healerSprite1, mageSprite1, protectorSprite1, // character sprites
                       fighterSprite2, healerSprite2, mageSprite2, protectorSprite2;
    private GameObject[] characterSpriteList1, characterSpriteList2; // list of character sprites
    private GameObject[] teamPlayer1, teamPlayer2; // list of the characters of player1 and player2 respectively (might be removed) 

    // stage selection data
    //[SerializeField] 
    private GameObject stageCharacterSprite; // the character that will be moving on the map
    private bool moving; // indicates if the character is moving (used for player inputs)
    //[SerializeField]
    private GameObject stageSprite0, stageSprite1, stageSprite2; // stage sprites
    private GameObject[] stageSpriteList; // list of stage sprites
    [SerializeField] private int unlockedStage; // last stage that was unlocked
    //private Stage[] stageList;

    // other visual components
    //[SerializeField] 
    private Button pvpButton, // PvP button on the main menu screen
                   pvmButton, // PvM button on the main menu screen
                   menuButton, // lets the player go back to the main menu
                   nextPlayerButton, // after chosing the PvP option, lets the player move on to the 2nd player's character selection
                   startPvPFightButton, // after chosing the PvP option, lets the players start the PvP fight
                   choseStageButton, // after chosing the PvM option and selecting a stage, moves to the character selection (might be removed)
                   stageButton, // lets the player go back to the stage selection
                   startPvMFightButton, // after chosing the PvM option, lets the players start the PvM fight
                   leftButton1, // character selection button that switches the character appearing on the left side of the screen
                   rightButton1, // character selection button that switches the character appearing on the left side of the screen
                   leftButton2, // character selection button that switches the character appearing on the right side of the screen
                   rightButton2, // character selection button that switches the character appearing on the right side of the screen
                   resetButton,
                   resetYesButton,
                   resetNoButton;
    private GameObject resetPanel;
    [SerializeField] private GameObject[] characterBasePrefabs; // list of character prefabs (used for getting character data like description and skill names)
    [SerializeField] public GameObject[] monsterPrefabs; // list of monster prefabs (might be used for getting monster data like description and skill names)
    public GameObject leftInfoPanel, rightInfoPanel;
    public GameObject leftNameObj, leftBioObj, rightNameObj, rightBioObj;
    public GameObject stageInfoPanel; 
    public GameObject stageEnemyListObj; 
    public GameObject[] listMapName = new GameObject[3]; // list of map name objects (used for displaying the name of the stage)
    // ---------- Set and Get ---------- //
    // set
    public void setSelectedCharacters1(int[] list){ selectedCharacters1 = list; }
    public void setSelectedCharacters2(int[] list){ selectedCharacters2 = list; }
    public void setSelectedStage(int s){ selectedStage = s; }
    public void setCharacter1(int s){ character1 = s; }
    public void setCharacter2(int s){ character2 = s; }
    public void setMoving(bool b){ moving = b; }
    public void setUnlockedStage(int s){ unlockedStage = s; }

    // get
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
    

    // ---------- Initialisation ---------- //
    void Awake(){
        // ----- script access ----------
        buttonScript = this.gameObject.AddComponent<MenuButtons>(); // to access MenuButtons script
        saveScript = this.gameObject.AddComponent<Save>();


        // ----- button initialisation ----------
        // finds each component that will be used (make sure that all components have the correct name)
        pvpButton = GameObject.Find("PvPButton").GetComponent<Button>();
        pvmButton = GameObject.Find("PvMButton").GetComponent<Button>();
        menuButton = GameObject.Find("MenuButton").GetComponent<Button>();
        nextPlayerButton = GameObject.Find("NextPlayerButton").GetComponent<Button>();
        startPvPFightButton = GameObject.Find("StartPvPFightButton").GetComponent<Button>();
        startPvMFightButton = GameObject.Find("StartPvMFightButton").GetComponent<Button>();
        leftButton1 = GameObject.Find("LeftButton1").GetComponent<Button>();
        rightButton1 = GameObject.Find("RightButton1").GetComponent<Button>();
        leftButton2 = GameObject.Find("LeftButton2").GetComponent<Button>();
        rightButton2 = GameObject.Find("RightButton2").GetComponent<Button>();
        stageButton = GameObject.Find("StageButton").GetComponent<Button>();
        choseStageButton = GameObject.Find("ChoseStageButton").GetComponent<Button>();
        resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
        resetYesButton = GameObject.Find("ResetYesButton").GetComponent<Button>();
        resetNoButton = GameObject.Find("ResetNoButton").GetComponent<Button>();

        resetPanel = GameObject.Find("ResetPanel");

        // adds OnClick() to each button
        pvpButton.onClick.AddListener(delegate{ buttonScript.PvPButton(); });
        pvmButton.onClick.AddListener(delegate{ buttonScript.PvMButton(); });
        menuButton.onClick.AddListener(delegate{ buttonScript.backToMenuButton(); });
        nextPlayerButton.onClick.AddListener(delegate{ buttonScript.nextPlayerButton(); });
        startPvPFightButton.onClick.AddListener(delegate{ buttonScript.startPvPFightButton(); });
        startPvMFightButton.onClick.AddListener(delegate{ buttonScript.startPvMFightButton(); });
        leftButton1.onClick.AddListener(delegate{ buttonScript.characterButton1(true); });
        rightButton1.onClick.AddListener(delegate{ buttonScript.characterButton1(false); });
        leftButton2.onClick.AddListener(delegate{ buttonScript.characterButton2(true); });
        rightButton2.onClick.AddListener(delegate{ buttonScript.characterButton2(false); });
        stageButton.onClick.AddListener(delegate{ buttonScript.PvMButton(); });
        choseStageButton.onClick.AddListener(delegate{ buttonScript.choseStageButton(); });
        resetButton.onClick.AddListener(delegate{ buttonScript.resetButton(resetPanel); });
        resetYesButton.onClick.AddListener(delegate{ buttonScript.confirmReset(resetPanel); });
        resetNoButton.onClick.AddListener(delegate{ buttonScript.refuseReset(resetPanel); });


        // ----- character initialisation ----------
        fighterSprite1 = GameObject.Find("FighterSpriteLeft");
        healerSprite1 = GameObject.Find("HealerSpriteLeft");
        mageSprite1 = GameObject.Find("MageSpriteLeft");
        protectorSprite1 = GameObject.Find("ProtectorSpriteLeft");
        fighterSprite2 = GameObject.Find("FighterSpriteRight");
        healerSprite2 = GameObject.Find("HealerSpriteRight");
        mageSprite2 = GameObject.Find("MageSpriteRight");
        protectorSprite2 = GameObject.Find("ProtectorSpriteRight");
        characterSpriteList1 = new GameObject[] {fighterSprite1, healerSprite1, mageSprite1, protectorSprite1};
        characterSpriteList2 = new GameObject[] {fighterSprite2, healerSprite2, mageSprite2, protectorSprite2};


        // ----- stage initialisation ----------
        stageSprite0 = GameObject.Find("StageSprite0");
        stageSprite1 = GameObject.Find("StageSprite1");
        stageSprite2 = GameObject.Find("StageSprite2");
        stageCharacterSprite = GameObject.Find("StageCharacterSprite");
        
        stageSpriteList = new GameObject[] {stageSprite0, stageSprite1, stageSprite2};
    }

    void Reset(){
        Awake();
    }


    // ---------- Start ---------- //

    void Start(){
        saveScript.loadFile();
        menu();
    }

    // ---------- Key inputs ---------- //

    public void leftArrow(InputAction.CallbackContext ctx){
        ///<summary> changes the selected stage and moves the character towards said stage </summary>
        if (ctx.started){ 
            if (!moving){ // stops code from changing selected stage outside of stage selection, or when character is moving
                if (selectedStage > 0)
                    selectedStage -= 1; // chose the previous stage
                    UpdateStageInfo(); // updates stage info panel
                StartCoroutine(moveCharacter()); // moves the character towards the selected stage
            }
        }
    }

    public void rightArrow(InputAction.CallbackContext ctx){
        ///<summary> changes the selected stage and moves the character towards said stage </summary>
        if (ctx.started){ 
            if (!moving){ // stops code from changing selected stage outside of stage selection, or when character is moving
                if (selectedStage < unlockedStage) 
                    selectedStage += 1; // chose the next stage
                    UpdateStageInfo(); // updates stage info panel
                StartCoroutine(moveCharacter()); // moves the character towards the selected stage
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
            enemiesToShow = new int[] { 2, 3, 3 }; 
            break;
        case 2:
            enemiesToShow = new int[] { 4 };
            break;
        default:
            enemiesToShow = new int[] { };
            break;
    }


    foreach (int index in enemiesToShow) {
        if (index < monsterPrefabs.Length) {
        
            Enemy data = monsterPrefabs[index].GetComponent<Enemy>();
            if (data != null) {
                enemyText += "- " + data.enemyName + "\n"+ data.enemyDescription + "\n\n";
            }
        }
    }

    
    stageEnemyListObj.GetComponent<UnityEngine.Component>().SendMessage("set_text", enemyText);
    }

    // ---------- Methods ---------- //

    public void menu(){
        ///<summary> Puts all element of menu in place and displays them, then waits for player to chose PvP or PvM </summary>
        
        moving = true; // stops code from changing selected stage outside of stage selection
        hideAll(); // hides all components that might be active

        // positions will be implemented here
        // ...

        // show components
        pvmButton.gameObject.SetActive(true); // shows PvP button
        pvpButton.gameObject.SetActive(true); // shows PvM button
        resetButton.gameObject.SetActive(true); // shows PvM button

    }

    // ----- PvP mode ----------
    public void characterSelection1(){
        ///<summary> puts all element of character selection in place and displays them (PvP mode, player 1) </summary>

        hideAll();

        // positions will be implemented here
        // ...

        // show components
        showCharacterSelectionButtons(); // shows character selection buttons and characters

        menuButton.gameObject.SetActive(true); // shows menu button
        nextPlayerButton.gameObject.SetActive(true); // shows next player button
    }

    public void characterSelection2(){
        ///<summary> puts all element of character selection in place and displays them (PvP mode, player 2) </summary>

        selectedCharacters1 = new int[]{character1, character2}; // saves characters
        

        hideAll();

        // positions will be implemented here
        // ...

        // show components
        showCharacterSelectionButtons(); // shows character selection buttons and characters

        menuButton.gameObject.SetActive(true); // shows menu button
        startPvPFightButton.gameObject.SetActive(true); // shows startPvPFight button
    }

    public void startPvPFight(){
        ///<summary> displays the stage and starts the PvP fight </summary>
        
        selectedCharacters2 = new int[]{character1, character2}; // saves characters
        SelectionData.Instance.team1 = selectedCharacters1;
        SelectionData.Instance.team2 = selectedCharacters2;
        SelectionData.Instance.isPvP = true;
        SelectionData.Instance.selectedStage = 0;
        SceneManager.LoadScene("Combat_Scene");
    }
    
    // ----- PvM mode ----------
    public void stageSelection(){
        ///<summary> puts all element of stage selection in place and displays them </summary>

        moving = false; // allows the code to change the selected stage
        hideAll();

        // positions will be implemented here
        stageCharacterSprite.transform.position = stageSpriteList[selectedStage].transform.position;
        UpdateStageInfo();

        // show components
        menuButton.gameObject.SetActive(true); // shows menu button
        choseStageButton.gameObject.SetActive(true); // shows stage validation button
        stageCharacterSprite.gameObject.SetActive(true); // shows character sprite
        stageInfoPanel.SetActive(true); // shows stage info panel
        foreach (GameObject s in stageSpriteList){ // shows stage sprites
            s.gameObject.SetActive(true);
        }
        foreach(GameObject s in listMapName){
            s.gameObject.SetActive(true);
        }
        
        // makes the locked stages grey
        for (int i=0; i<stageSpriteList.Length; i++){
            if (i > unlockedStage){
                stageSpriteList[i].GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.7f); 
            } else {
                stageSpriteList[i].GetComponent<Image>().color = Color.white; 
            }
        }
        
    }

    public IEnumerator moveCharacter(){
    ///<summary> moves the stage selection character sprite towards the selected stage </summary>
        Vector3 targetPosition = stageSpriteList[selectedStage].transform.position;
        Vector3 characterScale = stageCharacterSprite.transform.localScale;

        if (targetPosition.x < stageCharacterSprite.transform.position.x) {
            characterScale.x = 1; 
        } 
        else if (targetPosition.x > stageCharacterSprite.transform.position.x) {
            characterScale.x = -1; 
        }

        stageCharacterSprite.transform.localScale = characterScale;

        moving = true;
        while (Vector3.Distance(stageCharacterSprite.transform.position, targetPosition) > 1f) {
            stageCharacterSprite.transform.position = Vector3.MoveTowards(stageCharacterSprite.transform.position, targetPosition, 500f * Time.deltaTime);
            yield return null;
        }
        if (selectedStage == 0) {
            characterScale.x = 1; 
        } 
        else if (selectedStage == stageSpriteList.Length - 1) {
            characterScale.x = -1;
        }

        stageCharacterSprite.transform.localScale = characterScale;
        moving = false;
    }
    

    public void characterSelection(){
        ///<summary> puts all element of character selection in place and displays them (PvM mode) </summary>

        moving = true; // stops code from changing selected stage outside of stage selection
        hideAll();

        // positions will be implemented here
        // ...

        // show components
        showCharacterSelectionButtons(); // shows character selection buttons and characters
        
        menuButton.gameObject.SetActive(true); // shows menu button
        stageButton.gameObject.SetActive(true); // shows back to stage button
        startPvMFightButton.gameObject.SetActive(true); // shows startPvMFight button
    }

    public void startPvMFight(){
        ///<summary> displays the stage and starts the PvP fight </summary>

        saveScript.currentStage = selectedStage;
        saveScript.save();

        selectedCharacters1 = new int[]{character1, character2}; // saves characters
        SelectionData.Instance.team1 = selectedCharacters1;
        SelectionData.Instance.isPvP = false;
        SelectionData.Instance.selectedStage = selectedStage + 1; // because stages are indexed from 0 but stage selection is indexed from 1
        SceneManager.LoadScene("Combat_Scene");
        //combatScript.startPvMFight(); // starts fight
    }

    // ----- character selection ----------
    private void showCharacterSelectionButtons(){
        ///<summary> shows the buttons and characters of the character selection screen </summary>
        
        character1 = 0;
        character2 = 1;
        
        leftInfoPanel.SetActive(true);
        rightInfoPanel.SetActive(true);

        // show buttons
        leftButton1.gameObject.SetActive(true);
        rightButton1.gameObject.SetActive(true);
        leftButton2.gameObject.SetActive(true);
        rightButton2.gameObject.SetActive(true);

        // show characters
        switchCharacter1(character1);
        switchCharacter2(character2);

        // show info panels
    }

    public void switchCharacter1(int newCharacter){
        ///<param> newCharacter : index of the new character that will appear on the screen </param> 
        ///<summary> switches the character on the left with a new one </summary>
        
        characterSpriteList1[character1].gameObject.SetActive(false);
        characterSpriteList1[newCharacter].gameObject.SetActive(true);
        character1 = newCharacter;
        UpdateDescription(newCharacter, leftNameObj, leftBioObj);
    }

    public void switchCharacter2(int newCharacter){
        ///<param> newCharacter : index of the new character that will appear on the screen </param> 
        ///<summary> switches the character on the left with a new one </summary>
        
        characterSpriteList2[character2].gameObject.SetActive(false);
        characterSpriteList2[newCharacter].gameObject.SetActive(true);
        character2 = newCharacter;
        UpdateDescription(newCharacter, rightNameObj, rightBioObj);
    }

    // ----- reset game ----------
    public void resetGame(){
        saveScript.reset();
    }

    // ----- general ----------
    private void hideAll(){
        ///<summary> hides all components </summary>
        
        pvpButton.gameObject.SetActive(false);
        pvmButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(false);
        nextPlayerButton.gameObject.SetActive(false);
        startPvPFightButton.gameObject.SetActive(false);
        startPvMFightButton.gameObject.SetActive(false);
        leftButton1.gameObject.SetActive(false);
        rightButton1.gameObject.SetActive(false);
        leftButton2.gameObject.SetActive(false);
        rightButton2.gameObject.SetActive(false);
        stageButton.gameObject.SetActive(false);
        choseStageButton.gameObject.SetActive(false);
        stageCharacterSprite.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        resetPanel.gameObject.SetActive(false);
        leftInfoPanel.SetActive(false);
        rightInfoPanel.SetActive(false);
        stageInfoPanel.SetActive(false);
        foreach(GameObject s in listMapName){
            s.gameObject.SetActive(false);
        }
        foreach (GameObject s in stageSpriteList){
            s.gameObject.SetActive(false);
        }
        foreach (GameObject c in characterSpriteList1){
            c.gameObject.SetActive(false);
        }
        foreach (GameObject c in characterSpriteList2){
            c.gameObject.SetActive(false);
        }
    }
}

