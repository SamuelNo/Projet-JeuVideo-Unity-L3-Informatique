using UnityEngine;
using System.Collections;
using Codice.Client.BaseCommands.Merge.Xml;
using System.Linq;
using System.Collections.Generic;


// TO DO : undo FROZEN state after 1 turn

public class Combat : MonoBehaviour
{
    // --------------- Attributes ---------------
    // script access
    // script access
    private BattleUIController buttonScript;

    // combat start
    // combat start
    private GameObject[] playerList, player2List, enemyList;
    public GameObject[] prefabsLibrary;
    public GameObject[] monsterPrefabs;
    public Transform[] spawnPoints;
    public GameObject[] uiTextsGameObjects;

    private bool PvP, PvM;
    private bool wait;

    // player input 
    // player input 
    private GameObject selectedCharacter;
    private GameObject[] selectedTargets;
    private int selectedSkill; 
    private int clickedTeam; // used in select(), saves the team of the clicked entity

    // turn by turn data
    private bool isBattleOver;

    private int turnCount; // increases by 1 whenever a turn is over (the 2 teams have played)
    private bool finishedTurn;
    private GameObject usedCharacter; // saves the character that was just used
    private BattlePhase currentPhase;
    private int currentTeam; // the team currently attacking (for now, player1 = 1, player2 = 2 & enemies = -1)

    private GameObject[] currentTeamList;
    private Character characterScript;
    private Enemy enemyScript;

    // --------------- set() & get() ---------------
    // sets
    public void setSelectedCharacter(GameObject character){ selectedCharacter = character; }
    public void setSelectedSkill(int skill){ selectedSkill = skill; }
    public void setSelectedTargets(GameObject[] targets){ selectedTargets = targets; }

    public void setCurrentPhase(BattlePhase phase){ currentPhase = phase; }
    public void setFinishedTurn(bool x){ finishedTurn = x; }


    // gets
    public GameObject getSelectedCharacter(){ return selectedCharacter; }
    public int getSelectedSkill(){ return selectedSkill; }
    public GameObject[] getSelectedTargets(){ return selectedTargets; }

    public BattlePhase getCurrentPhase(){ return currentPhase; }
    public int getCurrentTeam(){ return currentTeam; }
    public GameObject getUsedCharacter(){ return usedCharacter; }
    public bool getIsBattleOver(){ return isBattleOver; }


    // --------------- Initialisation ---------------
    void Awake(){
        buttonScript = Object.FindAnyObjectByType<BattleUIController>(FindObjectsInactive.Exclude);

        turnCount = 0;
        usedCharacter = null;
        currentTeam = 1; // at the start of the fight, player1 starts attaking 
        wait = false;
        isBattleOver = false;

        PvP = false;
        PvM = false;
    }

    void Reset(){
        Awake();
    }
    IEnumerator Start() {
        yield return new WaitForEndOfFrame();

        if (SelectionData.Instance != null && SelectionData.Instance.team1.Length >= 2) {
            InitializeBattle();
        } else {
            Debug.LogError("Les données de combat ne sont pas prêtes ou incomplètes !");
        }
    }
    void InitializeBattle() {

        int[] t1 = SelectionData.Instance.team1;
        GameObject p1 = SpawnUnit(t1[0], spawnPoints[0], Color.blue, 0, 1);
        GameObject p2 = SpawnUnit(t1[1], spawnPoints[1], Color.blue, 1, 1);

        playerList = new GameObject[] { p1, p2 };

        if (SelectionData.Instance.isPvP) {

            int[] t2 = SelectionData.Instance.team2;
            GameObject p3 = SpawnUnit(t2[0], spawnPoints[2], Color.red, 2, 2);
            GameObject p4 = SpawnUnit(t2[1], spawnPoints[3], Color.red, 3, 2);


            player2List = new GameObject[] { p3, p4 };
        } else {
            /*
            GameObject m1 = Instantiate(monsterPrefabs[0], spawnPoints[2].position, Quaternion.identity);
            GameObject m2 = Instantiate(monsterPrefabs[1], spawnPoints[3].position, Quaternion.identity);
            SpawnUnit(monsterPrefabs[0], spawnPoints[2], Color.red, 2, 2);
            SpawnUnit(monsterPrefabs[1], spawnPoints[3], Color.red, 3, 2);
            m1.GetComponent<SpriteRenderer>().color = Color.red;
            m2.GetComponent<SpriteRenderer>().color = Color.red;

            enemyList = new GameObject[] { m1, m2 }; */
        }
    }
    GameObject SpawnUnit(int index, Transform point, Color sideColor, int uiIndex, int tID) {
        GameObject unit = Instantiate(prefabsLibrary[index], point.position, Quaternion.identity);
        unit.GetComponent<SpriteRenderer>().color = sideColor;
        Character scriptJ = unit.GetComponent<Character>();
        if (scriptJ != null) {
            scriptJ.setTeamID(tID);
            scriptJ.textInfoPV = uiTextsGameObjects[uiIndex];
        } else {
            Enemy scriptE = unit.GetComponent<Enemy>();
            if (scriptE != null) {
                scriptE.TeamId = tID;
                scriptE.textInfoPV = uiTextsGameObjects[uiIndex];
            }
        }
        return unit;
    }



    // --------------- start fight ---------------
    public void startPvPFight(){
        ///<param> playerList : list of player1's characters, player2List : list of player2's characters </param>
        ///<summary> starts the PvP fight </summary>
        
        enemyList = null;

        turnCount = 0;
        usedCharacter = null;
        wait = false;
        currentTeam = 1;
        PvP = true; // starts the battle
        Debug.Log("Le combat PvP commence.");
        buttonScript.setAnnouncementText("Le combat PvP commence.");
    }

    public void startPvMFight(){
        ///<param> playerList : list of the player's characters, enemyList : list of the enemies the characters are fighting </param>
        ///<summary> starts the PvM fight </summary>
        
        player2List = null;

        turnCount = 0;
        usedCharacter = null;
        wait = false;
        currentTeam = 1;

        PvM = true; // starts the battle
        Debug.Log("Le combat PvM commence.");
        buttonScript.setAnnouncementText("Le combat PvM commence.");
    }

    // --------------- Update ---------------
    void FixedUpdate(){
        if (playerList == null || playerList.Length == 0) return;
        if (PvP & !wait){ // only enters the loop when a turn is over
            if (teamDead(playerList) | teamDead(player2List)){ // checks if the battle is over
                if (teamDead(player2List)){
                    Debug.Log("Fin du combat, le joueur 1 a gagné.");
                    buttonScript.setInfoText("Fin du combat, le joueur 1 a gagné.");
                    buttonScript.DisplayEndGame("Victoire du Joueur 1 !");
                } else {
                    Debug.Log("Fin du combat, le joueur 2 a gagné.");
                    buttonScript.setInfoText("Fin du combat, le joueur 2 a gagné.");
                    buttonScript.DisplayEndGame("Victoire du Joueur 2 !");
                }
                wait = true;
                isBattleOver = true;
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

        if (PvM & !wait){ // PvM battle
            if (teamDead(playerList) | teamDead(enemyList)){ // checks if the battle is over
                if (teamDead(enemyList)){
                    Debug.Log("Fin du combat, le joueur a gagné.");
                    buttonScript.setInfoText("Fin du combat, le joueur a gagné.");
                    buttonScript.DisplayEndGame("Victoire du Joueur !");
                } else {
                    Debug.Log("Fin du combat, l'ennemi a gagné.");
                    buttonScript.setInfoText("Fin du combat, l'ennemi a gagné.");
                    buttonScript.DisplayEndGame("Défaite !");
                }
                wait = true;
                isBattleOver = true;
            }
            if (currentTeam == 1){ // player's turn
                if (!teamDead(playerList) & !teamDead(enemyList)){ // while both teams are alive
                    StartCoroutine(playerTurn());
                } else if (currentTeam == -1){ // enemy's turn
                    enemyTurn();
                }
            }
        }
    }

    // --------------- Methods ---------------
    private IEnumerator playerTurn(){
        ///<summary> goes through the character, skill and target selection during the player's turn </summary>
        Debug.Log("Tour de l'équipe " + currentTeam + ".");
        buttonScript.setAnnouncementText("Tour de l'équipe " + currentTeam + ".");
        buttonScript.setAttackerName("");
        wait = true;

        finishedTurn = false;
        currentTeamList = (currentTeam == 1) ? playerList : (currentTeam == 2) ? player2List : null;

        foreach (GameObject c in currentTeamList){ // once per character that is alive
            if (!finishedTurn & !isDead(c)){ // or until the "Fin du tour" button is clicked
                // waits for player to select a character, a skill and a target

                selectedCharacter = null;
                selectedSkill = -1;
                selectedTargets = null;

                // waits for player to select a character
                currentPhase = BattlePhase.SELECT_CHARACTER;
                buttonScript.ButtonAccess();
                Debug.Log("Veuillez selectionner un personnage.");
                buttonScript.setInstructionText("Veuillez selectionner un personnage.");
                yield return new WaitUntil(() => (selectedCharacter != null) | finishedTurn);
                if (finishedTurn) {
                    usedCharacter = null;
                    break; // if the "Fin du tour" button was clicked, end the turn
                }
        
                // waits for player to select a skill
                yield return new WaitUntil(() => (selectedSkill != -1 | finishedTurn));
                if (finishedTurn) {
                    selectedCharacter.GetComponent<Character>().Deselect();
                    usedCharacter = null;
                    break; // if the "Fin du tour" button was clicked, end the turn
                }

                // waits for player to select a target (if the automatic target selection hasn't happened)
                if (selectedTargets == null) {
                    yield return new WaitUntil(() => (selectedTargets != null | finishedTurn));
                    if (finishedTurn) {
                        selectedCharacter.GetComponent<Character>().Deselect();
                        usedCharacter = null;
                        break; // if the "Fin du tour" button was clicked, end the turn
                    }   
                }

                // applies skill to target(s)
                characterScript = selectedCharacter.GetComponent<Character>();
                switch (selectedSkill){
                    case 0 : characterScript.baseAttack(selectedTargets[0]);
                             Debug.Log("Attaque basique, lancée par " + selectedCharacter.name + " sur " + selectedTargets[0].name);
                             buttonScript.setInfoText("Attaque basique, lancée par " + selectedCharacter.name + " sur " + selectedTargets[0].name);
                             break;

                    case 1 : characterScript.skillLvl1(selectedTargets[0]);
                             Debug.Log("Compétence niveau 1, lancée par " + selectedCharacter.name + " sur " + selectedTargets[0].name);
                             buttonScript.setInfoText("Compétence niveau 1, lancée par " + selectedCharacter.name + " sur " + selectedTargets[0].name);
                             break;

                    case 2 : characterScript.skillLvl2(selectedTargets);
                             Debug.Log("Compétence niveau 2, lancée par " + selectedCharacter.name);
                             buttonScript.setInfoText("Compétence niveau 2 , lancée par " + selectedCharacter.name);
                             break;

                    case 3 : characterScript.skillLvl3(selectedTargets);
                             Debug.Log("Compétence niveau 3, lancée par " + selectedCharacter.name);
                             buttonScript.setInfoText("Compétence niveau 3, lancée par " + selectedCharacter.name);
                             break;
                }
                // if a team is dead, the battle is over
                if (teamDead(playerList) | teamDead(player2List) | teamDead(enemyList)){ // checks if the battle is over    
                    finishedTurn = true;
                    buttonScript.setInstructionText("Fin du combat.");
                }
                selectedCharacter.GetComponent<Character>().Deselect();
                for(int i = 0; i < selectedTargets?.Length; i++){
                    if (selectedTargets[i] != null){
                        if (selectedTargets[i].GetComponent<Character>() != null){
                            selectedTargets[i].GetComponent<Character>().Deselect();
                        }
                        else if (selectedTargets[i].GetComponent<Enemy>() != null){
                            selectedTargets[i].GetComponent<Enemy>().Deselect();
                        }
                    }
                }
                    Debug.Log("Nombre de personnages encore en vie dans l'équipe du joueur" + currentTeam+ ": " + numberAliveMembers(currentTeamList));
                    if (usedCharacter != null || numberAliveMembers(currentTeamList) == 1){ // if the two characters have been used, the player's turn is over
                    usedCharacter = null;
                    finishedTurn = true;
                } else {
                    usedCharacter = selectedCharacter; // otherwise, the character that has just been used is saved
                }
            }
        }

        // switch to the other team
        if (PvM){
            currentTeam = -1; // enemies' turn
        } else if (currentTeam == 1){

            currentTeam = 2; // player2's turn
        } else {
            currentTeam = 1; // player1's turn
        }

        if (currentTeam == 1){ // after the 2 teams have played
            turnCount += 1; // increments turnCount
        }
    }

    private void enemyTurn(){
        ///<summary> makes the enemies attack during the enemies' turn </summary>
        
        Debug.Log("Tour de l'ennemi.");
        
        // implémenter l'ia ici
        // ...
    }
    int numberAliveMembers(GameObject[] team) {
        int count = 0;
        foreach (GameObject member in team) {
            // C'est ici que tu gères le fait que l'objet est détruit (null)
            if (member != null && !isDead(member)) {
                count++;
            }
        }
        return count;
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
        buttonScript.setInfoText("L'équipe " + ((currentTeam == 1) ? "1" : (currentTeam == 2) ? "2" : "ennemie") + " est morte.");
        return true; // otherwise, the team is dead
    }

    private bool isDead(GameObject target){
        ///<param> target : an entity (either a character or an enemy) </param>
        ///<summary> returns true if the charater is dead, otherwise return false </summary>
        if (target == null) {
        return true; 
     }else if (target.GetComponent<Character>() != null){ // if target is a character
            if (target.GetComponent<Character>().getCurrentHP() <= 0){ // if hp of the target is 0 (or less)
                Debug.Log(target.name + " is dead");
                buttonScript.setInfoText(target.name + " est mort.");
                return true; // target is dead
            }

        } else if (target.GetComponent<Enemy>() != null){ // if target is an enemy
            if (target.GetComponent<Enemy>().CurrentHP <= 0){  // if hp of the target is 0 (or less)
                Debug.Log(target.name + " is dead");
                buttonScript.setInfoText(target.name + " est mort.");
                return true; // target is dead
            }
        }
        return false; // otherwise, target is alive
    }

    public void automaticTargetSelection(){
        ///<summary> selects all opponents/allies as targets depending on the selected character and skill </summary>
        
        if ((selectedCharacter.GetComponent<Mage>() != null & selectedSkill == 3)| // if character is a mage and using skill lvl 3
            (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 2)){ // or if character is a protector and using skill lvl 2
            // skill affects all opponents (player doesn't need to select target)
            if (player2List == null){ // if it's a PvM
                selectedTargets = enemyList;
            } else if (currentTeam == 1){
                selectedTargets = player2List;
            } else {
                selectedTargets = playerList;
            }
            Debug.Log("Tous les ennemis ont étés ciblés. (La compétence affecte tous les ennemis)");


        } else if ((selectedCharacter.GetComponent<Healer>() != null & selectedSkill == 3)| // if character is a healer and using skill lvl 3
            (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 3)){ // or if character is a protector and using skill lvl 3
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


        } else if ((selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 1)){ // if character is a protector and using skill lvl 1
            // skill affects their ally
            selectedTargets = new GameObject[] {(currentTeamList[0].GetComponent<Protector>() != null) ? currentTeamList[1] : currentTeamList[0]};
            Debug.Log("L'allié a été ciblé. (La compétence affecte l'allié)");
        }
    }

    public void select(GameObject clickedObject){
        ///<summary> handles the character and target selection </summary>

        clickedTeam = (clickedObject.GetComponent<Character>() != null) ? clickedObject.GetComponent<Character>().getTeamID() : -1; // gets the team id of the clicked character

        if (currentPhase == BattlePhase.SELECT_CHARACTER){
            if (clickedTeam == currentTeam){
                if (clickedObject != usedCharacter){ // if the character is allowed to be selected, select the character
                    selectedCharacter = clickedObject;
                    buttonScript.setAttackerName(selectedCharacter.name);
                    Debug.Log("Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");
                    buttonScript.setInstructionText("Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");

                    currentPhase = BattlePhase.SELECT_SKILL;
                    buttonScript.ButtonAccess();


                } else { // the character was already used, nothing happens
                    Debug.LogWarning("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                    buttonScript.setWarningText("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                    Character character_selected = clickedObject.GetComponent<Character>();
                    if (character_selected != null){
                        character_selected.Deselect();
                    }

                }

            } else { // the character/enemy is not on the right team, nothing happens
                Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                if(clickedObject.GetComponent<Character>() != null){
                    clickedObject.GetComponent<Character>().Deselect();
                }
                else if (clickedObject.GetComponent<Enemy>() != null){
                    clickedObject.GetComponent<Enemy>().Deselect();
                }
            }
    
        } else if(currentPhase == BattlePhase.SELECT_SKILL){

            if(clickedTeam == currentTeam & clickedObject != selectedCharacter & clickedObject == usedCharacter){ 
                Debug.LogWarning("Vous avez déjà utilisé ce personnage. Veuillez choisir un autre ou passer le tour.");
                buttonScript.setWarningText("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                clickedObject.GetComponent<Character>().Deselect();


            } else if (clickedTeam == currentTeam & clickedObject != selectedCharacter){ // if the character is an ally but wasn't selected, switch to this character
                selectedCharacter.GetComponent<Character>().Deselect();
                selectedCharacter = clickedObject;
                buttonScript.setAttackerName(selectedCharacter.name);
                Debug.Log("Changement.Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");
                buttonScript.setInstructionText("Changement. Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");


            } else if (clickedTeam != currentTeam){ // if character is an opponent
                Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le sélectionner.");
                buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le sélectionner.");
                if(clickedObject.GetComponent<Character>() != null){
                    clickedObject.GetComponent<Character>().Deselect();
                }
                else if (clickedObject.GetComponent<Enemy>() != null){
                    clickedObject.GetComponent<Enemy>().Deselect();
                }
            }
            else
            {
                Debug.LogWarning("Vous utilisé déjà ce personnage. Veuillez choisir une compétence.");
                buttonScript.setWarningText("Vous utilisé déjà ce personnage. Veuillez choisir une compétence.");
            }
        
        }else if (currentPhase == BattlePhase.SELECT_TARGET){
            if (clickedTeam == currentTeam){ // if character is an ally
                if ((selectedCharacter.GetComponent<Healer>() & selectedSkill != 0)   | (selectedCharacter.GetComponent<Protector>() & selectedSkill != 2 & selectedSkill != 0)){ // if an ally can be targeted, target the character
                    selectedTargets = new GameObject[] {clickedObject};
                    Debug.Log("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");
                    buttonScript.setInstructionText("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");

                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();

                }else if (clickedObject != selectedCharacter & clickedObject == usedCharacter)
                {
                    Debug.LogWarning("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                    buttonScript.setWarningText("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                    clickedObject.GetComponent<Character>().Deselect();
                }
                else if (clickedObject != selectedCharacter){ // if the character wasn't used before, switch to character
                    selectedCharacter.GetComponent<Character>().Deselect();
                    selectedCharacter = clickedObject;
                    buttonScript.setAttackerName(selectedCharacter.name);
                    Debug.Log("Changement. Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");
                    buttonScript.setInstructionText("Changement. Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");

                    currentPhase = BattlePhase.SELECT_SKILL;
                    buttonScript.ButtonAccess();


                } else if(clickedObject == selectedCharacter) { 
                    Debug.LogWarning("Cette attaque est impossible sur soit même. Veuillez choisir un autre personnage.");
                    buttonScript.setWarningText("Cette attaque est impossible sur soit même. Veuillez choisir un autre personnage.");
                }else{// the character cannot be selected
                    Debug.LogWarning("Ce personnage fait partie de votre équipe, vous ne pouvez pas l'attaquer. Veuillez choisir un autre personnage.");
                    buttonScript.setWarningText("Ce personnage fait partie de votre équipe, vous ne pouvez pas l'attaquer. Veuillez choisir un autre personnage.");
                    clickedObject.GetComponent<Character>().Deselect();
                }   

            } else { // if character/enemy is an opponent
                if ((selectedCharacter.GetComponent<Healer>() & selectedSkill != 0)   | (selectedCharacter.GetComponent<Protector>() & selectedSkill != 2 & selectedSkill != 0)){ // if the target has to be an ally, nothing happens
                    Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le clibler (compétence aidant un allié). Veuillez choisir un autre personnage.");
                    buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le clibler (compétence aidant un allié). Veuillez choisir un autre personnage.");

                    if(clickedObject.GetComponent<Character>() != null){
                        clickedObject.GetComponent<Character>().Deselect();
                    }
                    else if (clickedObject.GetComponent<Enemy>() != null){
                        clickedObject.GetComponent<Enemy>().Deselect();
                    }

                } else { // if the character/enemy can be targetted, target the character/enemy
                    selectedTargets = new GameObject[] {clickedObject};
                    Debug.Log("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");
                    buttonScript.setInstructionText("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");

                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();
                }
            }
        }
    }
}