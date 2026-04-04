using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class Principal : MonoBehaviour
{
    // ---------- Attributes ---------- //

    // script access
    private GameObject scriptsGameObject; 
    private MenuButtons buttonScript; 
    private Combat combatScript;

    // player selection data
    private int[] selectedCharacters1, selectedCharacters2; // saved characters for player1 and player2 respectively
    private int selectedStage, character1, character2; 

    // character selection data
    //[SerializeField] 
    private GameObject fighterSprite1, healerSprite1, mageSprite1, protectorSprite1, // character sprites
                       fighterSprite2, healerSprite2, mageSprite2, protectorSprite2;
    private GameObject[] characterSpriteList1, characterSpriteList2; // list of character sprites

    // stage selection data
    //[SerializeField] 
    private GameObject stageCharacterSprite; // the character that will be moving on the map
    private bool moving; // indicates if the character is moving (used for player inputs)
    //[SerializeField]
    private GameObject stageSprite0, stageSprite1, stageSprite2; // stage sprites
    private GameObject[] stageSpriteList; // list of stage sprites
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
                   rightButton2; // character selection button that switches the character appearing on the right side of the screen
    
    
    // ---------- Set and Get ---------- //
    // set
    public void setSelectedCharacters1(int[] list){ selectedCharacters1 = list; }
    public void setSelectedCharacters2(int[] list){ selectedCharacters2 = list; }
    public void setSelectedStage(int s){ selectedStage = s; }
    public void setCharacter1(int s){ character1 = s; }
    public void setCharacter2(int s){ character2 = s; }
    public void setMoving(bool b){ moving = b; }

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
    

    // ---------- Initialisation ---------- //

    void Awake(){
        // ----- script access ----------
        scriptsGameObject = this.gameObject;
        buttonScript = scriptsGameObject.AddComponent<MenuButtons>(); // to access MenuButtons script
        combatScript = scriptsGameObject.AddComponent<Combat>();


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

        // other
        selectedStage = 0;
    }

    void Reset(){
        Awake();
    }


    // ---------- Start ---------- //

    void Start(){
        menu();
    }

    // ---------- Key inputs ---------- //

    public void leftArrow(InputAction.CallbackContext ctx){
        ///<summary> changes the selected stage and moves the character towards said stage </summary>
        if (ctx.started){ 
            if (!moving){ // stops code from changing selected stage outside of stage selection, or when character is moving
                if (selectedStage > 0)
                    selectedStage -= 1; // chose the previous stage
                StartCoroutine(moveCharacter()); // moves the character towards the selected stage
            }
        }
    }

    public void rightArrow(InputAction.CallbackContext ctx){
        ///<summary> changes the selected stage and moves the character towards said stage </summary>
        if (ctx.started){ 
            if (!moving){ // stops code from changing selected stage outside of stage selection, or when character is moving
                if (selectedStage < stageSpriteList.Length-1) 
                    selectedStage += 1; // chose the next stage
                StartCoroutine(moveCharacter()); // moves the character towards the selected stage
            }
        }
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
        
        //stageList[selectedStage].setEnemyList(selectedCharacters2); // ?
        //stageList[selectedStage].start(); // displays stage
        //combatScript.startPvPFight(); // starts fight
    }

    // ----- PvM mode ----------
    public void stageSelection(){
        ///<summary> puts all element of stage selection in place and displays them </summary>

        moving = false; // allows the code to change the selected stage
        hideAll();

        // positions will be implemented here
        stageCharacterSprite.transform.position = stageSpriteList[selectedStage].transform.position;
        /*
        foreach (GameObject s in stageSpriteList){ 
            s.gameObject.transform.position = ...;
        }
        */

        // show components
        menuButton.gameObject.SetActive(true); // shows menu button
        choseStageButton.gameObject.SetActive(true); // shows stage validation button
        stageCharacterSprite.gameObject.SetActive(true); // shows character sprite
        foreach (GameObject s in stageSpriteList){ // shows stage sprites
            s.gameObject.SetActive(true);
        }
    }

    public IEnumerator moveCharacter(){
        ///<summary> moves the stage selection character sprite towards the selected stage </summary>
        Vector3 targetPosition = stageSpriteList[selectedStage].transform.position,
                characterScale = stageCharacterSprite.transform.localScale,
                direction = targetPosition - stageCharacterSprite.transform.position;

        // makes the character face the selected stage's direction
        characterScale.x = targetPosition.x < stageCharacterSprite.transform.position.x ? -1 : 1; // character faces left if stage is on the left, otherwise character faces right
        stageCharacterSprite.transform.localScale = characterScale; // flips the character towards the correct direction

        // moves the character towards the selected stage
        moving = true; // stops code from changing selected stage while moving
        while (Vector3.Distance(stageCharacterSprite.transform.position, targetPosition) > 1f){ // stops when character is close to the target
            stageCharacterSprite.transform.position = Vector3.MoveTowards(stageCharacterSprite.transform.position, targetPosition, 500f * Time.deltaTime); // calculates the position of the character
            yield return null; // applies the calculated position to the character
        }
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

        selectedCharacters1 = new int[]{character1, character2}; // saves characters
        
        //stageList[selectedStage].start(); // displays stage
        //combatScript.startPvMFight(); // starts fight
    }

    // ----- character selection ----------
    private void showCharacterSelectionButtons(){
        ///<summary> shows the buttons and characters of the character selection screen </summary>
        
        character1 = 0;
        character2 = 1;
        
        // positions will be implemented here
        // ...

        // show buttons
        leftButton1.gameObject.SetActive(true);
        rightButton1.gameObject.SetActive(true);
        leftButton2.gameObject.SetActive(true);
        rightButton2.gameObject.SetActive(true);

        // show characters
        switchCharacter1(character1);
        switchCharacter2(character2);
    }

    public void switchCharacter1(int newCharacter){
        ///<param> newCharacter : index of the new character that will appear on the screen </param> 
        ///<summary> switches the character on the left with a new one </summary>
        
        characterSpriteList1[character1].gameObject.SetActive(false);
        characterSpriteList1[newCharacter].gameObject.SetActive(true);
        character1 = newCharacter;
    }

    public void switchCharacter2(int newCharacter){
        ///<param> newCharacter : index of the new character that will appear on the screen </param> 
        ///<summary> switches the character on the left with a new one </summary>
        
        characterSpriteList2[character2].gameObject.SetActive(false);
        characterSpriteList2[newCharacter].gameObject.SetActive(true);
        character2 = newCharacter;
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

