using UnityEngine;
using UnityEngine.InputSystem;

public class MenuButtons : MonoBehaviour
{
    // ---------- Attributes ---------- //

    // script access
    private GameObject scriptsGameObject;
    private Principal principalScript;
    private Combat combatScript;

    // player selection data
    private int character1, character2;
    

    // ---------- Initialisation ---------- //

    void Awake(){
        scriptsGameObject = GameObject.Find("Script"); // to access scripts (make sure to have a gameobject named "Script")
        principalScript = scriptsGameObject.GetComponent(typeof(Principal)) as Principal; // to access Principal script
        combatScript = scriptsGameObject.AddComponent(typeof(Combat)) as Combat; // to access combat script
    }

    void Reset(){
        Awake();
    }


    // ---------- Methods ---------- //

    // ----- menu buttons ---------------
    public void PvPButton(){
        ///<summary> starts the character selection for 2 players </summary>

        principalScript.characterSelection1();
    }

    public void PvMButton(){
        ///<summary> starts the stage and character selection </summary>

        principalScript.stageSelection();
    }

    // ----- back to menu button ---------------
    public void backToMenuButton(){
        ///<summary> goes back to main menu </summary>

        principalScript.menu();
    }

    // ----- character selection buttons ---------------
    public void characterButton1(bool left){
        ///<param> left : weither the button is the left arrow or not </param> 
        ///<summary> switches the character appearing on the left side during the character selection </summary>
        
        character1 = principalScript.getCharacter1();
        character1 = left ? (character1+3)%4 : (character1+1)%4; // chose the next character
        if (character1 == principalScript.getCharacter2()){ // if the character is already taken by the other slot
            character1 = left ? (character1+3)%4 : (character1+1)%4; // chose the next character again
        }
        principalScript.switchCharacter1(character1); // replace the old character with the new one
    }

    public void characterButton2(bool left){
        ///<param> left : weither the button is the left arrow or not </param> 
        ///<summary> switches the character appearing on the right side during the character selection </summary>
        
        character2 = principalScript.getCharacter2();
        character2 = left ? (character2+3)%4 : (character2+1)%4; // chose the next character
        if (character2 == principalScript.getCharacter1()){ // if the character is already taken by the other slot
            character2 = left ? (character2+3)%4 : (character2+1)%4; // chose the next character again
        }
        principalScript.switchCharacter2(character2); // replace the old character with the new one
    }

    // ----- PvP buttons ---------------
    public void nextPlayerButton(){
        ///<summary> saves the characters selected by the 1st player and moves on to the 2nd character selection </summary>

        principalScript.setSelectedCharacters1(new int[]{principalScript.getCharacter1(), principalScript.getCharacter2()}); // saves characters
        Debug.Log("Player1 characters : " + principalScript.getcharacterSpriteList1()[principalScript.getSelectedCharacters1()[0]].gameObject.name + ", " + principalScript.getcharacterSpriteList2()[principalScript.getSelectedCharacters1()[1]].gameObject.name);

        principalScript.characterSelection2();
    }

    public void startPvPFightButton(){
        ///<summary> saves the characters selected by the 2nd player and moves on to the PvP combat </summary>

        principalScript.setSelectedCharacters2(new int[]{principalScript.getCharacter1(), principalScript.getCharacter2()}); // saves characters
        Debug.Log("Player2 characters : " + principalScript.getcharacterSpriteList1()[principalScript.getSelectedCharacters2()[0]].gameObject.name + ", " + principalScript.getcharacterSpriteList2()[principalScript.getSelectedCharacters2()[1]].gameObject.name);

        //combatScript.startPvPFight(); // starts fight
        Debug.Log("PvP fight has started");
    }

    // ----- PvM buttons ---------------
    /*
    public void stageButton(int stage){
        ///<param> stage : the number refering to the stage </param> 
        ///<summary> saves the stage selected by the player and moves the character towards said stage </summary>

        principalScript.setSelectedStage(stage);
        Debug.Log("Selected stage : " + principalScript.getSelectedStage());

        StartCoroutine(principalScript.moveCharacter());
    }
    */
    public void choseStageButton(){
        ///<summary> moves on to the character selection </summary>

        principalScript.characterSelection();
    }


    public void startPvMFightButton(){
        ///<summary> saves the characters selected by the player and moves on to the PvM fight  </summary>

        principalScript.setSelectedCharacters1(new int[]{principalScript.getCharacter1(), principalScript.getCharacter1()}); // saves characters
        Debug.Log("Selected characters : " + principalScript.getcharacterSpriteList1()[principalScript.getSelectedCharacters1()[0]].gameObject.name + ", " + principalScript.getcharacterSpriteList2()[principalScript.getSelectedCharacters1()[1]].gameObject.name);

        //stageList[selectedStage].start(); // displays stage
        //combatScript.startPvMFight(); // starts fight
        Debug.Log("PvM fight has started");
    }
}
