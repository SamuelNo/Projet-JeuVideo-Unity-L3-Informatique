using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


public class Combat : MonoBehaviour
{
    // --------------- Attributes ---------------
    // script access
    // script access
    private BattleUIController buttonScript;
    private Principal principalScript;
    private Save saveScript;

    // combat start
    // combat start
    public SpriteRenderer backgroundRenderer; 
    public Sprite[] stageBackgrounds; 
    private GameObject[] playerList, player2List, enemyList;
    public GameObject[] team1Prefabs;
    public GameObject[] team2Prefabs; 
    public GameObject[] monsterPrefabs;
    
    [System.Serializable] 
    public struct MapSpawns {
        public Transform[] points; 
    }
    public MapSpawns[] allMapsSpawns;
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
    private bool endGameClicked; // becomes true when the player clicks on "Fin de la partie" at the end of the fight, used in BattleUIController to know when to return to the menu
    private GameObject usedCharacter; // saves the character that was just used
    private BattlePhase currentPhase;
    private int currentTeam; // the team currently attacking (for now, player1 = 1, player2 = 2 & enemies = -1)
    private bool isActionLocked = false; // used to prevent the player from clicking on multiple targets when selecting multiple targets for a skill
    private GameObject[] currentTeamList;
    private Character characterScript;
    private Enemy enemyScript;
    private List<(Status, int)> statusList;
    private bool effect;

    // --------------- set() & get() ---------------
    // sets
    public void setSelectedCharacter(GameObject character){ selectedCharacter = character; }
    public void setSelectedSkill(int skill){ selectedSkill = skill; }
    public void setSelectedTargets(GameObject[] targets){ selectedTargets = targets; }

    public void setCurrentPhase(BattlePhase phase){ currentPhase = phase; }
    public void setFinishedTurn(bool x){ finishedTurn = x; }
    public void setFinishedGame(bool x){ endGameClicked = x; }
    public void setIsBattleOver(bool x){ isBattleOver = x; }

    // gets
    public GameObject getSelectedCharacter(){ return selectedCharacter; }
    public int getSelectedSkill(){ return selectedSkill; }
    public GameObject[] getSelectedTargets(){ return selectedTargets; }

    public BattlePhase getCurrentPhase(){ return currentPhase; }
    public int getCurrentTeam(){ return currentTeam; }
    public GameObject getUsedCharacter(){ return usedCharacter; }
    public bool getIsBattleOver(){ return isBattleOver; }
    public bool getWait(){ return wait; }


    // --------------- Initialisation ---------------
    void Awake(){
        buttonScript = Object.FindAnyObjectByType<BattleUIController>(FindObjectsInactive.Exclude);
        saveScript = this.gameObject.AddComponent<Save>();

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
            if (!SelectionData.Instance.isPvP) {
                startPvMFight();
            }
        } else {
            Debug.LogError("Les données de combat ne sont pas prêtes ou incomplètes !");
        }
    }
    void InitializeBattle() {
        int selectedStage = SelectionData.Instance.selectedStage;
        int[] t1 = SelectionData.Instance.team1;
        GameObject p1 = SpawnUnit(t1[0], allMapsSpawns[selectedStage].points[0], 0, 1);
        GameObject p2 = SpawnUnit(t1[1], allMapsSpawns[selectedStage].points[1], 1, 1);

        playerList = new GameObject[] { p1, p2 };

        if (SelectionData.Instance.isPvP) {

            int[] t2 = SelectionData.Instance.team2;
            GameObject p3 = SpawnUnit(t2[0], allMapsSpawns[selectedStage].points[2], 2, 2);
            GameObject p4 = SpawnUnit(t2[1], allMapsSpawns[selectedStage].points[3], 3, 2);


            player2List = new GameObject[] { p3, p4 };
        } else if(!SelectionData.Instance.isPvP) {
            if(selectedStage == 1){
                GameObject m1 = SpawnUnit(0, allMapsSpawns[selectedStage].points[2], 2, -1);
                GameObject m2 = SpawnUnit(1, allMapsSpawns[selectedStage].points[3], 3, -1);
                enemyList = new GameObject[] { m1, m2 };
            }
            if(selectedStage == 2){
                GameObject m1 = SpawnUnit(3, allMapsSpawns[selectedStage].points[2], 2, -1);
                GameObject m2 = SpawnUnit(2, allMapsSpawns[selectedStage].points[3], 3, -1);
                GameObject m3 = SpawnUnit(4, allMapsSpawns[selectedStage].points[4], 4, -1);
                GameObject[] currentEnemies = { m1, m2, m3 };
                float newOffset = 1.5f;

                foreach(GameObject m in currentEnemies) {
                    Enemy e = m.GetComponent<Enemy>();
                    if(e != null) {
                        e.statBar.offset = new Vector3(0, newOffset, 0);
                        e.offset = new Vector3(0, newOffset+0.5f, 0);
                    }
                }
                enemyList = currentEnemies;
            }
            if(selectedStage == 3){
                GameObject m1 = SpawnUnit(5, allMapsSpawns[selectedStage].points[2], 4, -1);
                enemyList = new GameObject[] { m1};
            }
        }
    }
    
    GameObject SpawnUnit(int index, Transform point, int uiIndex, int tID) {
        GameObject unit = null;
        if(tID == 1) {
            unit = Instantiate(team1Prefabs[index], point.position, Quaternion.identity);
        } else if(tID == 2) {
            unit = Instantiate(team2Prefabs[index], point.position, Quaternion.identity);
        } else if(tID == -1) {
            unit = Instantiate(monsterPrefabs[index], point.position, Quaternion.identity);
        }
        if(unit == null) {
            Debug.LogError("Erreur lors de l'instanciation de l'unité. Vérifiez les index et les tableaux de prefabs.");
            return null;
        }
        StatBarHandler handler = buttonScript.CreateStatBar(unit.transform);
        Character scriptJ = unit.GetComponent<Character>();
        if (scriptJ != null) {
            scriptJ.setTeamID(tID);
            scriptJ.textInfoPV = uiTextsGameObjects[uiIndex];
            scriptJ.statBar = handler;
            scriptJ.UpdateBars();
        } else {
            Enemy scriptE = unit.GetComponent<Enemy>();
            if (scriptE != null) {
                scriptE.TeamId = tID;
                scriptE.textInfoPV = uiTextsGameObjects[uiIndex];
                scriptE.statBar = handler;
                scriptE.UpdateBars();
            }
        }
        return unit;
    }



    // --------------- start fight ---------------
    public void startPvPFight(){
        ///<param> playerList : list of player1's characters, player2List : list of player2's characters </param>
        ///<summary> starts the PvP fight </summary>
        
        enemyList = null;
        endGameClicked = false;
        turnCount = 0;
        usedCharacter = null;
        wait = false;
        currentTeam = 1;
        PvP = true; // starts the battle
        int chosenStage = SelectionData.Instance.selectedStage;
        if(chosenStage == 0)
        {
            backgroundRenderer.sprite = stageBackgrounds[chosenStage];
        }
        else
        {
            Debug.LogWarning("Selected stage index is out of range. Using default background.");
        }
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
        endGameClicked = false;
        PvM = true; // starts the battle
        int chosenStage = SelectionData.Instance.selectedStage;
        if(chosenStage >= 0 && chosenStage < stageBackgrounds.Length) {
            backgroundRenderer.sprite = stageBackgrounds[chosenStage];
        } else {
            Debug.LogWarning("Selected stage index is out of range. Using default background.");
        }
        Debug.Log("Le combat PvM commence.");
        buttonScript.setAnnouncementText("Le combat PvM commence.");
    }

    // --------------- Update ---------------
    void FixedUpdate(){
        if (playerList == null || playerList.Length == 0) return;
        if (endGameClicked) {
                    buttonScript.setInfoText("Fin du combat");
                    buttonScript.DisplayEndGame("Combat terminé");
                    buttonScript.setAttackerName("");
                    wait = true;
                    isBattleOver = true;
                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();
                    buttonScript.ClearAllBars();
                    return;
        }
        if (PvP & !wait){ // only enters the loop when a turn is over
            if (teamDead(playerList) | teamDead(player2List)){ // checks if the battle is over
                if (teamDead(player2List)){
                    Debug.Log("Fin du combat, le joueur 1 a gagné.");
                    buttonScript.setInfoText("Fin du combat, le joueur 1 a gagné.");
                    buttonScript.DisplayEndGame("Victoire du Joueur 1 !");
                    buttonScript.setAttackerName("");
                } else {
                    Debug.Log("Fin du combat, le joueur 2 a gagné.");
                    buttonScript.setInfoText("Fin du combat, le joueur 2 a gagné.");
                    buttonScript.DisplayEndGame("Victoire du Joueur 2 !");
                    buttonScript.setAttackerName("");
                }
                wait = true;
                isBattleOver = true;
                currentPhase = BattlePhase.WAITING;
                buttonScript.ButtonAccess();
                buttonScript.ClearAllBars();
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
                    // saves victory in save file
                    if ((saveScript.currentStage == saveScript.unlockedStage) & (saveScript.unlockedStage != 2)){
                        saveScript.currentStage++;
                        saveScript.unlockedStage++;
                    }
                    saveScript.save();
                    
                    Debug.Log("Fin du combat, le joueur a gagné.");
                    buttonScript.setInfoText("Fin du combat, le joueur a gagné.");
                    buttonScript.DisplayEndGame("Victoire du Joueur !");
                    buttonScript.setAttackerName("");
                } else {
                    Debug.Log("Fin du combat, l'ennemi a gagné.");
                    buttonScript.setInfoText("Fin du combat, l'ennemi a gagné.");
                    buttonScript.DisplayEndGame("Défaite !");
                    buttonScript.setAttackerName("");
                }
                wait = true;
                isBattleOver = true;
                currentPhase = BattlePhase.WAITING;
                buttonScript.ButtonAccess();
                buttonScript.ClearAllBars();
            }
            if (currentTeam == 1)
            { // player's turn
                if (!teamDead(playerList) & !teamDead(enemyList))
                { // while both teams are alive
                    StartCoroutine(playerTurn());
                }
            }else if (currentTeam == -1){ // enemy's turn
                if (!teamDead(playerList) & !teamDead(enemyList))
                {
                    StartCoroutine(enemyTurn());
                }
            }
        }
    }

    // --------------- Methods ---------------
    private IEnumerator playerTurn(){
        ///<summary> goes through the character, skill and target selection during the player's turn </summary>
        currentPhase = BattlePhase.WAITING;
        Debug.Log("Tour de l'équipe " + currentTeam + ".");
        buttonScript.setAnnouncementText("Tour de l'équipe " + currentTeam + ".");
        buttonScript.setAttackerName("");
        wait = true;
        usedCharacter = null;
        finishedTurn = false;
        currentTeamList = (currentTeam == 1) ? playerList : (currentTeam == 2) ? player2List : null;
        turnCount = 0;
        foreach (GameObject c in currentTeamList){ // once per character that is alive
            if (!finishedTurn & !isDead(c)){ // or until the "Fin du tour" button is clicked
                // waits for player to select a character, a skill and a target
                turnCount += 1;
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
                    wait = false;
                    break; // if the "Fin du tour" button was clicked, end the turn
                }
        
                // waits for player to select a skill
                yield return new WaitUntil(() => (selectedSkill != -1 | finishedTurn));
                if (finishedTurn) {
                    selectedCharacter.GetComponent<Character>().Deselect();
                    buttonScript.setAttackerName("");
                    usedCharacter = null;
                    wait = false;
                    break; // if the "Fin du tour" button was clicked, end the turn
                }

                // waits for player to select a target (if the automatic target selection hasn't happened)
                if (selectedTargets == null) {
                    yield return new WaitUntil(() => (selectedTargets != null | finishedTurn));
                    if (finishedTurn) {
                        selectedCharacter.GetComponent<Character>().Deselect();
                        buttonScript.setAttackerName("");
                        usedCharacter = null;
                        wait = false;
                        break; // if the "Fin du tour" button was clicked, end the turn
                    }   
                }
                currentPhase = BattlePhase.WAITING;
                // handles the character's status 
                statusHandler();

                // applies skill to target(s)
                skillHandler();
                buttonScript.setAnnouncementText("Waiting...");
                yield return new WaitForSeconds(1f);
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
                // checks if the turn is over
                Debug.Log("Nombre de personnages encore en vie dans l'équipe du joueur" + currentTeam+ ": " + numberAliveMembers(currentTeamList));
                if (teamDead(enemyList) | teamDead(playerList) | teamDead(player2List)){ // if a team is dead, the battle is over
                    finishedTurn = true;
                    wait = false;
                    buttonScript.setInstructionText("Fin du combat.");
                    
                } else if (turnCount == 2){ // if the two characters have been used, the player's turn is over
                    usedCharacter = null;
                    finishedTurn = true;
                    wait = false;

                } else if (numberAliveMembers(currentTeamList) == 1){
                    if (currentTeamList[0] == usedCharacter){
                        usedCharacter = null;
                        finishedTurn = true;
                        wait = false;
                    } else if (currentTeamList[1] == usedCharacter){
                        usedCharacter = null;
                        finishedTurn = true;
                        wait = false;
                    }
                    
                } else {
                    // if a character has been used and the other is frozen, the player's turn is over
                    statusList = new List<(Status,int)>(currentTeamList[0].GetComponent<Character>().getStatusList());
                    foreach ((Status,int) s in statusList){
                        if (s.Item1 == Status.FROZEN){
                            usedCharacter = null;
                            finishedTurn = true;
                            wait = false;
                        }
                    }
                    statusList = new List<(Status,int)>(currentTeamList[1].GetComponent<Character>().getStatusList());
                    foreach ((Status,int) s in statusList){
                        if (s.Item1 == Status.FROZEN){
                            usedCharacter = null;
                            finishedTurn = true;
                            wait = false;
                        }
                    }


                    // otherwise, the character that has just been used is saved
                    usedCharacter = selectedCharacter;
                }
            }
        }

        // switch to the other team
        if (PvM){
            statusUpdate(playerList);
            currentTeam = -1; // enemies' turn
        } else if (currentTeam == 1){
            statusUpdate(playerList);
            currentTeam = 2; // player2's turn
        } else {
            statusUpdate(player2List);
            currentTeam = 1; // player1's turn
        }
    }

    private IEnumerator enemyTurn()
    {
        ///<summary> makes the enemies attack during the enemies' turn </summary>

        wait = true;
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Tour de l'ennemi.");
        buttonScript.setAnnouncementText("Tour de l'ennemi.");
        buttonScript.setAttackerName("");
        buttonScript.setInstructionText("L'ennemi attaque...");
        buttonScript.getButtonEndTurn().SetState(ButtonState.BLOCKED);
        wait = true;


        // build initial lists of living units
        List<GameObject> charactersAlive = new List<GameObject>();
        List<GameObject> enemiesAlive = new List<GameObject>();

        if (playerList != null)
        {
            for (int j = 0; j < playerList.Length; j++)
            {
                if (!isDead(playerList[j]))
                {
                    charactersAlive.Add(playerList[j]);
                }
            }
        }

        if (enemyList != null)
        {
            for (int k = 0; k < enemyList.Length; k++)
            {
                if (!isDead(enemyList[k]))
                {
                    enemiesAlive.Add(enemyList[k]);
                }
            }
        }

        List<GameObject> targetsWithCircle = new List<GameObject>(); // UI for the targets of the enemy's attack
        yield return new WaitForSeconds(4f); 
        // loop over enemies
        for (int i = 0; i < enemyList.Length; i++)
        {
            GameObject currentEnemy = enemyList[i];
            if (currentEnemy == null || isDead(currentEnemy))
            {
                continue;
            }

            Enemy enemy = currentEnemy.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.Log("Composant ennemi null : " + currentEnemy.name);
                continue;
            }
            enemy.ShowActiveCircle(); // shows the enemy selected

            if (enemy.AI == null)
            {
                enemy.AI = new EnemyAI();
            }

            EnemyAI ai = enemy.AI;
            int action = ai.DecideAction(enemy, charactersAlive, enemiesAlive);
            GameObject target = ai.DecideTarget(enemy, charactersAlive, enemiesAlive);
            if (target == null)
            {
                target = ai.GetLowestHP(enemiesAlive);
            }

            yield return new WaitForSeconds(2f); // short delay before the enemy acts

            // if the enemy is frozen, skip the turn
            effect = false;
            statusList = enemy.getStatusList();
            foreach ((Status,int) s in statusList){
                if (s.Item1 == Status.FROZEN) effect = true;
            }
            if (effect){
                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" est gelé et ne peut pas attaquer.");

            } else {

                // if the target is shielded, skip the attacks
                if (target.GetComponent<Character>() != null){
                    effect = false;
                    statusList = target.GetComponent<Character>().getStatusList();
                    foreach ((Status,int) s in statusList){
                        if (s.Item1 == Status.SHIELDED) effect = true; // (attacks will check if effect==true)
                    }
                }

                // array for AoE attacks
                selectedTargets = charactersAlive.ToArray();

                
                // if a target is protected, attack the protector instead
                if (target.GetComponent<Character>() != null){
                    for (int x=0; x<selectedTargets.Length; x++) {
                        statusList = selectedTargets[x].GetComponent<Character>().getStatusList();
                        foreach ((Status, int) s in statusList){
                            if (s.Item1 == Status.PROTECTED) {
                                // if the protector is dead, the character is no longer protected
                                if (charactersAlive.ElementAt(0) == null)
                                {
                                    charactersAlive.ElementAt(1).GetComponent<Character>().getStatusList().Remove(s);
                                }
                                else if (charactersAlive.ElementAt(1) == null)
                                {
                                    charactersAlive.ElementAt(0).GetComponent<Character>().getStatusList().Remove(s);

                                }
                                else
                                { // otherwise, replaces the target with the protector
                                    target = (charactersAlive.ElementAt(0).GetComponent<Protector>() != null) ? charactersAlive.ElementAt(0) : charactersAlive.ElementAt(1);
                                }

                                // new array with the protector x2 (for AoE attacks) so the protected target doesn't get attacked
                                if (charactersAlive.ElementAt(0).GetComponent<Protector>() != null)
                                {
                                    selectedTargets = new GameObject[] { charactersAlive.ElementAt(0), charactersAlive.ElementAt(0) };
                                }
                                else
                                {
                                    selectedTargets = new GameObject[] { charactersAlive.ElementAt(1), charactersAlive.ElementAt(1) };
                                }
                            }
                        }
                    }
                }

                // Security check in case the target is dead or has been destroyed since the enemy decided to attack it
                if (target == null || target.GetComponent<Character>() == null || isDead(target))
                {
                    Debug.LogWarning("Target invalide, nouvelle sélection...");

                    // Re-choose a target among the living characters
                    target = ai.DecideTarget(enemy, charactersAlive, enemiesAlive);

                    // ultimate fallback
                    if (target == null && charactersAlive.Count > 0)
                    {
                        target = charactersAlive[Random.Range(0, charactersAlive.Count)];
                    }
                }
                switch (action)
                {
                    // Targeted attack
                    case 1:
                        if (target != null)
                        {
                            if (!effect){ // if the character isn't shielded

                                // shows the target circle for the attacked target
                                target.GetComponent<Character>().ShowTargetCircle(Color.red);
                                if (!targetsWithCircle.Contains(target))
                                    targetsWithCircle.Add(target);

                                enemy.TargetedAttack(target);

                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a attaqué "+target.name+".");

                            } else {
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a sauté son tour.");
                            }
                            // if the target is dead after the attack, remove it from the list
                            if (isDead(target))
                            {
                                charactersAlive.Remove(target);
                            }
                        }
                        break;

                    // Aoe Attack
                    case 2:
                        if (charactersAlive.Count > 0)
                        {
                            if (!effect){ // if the character isn't shielded

                                // shows the target circle
                                foreach (var t in selectedTargets)
                                {
                                    t.GetComponent<Character>().ShowTargetCircle(Color.red);
                                    if (!targetsWithCircle.Contains(t))
                                        targetsWithCircle.Add(t);
                                }

                                enemy.AoeAttack(selectedTargets);
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a attaqué les 2 personnages.");
                                
                            } else {
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a sauté son tour.");
                            }

                            // clean up dead targets
                            for (int idx = charactersAlive.Count - 1; idx >= 0; idx--)
                            {
                                if (isDead(charactersAlive[idx])) charactersAlive.RemoveAt(idx);
                            }
                        }
                        break;

                    // Special Attack
                    case 3:
                        Boss boss = enemy as Boss;
                        if (boss != null && charactersAlive.Count > 0)
                        {
                            if (!effect){ // if the character isn't shielded
                                // shows the target circle
                                foreach (var t in selectedTargets)
                                {
                                    t.GetComponent<Character>().ShowTargetCircle(Color.red);
                                    if (!targetsWithCircle.Contains(t))
                                        targetsWithCircle.Add(t);
                                }

                                boss.SpecialAttack(selectedTargets);
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a utilisé son attaque spéciale.");
                               
                            } else {
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a sauté son tour.");
                            }

                            // clean up dead targets
                            for (int idx = charactersAlive.Count - 1; idx >= 0; idx--)
                            {
                                if (isDead(charactersAlive[idx])) charactersAlive.RemoveAt(idx);
                            }
                        }
                        break;

                    case 4:
                        // heal the ally with the lowest HP (including themselves)
                        if (target != null)
                        {
                            // shows the target circle 
                            target.GetComponent<Enemy>().ShowTargetCircle();
                            if (!targetsWithCircle.Contains(target))
                                targetsWithCircle.Add(target);

                            enemy.Heal(target);

                            buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a soigné "+target.name+".");
                        }
                        break;

                    case 5:
                        // boost the attack of the ally with the lowest HP (including themselves)
                        if (target != null)
                        {
                            // shows the target circle 
                            target.GetComponent<Enemy>().ShowTargetCircle();
                            if (!targetsWithCircle.Contains(target))
                                targetsWithCircle.Add(target);

                            enemy.BoostAttack(target);

                            buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a renforcé les attaques de "+target.name+".");
                        }
                        break;

                    case 6:
                        // protect the ally with the lowest HP (including themselves)
                        if (target != null)
                        {
                            // shows the target circle
                            target.GetComponent<Enemy>().ShowTargetCircle();
                            if (!targetsWithCircle.Contains(target))
                                targetsWithCircle.Add(target);

                            enemy.Protection(target);

                            buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a protégé "+target.name+".");
                        }
                        break;
                }
            }

            // after the attack, the enemy consumes their temporary effects (if they have any)
            enemy.EndTurnConsumeTemporaryEffects();

            yield return new WaitForSeconds(3f);

            foreach (GameObject c in targetsWithCircle)
            {
                if (c == null) continue; // if the target is dead and has been destroyed, skip it
                if (c.GetComponent<Character>() != null)
                {
                    c.GetComponent<Character>().HideTargetCircle();
                }
                else if (c.GetComponent<Enemy>() != null)
                {
                    c.GetComponent<Enemy>().HideTargetCircle();
                }
            }

            enemy.HideActiveCircle();
            targetsWithCircle.Clear();

            // break the enemy loop early if all characters are dead (the battle is over)
            if (charactersAlive.Count == 0)
            {
                break;
            }
        }
        currentTeam = 1;
        wait = false;
        buttonScript.getButtonEndTurn().SetState(ButtonState.SHOWN);
        statusUpdate(enemyList);
        yield return null;
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

    private void statusHandler(){
        ///<summary> handles the characters' status (protected) </summary>

        if (selectedTargets == null || selectedTargets.Length == 0)
        {
            return;
        }
        // deselects the character
        if (selectedTargets.Length == 1){ 
            characterScript = selectedTargets[0].GetComponent<Character>();
            if (characterScript != null)
            {
                characterScript.Deselect();
            }
        }
        int firstTargetTeam = (selectedTargets[0].GetComponent<Character>() != null) 
        ? selectedTargets[0].GetComponent<Character>().getTeamID() 
        : -1;

    
        if (currentTeam == firstTargetTeam) {
            return; 
        }

        for (int i=0; i<selectedTargets.Length; i++){ // for each target

            if (selectedTargets[i] == null) continue;

            characterScript = selectedTargets[i].GetComponent<Character>();
            if (characterScript == null) continue; //if it's an enemy, skip it

            statusList = new List<(Status,int)> (characterScript.getStatusList());

            foreach ((Status,int) s in statusList){
                if (s.Item1 == Status.PROTECTED){ // if target is protected, the opponent's protector takes on the damage instead
                
                    if (currentTeam == 1 & PvP){ 
                        // if the protector is dead, the character is no longer protected
                        if (player2List[0] == null){
                            player2List[1].GetComponent<Character>().getStatusList().Remove(s);
                        } else if (player2List[1] == null){
                            player2List[0].GetComponent<Character>().getStatusList().Remove(s);

                        } else { // otherwise, replaces the target with the protector
                            selectedTargets = new GameObject[] {(player2List[0].GetComponent<Protector>() != null) ? player2List[0] : player2List[1]};
                        }
                        
                    } else {
                        // if the protector is dead, the character is no longer protected
                        if (playerList[0] == null){
                            playerList[1].GetComponent<Character>().getStatusList().Remove(s);
                        } else if (playerList[1] == null){
                            playerList[0].GetComponent<Character>().getStatusList().Remove(s);

                        } else { // otherwise, replaces the target with the protector
                            selectedTargets = new GameObject[] {(playerList[0].GetComponent<Protector>() != null) ? playerList[0] : playerList[1]};
                        }
                    }
                }
            }
        }
    }

    private void skillHandler(){
        ///<summary> applies the selected character's skill to the target(s) </summary>
        
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
    }

    private void statusUpdate(GameObject[] list){
        ///<param> list : list of characters (no enemy for now) </param>
        ///<summary> updates all of the characters' status </summary>
        
        for (int i=0; i<list.Length; i++){ // for all the characters in the list

            if (list[i] != null){ // ... that are alive

                characterScript = list[i].GetComponent<Character>();
                if (characterScript != null) { // if it's a character
                    statusList = new List<(Status,int)> (characterScript.getStatusList());

                    foreach ((Status,int) s in statusList){ // for every status the character has
                        switch (s.Item1){
                            case Status.FROZEN :        // (status effect is taken care of in select())
                                characterScript.getStatusList().Remove(s);
                                break;

                            case Status.PROTECTED :     // (status effect is taken care of in statusHandler())
                                characterScript.getStatusList().Remove(s);
                                if (s.Item2 > 0){
                                    characterScript.getStatusList().Add((Status.PROTECTED, s.Item2-1)); 
                                }
                                break;

                            case Status.SHIELDED :      // (status effect is taken care of in select())
                                characterScript.getStatusList().Remove(s);
                                if (s.Item2 > 0){
                                    characterScript.getStatusList().Add((Status.SHIELDED, s.Item2-1)); // so the status doesn't immediatly disappear
                                }
                                break;

                            case Status.STRENGTHENED :  // (status effect is taken care of here and in class Healer/Fighter)
                                characterScript.getStatusList().Remove(s);
                                if (s.Item2 > 0){
                                    characterScript.getStatusList().Add((Status.STRENGTHENED, s.Item2-1)); // makes the status last one more turn
                                } else {
                                    characterScript.setDamageMultiplier(characterScript.getDamageMultiplier()/1.5f); // do not change '2' unless you change it in Healer and Fighter too
                                }
                                break;
                        }
                    }
                }

                enemyScript = list[i].GetComponent<Enemy>();
                if (enemyScript != null) { // if it's an enemy
                    statusList = new List<(Status,int)> (enemyScript.getStatusList());

                    foreach ((Status,int) s in statusList){ // for every status the character has
                        switch (s.Item1){
                            case Status.FROZEN :
                                enemyScript.getStatusList().Remove(s);
                                break;
                        }
                    }
                    continue;
                }
            }
        }

    }

    public void automaticTargetSelection(){
        ///<summary> selects all opponents/allies as targets depending on the selected character and skill </summary>
        if (currentTeam == -1) return;
        
        if ((selectedCharacter.GetComponent<Mage>() != null & selectedSkill == 3)| // if character is a mage and using skill lvl 3
            (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 2)){ // or if character is a protector and using skill lvl 2
            // skill affects all opponents (player doesn't need to select target)
            if (PvM){ // if it's a PvM
                selectedTargets = enemyList;
                Debug.Log("Tous les ennemis ont étés ciblés. (La compétence affecte tous les ennemis)");

            } else if (currentTeam == 1){
                // checks the that the player's team isn't shielded
                statusList = player2List[0].GetComponent<Character>().getStatusList();
                foreach ((Status,int) s in statusList){
                    if (s.Item1 == Status.SHIELDED){
                        effect = true;
                    }
                }
                if (effect){ // if it is, do not select them
                    Debug.LogWarning("Les adversaires ont un bouclier et ne recevront pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                    buttonScript.setWarningText("Les adversaires ont un bouclier et ne recevront pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                    buttonScript.ButtonAccess();

                } else {
                    selectedTargets = player2List;
                    Debug.Log("Tous les ennemis ont étés ciblés. (La compétence affecte tous les ennemis)");
                }

            } else {
                // checks the that the player's team isn't shielded
                statusList = playerList[0].GetComponent<Character>().getStatusList();
                foreach ((Status,int) s in statusList){
                    if (s.Item1 == Status.SHIELDED){
                        effect = true;
                    }
                }
                if (effect){ // if it is, do not select them
                    Debug.LogWarning("Les adversaires ont un bouclier et ne recevront pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                    buttonScript.setWarningText("Les adversaires ont un bouclier et ne recevront pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                    buttonScript.ButtonAccess();

                } else {
                    selectedTargets = playerList;
                    Debug.Log("Tous les ennemis ont étés ciblés. (La compétence affecte tous les ennemis)");
                }
            }


        } else if ((selectedCharacter.GetComponent<Healer>() != null & selectedSkill == 3)| // if character is a healer and using skill lvl 3
            (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 3)){ // or if character is a protector and using skill lvl 3
            // skill affects all allies (player doesn't need to select target)
            GameObject[] allies = (currentTeam == 1) ? playerList : player2List;

            if (selectedCharacter.GetComponent<Healer>() != null && selectedSkill == 3) {
                
                bool teamIsFullLife = true;

                foreach (GameObject allyGO in allies) {
                    if (allyGO != null) {
                        Character allyScript = allyGO.GetComponent<Character>();
                        if (allyScript != null && allyScript.getCurrentHP() < allyScript.getMaxHP()) {
                            teamIsFullLife = false;
                            break; 
                        }
                    }
                }

                if (teamIsFullLife) {
                    Debug.LogWarning("Toute l'équipe est déjà au maximum de sa vie ! Choisissez une autre compétence.");
                    buttonScript.setWarningText("Équipe déjà au maximum de sa vie !");
                    
                    currentPhase = BattlePhase.SELECT_SKILL;
                    buttonScript.ButtonAccess();
                    
                    selectedTargets = null;
                    return; 
                }
            }

            selectedTargets = allies;
            Debug.Log("Tous les alliés ont étés ciblés. (La compétence affecte tous les alliés)");


        } else if ((selectedCharacter.GetComponent<Fighter>() != null & selectedSkill == 2)){ // if character is a fighter and using skill lvl 2
            // skill affects themselves 
            if(selectedCharacter.GetComponent<Fighter>().getStatusList().Contains((Status.STRENGTHENED,1))){
                Debug.LogWarning("Le personnage est déjà renforcé et ne peut pas utiliser cette compétence.");
                buttonScript.setWarningText("Le personnage est déjà renforcé et ne peut pas utiliser cette compétence.");
                currentPhase = BattlePhase.SELECT_SKILL;
                buttonScript.ButtonAccess();
                return;
            }
            selectedTargets = new GameObject[] {selectedCharacter};
            Debug.Log("Il n'y a pas besoin de sélectionner une cible. (La compétence n'a pas besoin de cible)");
            
        }else if ((selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 1)){ // if character is a protector and using skill lvl 1
            // checks if the protector has an ally
            currentPhase = BattlePhase.WAITING;
            if (currentTeamList[0]==null | currentTeamList[1]==null){
                Debug.LogWarning("Le protecteur n'a pas d'allié et donc personne à protéger. Veuillez choisir une autre compétence ou passer le tour.");
                buttonScript.setWarningText("Le protecteur n'a pas d'allié et donc personne à protéger. Veuillez choisir une autre compétence ou passer le tour.");
                currentPhase = BattlePhase.SELECT_SKILL;
                buttonScript.ButtonAccess();
                return;
            }

            // skill affects their ally
            selectedTargets = new GameObject[] {(currentTeamList[0].GetComponent<Protector>() != null) ? currentTeamList[1] : currentTeamList[0]};
            Debug.Log("L'allié a été ciblé. (La compétence affecte l'allié)");
        }
    }

    public void select(GameObject clickedObject){
        ///<summary> handles the character and target selection </summary>
        if (currentTeam == -1) {
            if(clickedObject.GetComponent<Character>() != null){
                clickedObject.GetComponent<Character>().Deselect();
            }
            else if (clickedObject.GetComponent<Enemy>() != null){
                clickedObject.GetComponent<Enemy>().Deselect();
            }
            return;
        }
        
        clickedTeam = (clickedObject.GetComponent<Character>() != null) ? clickedObject.GetComponent<Character>().getTeamID() : -1; // gets the team id of the clicked character
        effect = false;

        if (currentPhase == BattlePhase.SELECT_CHARACTER){
            // an enemy shouldn't be clickable right now
            if (clickedObject.GetComponent<Character>() == null) {
                Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                if (clickedObject.GetComponent<Enemy>() != null) 
                    clickedObject.GetComponent<Enemy>().Deselect();
                return;
            }

            // if the character is frozen, nothing happens
            statusList = new List<(Status,int)>(clickedObject.GetComponent<Character>().getStatusList());
            foreach ((Status,int) s in statusList){
                if (s.Item1 == Status.FROZEN){
                    effect = true;
                }
            }
            if (effect){
                Debug.LogWarning("Ce personnage est gelé et ne peux pas être utilisé pendant ce tour. Veuillez en choisir un autre ou passer le tour.");
                buttonScript.setWarningText("Ce personnage est gelé et ne peux pas être utilisé pendant ce tour. Veuillez en choisir un autre ou passer le tour.");
                Character character_selected = clickedObject.GetComponent<Character>();
                if (character_selected != null){
                    character_selected.Deselect();
                }

            } else if (clickedTeam == currentTeam){
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

            // an enemy shouldn't be clickable right now
            if (clickedObject.GetComponent<Character>() == null) {
                Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                if (clickedObject.GetComponent<Enemy>() != null) 
                    clickedObject.GetComponent<Enemy>().Deselect();
                return;
            }

            // if the character is frozen, nothing happens
            statusList = new List<(Status,int)>(clickedObject.GetComponent<Character>().getStatusList());
            foreach ((Status,int) s in statusList){
                if (s.Item1 == Status.FROZEN){
                    effect = true;
                }
            }
            if (effect){
                Debug.LogWarning("Ce personnage est gelé et ne peux pas être utilisé pendant ce tour. Veuillez en choisir un autre ou passer le tour.");
                buttonScript.setWarningText("Ce personnage est gelé et ne peux pas être utilisé pendant ce tour. Veuillez en choisir un autre ou passer le tour.");
                Character character_selected = clickedObject.GetComponent<Character>();
                if (character_selected != null){
                    character_selected.Deselect();
                }

            } else if(clickedTeam == currentTeam & clickedObject != selectedCharacter & clickedObject == usedCharacter){ 
                Debug.LogWarning("Vous avez déjà utilisé ce personnage. Veuillez choisir un autre ou passer le tour.");
                buttonScript.setWarningText("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                clickedObject.GetComponent<Character>().Deselect();


            } else if (clickedTeam == currentTeam & clickedObject != selectedCharacter){ // if the character is an ally but wasn't selected, switch to this character
                selectedCharacter.GetComponent<Character>().Deselect();
                selectedCharacter = clickedObject;
                buttonScript.setAttackerName(selectedCharacter.name);
                Debug.Log("Changement.Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");
                buttonScript.setInstructionText("Changement. Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");
                currentPhase = BattlePhase.SELECT_SKILL;
                buttonScript.ButtonAccess();

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

            
        } else if (currentPhase == BattlePhase.SELECT_TARGET){
            if (clickedTeam == currentTeam){ // if character is an ally

                if ((selectedCharacter.GetComponent<Healer>() & selectedSkill != 0) | (selectedCharacter.GetComponent<Protector>() & selectedSkill != 2 & selectedSkill != 0)){ // if an ally can be targeted, target the character
                        if (selectedCharacter.GetComponent<Healer>() != null && selectedSkill == 1) {
                            Character targetCharacter = clickedObject.GetComponent<Character>();
                            if (targetCharacter != null && targetCharacter.getCurrentHP() == targetCharacter.getMaxHP()) {
                                Debug.LogWarning(targetCharacter.name + " a déjà tous ses points de vie. Choisissez une autre cible.");
                                buttonScript.setWarningText(targetCharacter.name + " a déjà tous ses points de vie !");
                                if(clickedObject.GetComponent<Healer>() == null){
                                    targetCharacter.Deselect();
                                }
                                return; 
                            }
                        }    
                    
                        if (selectedCharacter.GetComponent<Healer>() != null && selectedSkill == 2) {
                        
                        Character targetCharacter = clickedObject.GetComponent<Character>();

                        if (targetCharacter != null) {
                            bool isAlreadyStrong = targetCharacter.getStatusList().Any(s => s.Item1 == Status.STRENGTHENED);

                            if (isAlreadyStrong) {
                                Debug.LogWarning(targetCharacter.name + " est déjà renforcé ! Choisissez une autre cible.");
                                buttonScript.setWarningText(targetCharacter.name + " est déjà renforcé !");
                                if(clickedObject.GetComponent<Healer>() == null){
                                    targetCharacter.Deselect();
                                }
                                return; 
                            }
                        }
                    }
                    selectedTargets = new GameObject[] {clickedObject};
                    Debug.Log("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");
                    buttonScript.setInstructionText("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");

                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();
                    

                } else if (clickedObject != selectedCharacter & clickedObject == usedCharacter){
                    Debug.LogWarning("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                    buttonScript.setWarningText("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                    clickedObject.GetComponent<Character>().Deselect();


                } else if (clickedObject != selectedCharacter){ // if the character wasn't used before, switch to character
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


                } else { // the character cannot be selected
                    Debug.LogWarning("Ce personnage fait partie de votre équipe, vous ne pouvez pas l'attaquer. Veuillez choisir un autre personnage.");
                    buttonScript.setWarningText("Ce personnage fait partie de votre équipe, vous ne pouvez pas l'attaquer. Veuillez choisir un autre personnage.");
                    clickedObject.GetComponent<Character>().Deselect();
                }   

            } else { // if character/enemy is an opponent

                // if the target is shielded
                //statusList = new List<(Status,int)>(clickedObject.GetComponent<Character>().getStatusList());
                Character c = clickedObject.GetComponent<Character>();
                if (c != null)
                {
                    statusList = new List<(Status, int)>(c.getStatusList());
                }
                else
                {
                    statusList = new List<(Status, int)>(); // to do (ennemy status)
                }

                foreach ((Status,int) s in statusList){
                    if (s.Item1 == Status.SHIELDED){
                        effect = true;
                    }
                }
                if (effect){
                    if (!(selectedCharacter.GetComponent<Mage>() & selectedSkill == 2)){ // the mage's lvl 2 skill can still work on the protected character (freezing the target)
                        Debug.LogWarning("Ce personnage a un bouclier et ne recevra pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                        buttonScript.setWarningText("Ce personnage a un bouclier et ne recevra pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                        Character character_selected = clickedObject.GetComponent<Character>();
                        if(clickedObject.GetComponent<Character>() != null){
                            clickedObject.GetComponent<Character>().Deselect();
                        }
                        else if (clickedObject.GetComponent<Enemy>() != null){
                            clickedObject.GetComponent<Enemy>().Deselect();
                        }

                    } else { // target the character/enemy
                    selectedTargets = new GameObject[] {clickedObject};
                    Debug.Log("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");
                    buttonScript.setInstructionText("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");

                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();
                    }

                } else if ((selectedCharacter.GetComponent<Healer>() & selectedSkill != 0)   | (selectedCharacter.GetComponent<Protector>() & selectedSkill != 2 & selectedSkill != 0)){ // if the target has to be an ally, nothing happens
                    Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le cibler (compétence aidant un allié). Veuillez choisir un autre personnage.");
                    buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le cibler (compétence aidant un allié). Veuillez choisir un autre personnage.");

                    if(clickedObject.GetComponent<Character>() != null){
                        clickedObject.GetComponent<Character>().Deselect();
                    }
                    else if (clickedObject.GetComponent<Enemy>() != null){
                        clickedObject.GetComponent<Enemy>().Deselect();
                    }

                } else { 
                    // if the character/enemy can be targetted, target the character/enemy
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