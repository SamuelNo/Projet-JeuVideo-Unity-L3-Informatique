using UnityEngine;
using System.Collections;

public class Combat : MonoBehaviour
{
    // --------------- Attributes ---------------
    private BattleUIController buttonScript;

    private GameObject[] playerList, player2List, enemyList;

    private bool PvP, PvM;

    private GameObject selectedCharacter;
    private GameObject[] selectedTargets;
    private int selectedSkill; 

    private bool wait;

    private int turnCount; // increases by 1 whenever a turn is over (the 2 teams have played)
    private bool finishedTurn;
    private GameObject usedCharacter; // saves the character that was just used
    private BattlePhase currentPhase;
    private int currentTeam; // the team currently attacking (for now, player1 = 1, player2 = 2 & enemies = -1)
    private int clickedTeam; // used in select(), saves the team of the clicked entity


    // --------------- set() & get() ---------------
    // sets
    public void setSelectedCharacter(GameObject character){ selectedCharacter = character; }
    public void setSelectedSkill(int skill){ selectedSkill = skill; }
    public void setSelectedTargets(GameObject[] targets){ selectedTargets = targets; }

    //public void setWait(bool x){ wait = x; }
    public void setCurrentPhase(BattlePhase phase){ currentPhase = phase; }
    public void setFinishedTurn(bool x){ finishedTurn = x; }


    // gets
    public GameObject getSelectedCharacter(){ return selectedCharacter; }
    public int getSelectedSkill(){ return selectedSkill; }
    public GameObject[] getSelectedTargets(){ return selectedTargets; }

    public BattlePhase getCurrentPhase(){ return currentPhase; }
    public int getCurrentTeam(){ return currentTeam; }
    public GameObject getUsedCharacter(){ return usedCharacter; }


    // --------------- Initialisation ---------------
    void Awake(){
        buttonScript = Object.FindAnyObjectByType<BattleUIController>(FindObjectsInactive.Exclude);

        turnCount = 0;
        usedCharacter = null;
        currentTeam = 1; // at the start of the fight, player1 starts attaking 
        wait = false;

        PvP = false;
        PvM = false;
    }

    void Reset(){
        Awake();
    }

    // --------------- start fight ---------------
    public void startPvPFight(GameObject[] player1, GameObject[] player2){
        ///<param> playerList : list of player1's characters, player2List : list of player2's characters </param>
        ///<summary> starts the PvP fight </summary>
        
        playerList = player1;
        player2List = player2;
        enemyList = null;

        PvP = true; // starts the battle

        Debug.Log("Le combat PvP commence.");
    }

    public void startPvMFight(GameObject[] players, GameObject[] ennemies){
        ///<param> playerList : list of the player's characters, enemyList : list of the enemies the characters are fighting </param>
        ///<summary> starts the PvM fight </summary>
        
        playerList = players;
        player2List = null;
        enemyList = ennemies;

        PvM = true; // starts the battle

        Debug.Log("Le combat PvM commence.");
    }

    // --------------- Update ---------------
    void FixedUpdate(){
        if (PvP & !wait){ // only enters the loop when a turn is over
            if (teamDead(playerList) | teamDead(player2List)){ // checks if the battle is over
                if (teamDead(player2List)){
                    Debug.Log("Fin du combat, le joueur 1 a gagné.");
                } else {
                    Debug.Log("Fin du combat, le joueur 2 a gagné.");
                }
                wait = true;
            }
            if (currentTeam == 1){ // player1's turn
                if (!teamDead(playerList) & !teamDead(player2List)){ // while both teams are alive
                    StartCoroutine(playerTurn());
                }
            } else if (currentTeam == 2){ // player2's turn
                if (!teamDead(playerList) & !teamDead(player2List)){ // while both teams are alive
                    StartCoroutine(playerTurn());
                }
            }
        }
            
        if (PvM & !wait){ // only enters the loop when a turn is over
            if (teamDead(playerList) | teamDead(enemyList)){ // checks if the battle is over
                if (teamDead(enemyList)){
                    Debug.Log("Fin du combat, le joueur a gagné.");
                } else {
                    Debug.Log("Fin du combat, l'ennemi a gagné.");
                }
                wait = true;
            }
            if (currentTeam == 1){ // player's turn
                if (!teamDead(playerList) & !teamDead(enemyList)){ // while both teams are alive
                    StartCoroutine(playerTurn());
                } 
            } else if (currentTeam == -1){ // enemy's turn
                if (!teamDead(playerList) & !teamDead(enemyList)){ // while both teams are alive
                    enemyTurn();
                }
            }
        }
    }

    // --------------- Methods ---------------
    private IEnumerator playerTurn(){
        ///<summary> goes through the character, skill and target selection during the player's turn </summary>
        
        Debug.Log("Tour de l'équipe " + currentTeam + ".");
        wait = true;

        finishedTurn = false;
        foreach (GameObject c in playerList){ // once per character that is alive 
            if (!finishedTurn & !isDead(c)){ // or until the "Fin du tour" button is clicked

                // waits for player to select a character, a skill and a target
                selectedCharacter = null;
                selectedSkill = -1;
                selectedTargets = null;

                // waits for player to select a character
                currentPhase = BattlePhase.SELECT_CHARACTER;
                buttonScript.ButtonAccess();
                Debug.Log("Veuillez selectionner un personnage.");
                yield return new WaitUntil(() => (selectedCharacter != null) | finishedTurn);
                if (finishedTurn) break; // if the "Fin du tour" button was clicked, end the turn
        
                // waits for player to select a skill
                yield return new WaitUntil(() => (selectedSkill != -1 | finishedTurn));
                if (finishedTurn) break; // if the "Fin du tour" button was clicked, end the turn

                // waits for player to select a target (if the automatic target selection hasn't happened)
                if (selectedTargets == null) {
                    yield return new WaitUntil(() => (selectedTargets != null | finishedTurn));
                    if (finishedTurn) break; // if the "Fin du tour" button was clicked, end the turn
                }

                // applies skill to target(s)
                Character characterScript = selectedCharacter.GetComponent<Character>();
                switch (selectedSkill){
                    case 0 : characterScript.baseAttack(selectedTargets[0]);
                             Debug.Log("Attaque basique réussie, lancée par " + selectedCharacter.name + " sur " + selectedTargets[0].name);
                             break;
                    case 1 : characterScript.skillLvl1(selectedTargets[0]);
                             Debug.Log("Compétence niveau 1 réussie, lancée par " + selectedCharacter.name + " sur " + selectedTargets[0].name);
                             break;
                    case 2 : characterScript.skillLvl2(selectedTargets);
                             Debug.Log("Compétence niveau 2 réussie, lancée par " + selectedCharacter.name);
                             break;
                    case 3 : characterScript.skillLvl3(selectedTargets);
                             Debug.Log("Compétence niveau 3 réussie, lancée par " + selectedCharacter.name);
                             break;
                }

                if (usedCharacter != null){ // if the two characters have been used, the player's turn is over
                    usedCharacter = null;
                    finishedTurn = true;
                } else {
                    usedCharacter = selectedCharacter; // otherwise, the character that has just been used is saved
                }

                // if a team is dead, the battle is over
                if (teamDead(enemyList) | teamDead(playerList) | teamDead(player2List)){ 
                    finishedTurn = true;
                }
            }
        }

        // switch to the other team
        if (player2List == null){
            currentTeam = -1; // enemies' turn
        } else if (currentTeam == 1){
            currentTeam = 2; // player2's turn
        } else {
            currentTeam = 1; // player1's turn
        }

        // increments turnCount
        if (currentTeam == 1){ // after the 2 teams have played
            turnCount += 1;
        }

        wait = false;
        yield return null;
    }
    
    private void enemyTurn(){
        ///<summary> makes the enemies attack during the enemies' turn </summary>
        
        Debug.Log("Tour de l'ennemi.");
        
        // implémenter l'ia ici
        // ...
    }

    private bool teamDead(GameObject[] team){
        ///<param> team : list of the memebers of a team </param>
        ///<summary> returns true if all the team members are dead, otherwise return false </summary>
        
        if (team == null) return false;

        foreach(GameObject c in team){ 
            if (!isDead(c)){ // if a character is alive, then the team is not dead
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
                //Debug.Log(target.name + " is dead");
                return true; // target is dead

        } else if (target.GetComponent<Enemy>() != null){ // if target is an enemy
            if (target.GetComponent<Enemy>().CurrentHP <= 0)  // if hp of the target is 0 (or less)
                //Debug.Log(target.name + " is dead");
                return true; // target is dead
        }
        return false; // otherwise, target is alive
    }

    public void automaticTargetSelection(){
        ///<summary> selects all opponents/allies as targets depending on the selected character and skill </summary>
        
        if ((selectedCharacter.GetComponent<Mage>() != null & selectedSkill == 3)| // if character is a mage and using skill lvl 3
           (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 2)| // or if character is a protector and using skill lvl 2
           (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 3)){ // or if character is a protector and using skill lvl 3
            // skill affects all opponents (player doesn't need to select target)
            if (player2List == null){ // if it's a PvM
                selectedTargets = enemyList;
            } else if (currentTeam == 1){
                selectedTargets = player2List;
            } else {
                selectedTargets = playerList;
            }
            Debug.Log("Tous les ennemis ont étés ciblés. (La compétence affecte tous les ennemis)");

        } else if ((selectedCharacter.GetComponent<Healer>() != null & selectedSkill == 3)){ // if character is a healer and using skill lvl 3
            // skill affects all allies (player doesn't need to select target)
            if (currentTeam == 1){
                selectedTargets = playerList;
            } else {
                selectedTargets = player2List;
            }
            Debug.Log("Tous les alliés ont étés ciblés. (La compétence affecte tous les alliés)");

        } else if ((selectedCharacter.GetComponent<Fighter>() != null & selectedSkill == 2)){ // if character is a fighter and using skill lvl 2
            // skill affects themselves 
            selectedTargets = new GameObject[] {selectedCharacter};
            Debug.Log("Il n'y a pas besoin de sélectionner une cible. (La compétence n'a pas besoin de cible)");
        }
    }

    public void select(GameObject clickedObject){
        ///<summary> handles the character and target selection </summary>

        clickedTeam = (clickedObject.GetComponent<Character>() != null) ? clickedObject.GetComponent<Character>().getTeamID() : -1; // gets the team id of the clicked character

        if (currentPhase == BattlePhase.SELECT_CHARACTER | currentPhase == BattlePhase.SELECT_SKILL){
            if (clickedTeam == currentTeam){
                if (clickedObject != usedCharacter){ // if the character is allowed to be selected, select the character
                    selectedCharacter = clickedObject;
                    Debug.Log("Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");

                    currentPhase = BattlePhase.SELECT_SKILL;
                    buttonScript.ButtonAccess();


                } else { // the character was already used, nothing happens
                    Debug.LogWarning("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                }

            } else { // the character/enemy is not on the right team, nothing happens
                Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
            }


        } else if (currentPhase == BattlePhase.SELECT_TARGET){
            if (clickedTeam == currentTeam){ // if character is an ally
                if (selectedCharacter.GetComponent<Healer>() | (selectedCharacter.GetComponent<Protector>() & selectedSkill != 2)){ // if an ally can be targeted, target the character
                    selectedTargets = new GameObject[] {clickedObject};
                    //Debug.Log("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");

                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();


                } else if (clickedObject != usedCharacter){ // if the character wasn't used before, switch to character
                    selectedCharacter = clickedObject;
                    Debug.Log("Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");

                    currentPhase = BattlePhase.SELECT_SKILL;
                    buttonScript.ButtonAccess();


                } else { // the character cannot be selected
                    Debug.LogWarning("Ce personnage fait partie de votre équipe, vous ne pouvez pas l'attaquer. Veuillez choisir un autre personnage.");
                }

            } else { // if character/enemy is an opponent
                if (selectedCharacter.GetComponent<Healer>() | (selectedCharacter.GetComponent<Protector>() & selectedSkill != 2)){ // if the target has to be an ally, nothing happens
                    Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le clibler (compétence aidant un allié). Veuillez choisir un autre personnage.");

                } else { // if the character/enemy can be targetted, target the character/enemy
                    selectedTargets = new GameObject[] {clickedObject};
                    //Debug.Log("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");

                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();
                }
            }
        }
    }
}