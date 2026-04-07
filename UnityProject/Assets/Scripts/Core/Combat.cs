using UnityEngine;
using System.Collections;

public class Combat : MonoBehaviour
{
    // --------------- Attributes ---------------

    private GameObject[] playerList, player2List, enemyList;

    private GameObject selectedCharacter;
    private GameObject[] selectedTargets;
    private int selectedSkill; 

    //private bool waitingForInput;

    private int currentTeam; // the team currently attacking (for now, player1 = 1, player2 = 2 & enemies = -1)
    private bool finishedTurn; // indicates if the "Fin du tour" button is clicked


    // --------------- set() & get() ---------------

    public void setPlayerList(GameObject[] list){ playerList = list; }
    public void setPlayer2List(GameObject[] list){ player2List = list; }
    public void setEnemyList(GameObject[] list){ enemyList = list; }
    public void setSelectedCharacter(GameObject character){ selectedCharacter = character; }
    public void setselectedTargets(GameObject[] target){ selectedTargets = target; }
    public void setSelectedSkill(int skill){ selectedSkill = skill; }
    //public void setWaitingForInput(bool b){ waitingForInput = b; }

    public GameObject[] setPlayerList(){ return playerList; }
    public GameObject[] setPlayer2List(){ return player2List; }
    public GameObject[] setEnemyList(){ return enemyList; }
    public GameObject setSelectedCharacter(){ return selectedCharacter; }
    public GameObject[] setselectedTargets(){ return selectedTargets; }
    public int setSelectedSkill(){ return selectedSkill; }
    //public bool setWaitingForInput(){ return waitingForInput; }


    // --------------- Methods ---------------

    private bool teamDead(GameObject[] team){
        ///<param> team : list of the memebers of a team </param>
        ///<summary> returns true if all the team members are dead, otherwise return false </summary>
        
        if (team == null) return false;

        foreach(GameObject c in team){ 
            if (isDead(c)){ // if a character is alive, then the team is not dead
                return false;
            }
        }
        return true; // otherwise, the team is dead
    }


    private bool isDead(GameObject target){
        ///<param> target : an entity (either a character or an enemy) </param>
        ///<summary> returns true if the charater is dead, otherwise return false </summary>
        
        if (target.GetComponent<Character>() != null){ // if target is a character
            if (target.GetComponent<Character>().getCurrentHP() <= 0) // if hp of the target is 0 (or less)
                return true; // target is dead

        } else if (target.GetComponent<Enemy>() != null){ // if target is an enemy
            if (target.GetComponent<Enemy>().CurrentHP <= 0)  // if hp of the target is 0 (or less)
                return true; // target is dead
        }
        return false; // otherwise, target is alive
    }


    public void startPvPFight(GameObject[] playerList, GameObject[] player2List){
        ///<param> playerList : list of player1's characters, player2List : list of player2's characters </param>
        ///<summary> starts the PvP fight </summary>
    }


    public void startPvMFight(GameObject[] playerList, GameObject[] enemyList){
        ///<param> playerList : list of the player's characters, enemyList : list of the enemies the characters are fighting </param>
        ///<summary> starts the PvM fight </summary>
        
        player2List = null;
        currentTeam = 1; // at the start of the fight, player starts attaking 
        while (!teamDead(playerList) & !teamDead(enemyList)){ // while both teams are alive
            if (currentTeam == 1){ // player's turn
                playerTurn();
            } else if (currentTeam == -1){ // enemies' turn
                enemyTurn();
            }
        }
    }


    private void playerTurn(){
        ///<summary> goes through the character, skill and target selection during the player's turn </summary>
        
        finishedTurn = false;
        while (!finishedTurn){ // until the "Fin du tour" button is clicked
            
            // waits for player to select a character, a skill and a target
            StartCoroutine(waitForInput());

            // applies skill to target(s)
            switch (selectedSkill){
                case 0 : selectedCharacter.GetComponent<Character>().baseAttack(selectedTargets[0]);
                         break;
                case 1 : selectedCharacter.GetComponent<Character>().skillLvl1(selectedTargets[0]);
                         break;
                case 2 : selectedCharacter.GetComponent<Character>().skillLvl2(selectedTargets);
                         break;
                case 3 : selectedCharacter.GetComponent<Character>().skillLvl3(selectedTargets);
                         break;
            }
 
            // if all the characters in a team are dead, the battle is over
            if (teamDead(enemyList) | teamDead(playerList) | teamDead(player2List)){ 
                finishedTurn = true;
            }
        }

        if (player2List == null){
            currentTeam = -1; // enemies' turn
        } else if (currentTeam == 1){
            currentTeam = 2; // player2's turn
        } else {
            currentTeam = 1; // player1's turn
        }
    }
    
    private IEnumerator waitForInput(){
        ///<summary> waits for player to select a character, a skill and a target </summary>

        selectedCharacter = null;
        selectedSkill = -1;
        selectedTargets = null;

        while(selectedCharacter == null & selectedSkill == -1 & selectedTargets == null){ // until the player selects a character, a skill and a target
            if(selectedCharacter != null & selectedSkill != -1){ // once the player selects a character and a skill

                if((selectedCharacter.GetComponent<Mage>() != null & selectedSkill == 3)| // if character is a mage and using skill lvl 3
                   (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 2)| // or if character is a protector and using skill lvl 2 (?)
                   (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 3)){ // or if character is a protector and using skill lvl 3
                    if (player2List == null){
                        selectedTargets = enemyList; // skill affects all enemies (player doesn't need to select target)
                    } else if (currentTeam == 1){
                        selectedTargets = player2List; // skill affects all opponents (player doesn't need to select target)
                    } else {
                        selectedTargets = playerList; // skill affects all opponents (player doesn't need to select target)
                    }
                }

                if((selectedCharacter.GetComponent<Healer>() != null & selectedSkill == 3)){ // if character is a healer and using skill lvl 3
                    if (currentTeam == 1){
                        selectedTargets = playerList; // skill affects all allies (player doesn't need to select target)
                    } else {
                        selectedTargets = player2List; // skill affects all allies (player doesn't need to select target)
                    }
                }
            }
        }

        yield return null;
    }


    private void enemyTurn(){
        ///<summary> makes the enemies attack during the enemies' turn </summary>
    }
}

/*
public IEnumerator combat(){
   // code avant d'attendre l'input du joueur
   waitingForInput = true;
   yield return new WaitUntil(() => (waitingForInput == false)); // attente de l’input
   // code après l'input du joueur
}

public void buttonClicked(){
   combatScript.setPlayerChoice(choice); // informe Combat du choix du joueur
   combatScript.setWaitingForInput(false); // informe combat() qu’il peut continuer 
}
*/


/*          // waits for player to select a character
            selectedCharacter = null;
            StartCoroutine(waitForCharacter());

            // waits for player to select a skill
            selectedSkill = -1;
            StartCoroutine(waitForSkill()); 

            if (attackIsTargeted(selectedCharacter, selectedSkill)){ // if the selected skill of the selected character needs a specific target
                // waits for player to select a target
                selectedTargets = null;
                StartCoroutine(waitForTarget());
            }
*/


/*
    private IEnumerator waitForCharacter(){
        ///<summary> waits for player to select a character </summary>

        waitingForInput = true;
        yield return new WaitUntil(() => (waitingForInput == false & selectedCharacter != null)); // waits for player to select a character
    }

    private IEnumerator waitForSkill(){
        ///<summary> waits for player to select a skill </summary>

        waitingForInput = true;
        yield return new WaitUntil(() => (waitingForInput == false & selectedSkill != -1)); // waits for player to select a skill
    }

    private IEnumerator waitForTarget(){
        ///<summary> // waits for player to select a target </summary>

        waitingForInput = true;
        yield return new WaitUntil(() => (waitingForInput == false & selectedTargets != null)); // waits for player to select a target
    }
*/