using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


/// <summary>
/// Coordinates turn-based combat flow, unit spawning, target selection, and battle resolution.
/// </summary>
public class Combat : MonoBehaviour
{
    // --------------- Attributes ---------------
    // scripts
    
    
    private BattleUIController buttonScript;
    private Save saveScript;

    
    
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

    
    
    private GameObject selectedCharacter;
    private GameObject[] selectedTargets;
    private int selectedSkill; 
    private int clickedTeam; 

    
    private bool isBattleOver;

    private int turnCount; 
    private bool finishedTurn;
    private bool endGameClicked; 
    private GameObject usedCharacter; 
    private BattlePhase currentPhase;
    private int currentTeam; 
    private GameObject[] currentTeamList;
    private Character characterScript;
    private Enemy enemyScript;
    private List<(Status, int)> statusList;
    private bool effect;
    
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AnimationCurve cameraShakeCurve, cameraShakeCurveLvl3;

    
    public GameObject frozenEffectPrefab1, frozenEffectPrefab2, shieldedEffectPrefab, reinforcedEffectPrefab;
    private List<GameObject> frozenEffectList, shieldedEffectList, reinforcedEffectList; 

    
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip defeatClip;

    
    
    public void setSelectedCharacter(GameObject character){ selectedCharacter = character; }
    public void setSelectedSkill(int skill){ selectedSkill = skill; }
    public void setSelectedTargets(GameObject[] targets){ selectedTargets = targets; }

    public void setCurrentPhase(BattlePhase phase){ currentPhase = phase; }
    public void setFinishedTurn(bool x){ finishedTurn = x; }
    public void setFinishedGame(bool x){ endGameClicked = x; }
    public void setIsBattleOver(bool x){ isBattleOver = x; }

    
    public GameObject getSelectedCharacter(){ return selectedCharacter; }
    public int getSelectedSkill(){ return selectedSkill; }
    public GameObject[] getSelectedTargets(){ return selectedTargets; }

    public BattlePhase getCurrentPhase(){ return currentPhase; }
    public int getCurrentTeam(){ return currentTeam; }
    public GameObject getUsedCharacter(){ return usedCharacter; }
    public bool getIsBattleOver(){ return isBattleOver; }
    public bool getWait(){ return wait; }

     // --------------- Initialisation ---------------
    
    /// <summary>
    /// Initializes combat runtime state and required references.
    /// </summary>
    void Awake(){
        buttonScript = Object.FindAnyObjectByType<BattleUIController>(FindObjectsInactive.Exclude);
        saveScript = this.gameObject.AddComponent<Save>();

        turnCount = 0;
        usedCharacter = null;
        currentTeam = 1; 
        wait = false;
        isBattleOver = false;
        frozenEffectList = new List<GameObject>();
        shieldedEffectList = new List<GameObject>();
        reinforcedEffectList = new List<GameObject>();

        PvP = false;
        PvM = false;
    }

    void Reset(){
        Awake();
    }
    /// <summary>
    /// Waits one frame, then initializes and starts the selected battle mode.
    /// </summary>
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
    /// <summary>
    /// Builds teams and spawns units based on the selected stage and mode.
    /// </summary>
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
                GameObject[] currentEnemies = { m1, m2 };
                float newOffset = 2.5f;

                foreach(GameObject m in currentEnemies) {
                    Enemy e = m.GetComponent<Enemy>();
                    if(e != null) {
                        e.statBar.offset = new Vector3(0, newOffset, 0);
                        e.offset = new Vector3(0, e.statBar.offset.y + 0.4f, 0);
                    }
                }
                enemyList = currentEnemies;
            }
            if(selectedStage == 2){
                GameObject m1 = SpawnUnit(3, allMapsSpawns[selectedStage].points[2], 2, -1);
                GameObject m2 = SpawnUnit(2, allMapsSpawns[selectedStage].points[3], 3, -1);
                GameObject m3 = SpawnUnit(4, allMapsSpawns[selectedStage].points[4], 4, -1);
                GameObject[] currentEnemies = { m1, m2, m3 };
                float newOffset = 2.75f;

                foreach(GameObject m in currentEnemies) {
                    Enemy e = m.GetComponent<Enemy>();
                    if(e != null) {
                        if(m == m2)
                        {
                            e.statBar.offset = new Vector3(0, 1f, 0);
                            e.offset = new Vector3(0, e.statBar.offset.y + 0.4f, 0);
                        } else {
                            e.statBar.offset = new Vector3(0, newOffset, 0);
                            e.offset = new Vector3(0, e.statBar.offset.y + 0.4f, 0);
                        }
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
    
    /// <summary>
    /// Instantiates a unit prefab, binds UI elements, and initializes runtime data.
    /// </summary>
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



    
    /// <summary>
    /// Configures and starts a player-versus-player battle.
    /// </summary>
    public void startPvPFight(){
        
        enemyList = null;
        endGameClicked = false;
        turnCount = 0;
        usedCharacter = null;
        wait = false;
        currentTeam = 1;
        PvP = true; 
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

    /// <summary>
    /// Configures and starts a player-versus-monsters battle.
    /// </summary>
    public void startPvMFight(){
        
        player2List = null;

        turnCount = 0;
        usedCharacter = null;
        wait = false;
        currentTeam = 1;
        endGameClicked = false;
        PvM = true; 
        int chosenStage = SelectionData.Instance.selectedStage;
        if(chosenStage >= 0 && chosenStage < stageBackgrounds.Length) {
            backgroundRenderer.sprite = stageBackgrounds[chosenStage];
        } else {
            Debug.LogWarning("Selected stage index is out of range. Using default background.");
        }
        Debug.Log("Le combat PvM commence.");
        buttonScript.setAnnouncementText("Le combat PvM commence.");
    }

    
    void FixedUpdate(){
        if (playerList == null || playerList.Length == 0) return;
        if (endGameClicked) {
                    buttonScript.DisplayEndGame("Combat terminé");
                    buttonScript.setAttackerName("");
                    wait = true;
                    isBattleOver = true;
                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();
                    buttonScript.ClearAllBars();
                    return;
        }
        if (PvP & !wait){ 
            if (teamDead(playerList) | teamDead(player2List)){ 
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
                PlayEndMusic(true);
                wait = true;
                isBattleOver = true;
                currentPhase = BattlePhase.WAITING;
                buttonScript.ButtonAccess();
                buttonScript.ClearAllBars();
            }
            if (currentTeam == 1){ 
                if (!teamDead(playerList) & !teamDead(player2List)){ 
                    StartCoroutine(playerTurn());
                }
            } else if (currentTeam == 2){ 
                if (!teamDead(playerList) & !teamDead(player2List)){ 
                    StartCoroutine(playerTurn());
                }
            }
        }

        if (PvM & !wait){ 
            if (teamDead(playerList) | teamDead(enemyList)){ 
                if (teamDead(enemyList)){
                    
                    if ((saveScript.currentStage == saveScript.unlockedStage) & (saveScript.unlockedStage != 2)){
                        saveScript.currentStage++;
                        saveScript.unlockedStage++;
                    }
                    saveScript.save();
                    
                    Debug.Log("Fin du combat, le joueur a gagné.");
                    buttonScript.setInfoText("Fin du combat, le joueur a gagné.");
                    buttonScript.DisplayEndGame("Victoire du Joueur !");
                    buttonScript.setAttackerName("");
                    PlayEndMusic(true);
                } else {
                    Debug.Log("Fin du combat, l'ennemi a gagné.");
                    buttonScript.setInfoText("Fin du combat, l'ennemi a gagné.");
                    buttonScript.DisplayEndGame("Défaite !");
                    buttonScript.setAttackerName("");
                    PlayEndMusic(false);
                }
                wait = true;
                isBattleOver = true;
                currentPhase = BattlePhase.WAITING;
                buttonScript.ButtonAccess();
                buttonScript.ClearAllBars();
            }
            if (currentTeam == 1)
            { 
                if (!teamDead(playerList) & !teamDead(enemyList))
                { 
                    StartCoroutine(playerTurn());
                }
            }else if (currentTeam == -1){ 
                if (!teamDead(playerList) & !teamDead(enemyList))
                {
                    StartCoroutine(enemyTurn());
                }
            }
        }
    }

    
    /// <summary>
    /// Runs character, skill, and target selection for the active player team.
    /// </summary>
    private IEnumerator playerTurn(){
        currentPhase = BattlePhase.WAITING;
        Debug.Log("Tour de l'équipe " + currentTeam + ".");
        buttonScript.setAnnouncementText("Tour de l'équipe " + currentTeam + ".");
        buttonScript.setAttackerName("");
        wait = true;
        usedCharacter = null;
        finishedTurn = false;
        currentTeamList = (currentTeam == 1) ? playerList : (currentTeam == 2) ? player2List : null;
        int maxActionsThisTurn = currentTeamList.Count(c => c != null && !isDead(c));
        turnCount = 0;
        foreach (GameObject c in currentTeamList){ 
            if (!finishedTurn && c != null && !isDead(c)){ 
                
                turnCount += 1;
                selectedCharacter = null;
                selectedSkill = -1;
                selectedTargets = null;

                
                currentPhase = BattlePhase.SELECT_CHARACTER;
                buttonScript.ButtonAccess();
                Debug.Log("Veuillez selectionner un personnage.");
                buttonScript.setInstructionText("Veuillez selectionner un personnage.");
                yield return new WaitUntil(() => (selectedCharacter != null) | finishedTurn);
                if (finishedTurn) {
                    usedCharacter = null;
                    wait = false;
                    break; 
                }
        
                
                yield return new WaitUntil(() => (selectedSkill != -1 | finishedTurn));
                if (finishedTurn) {
                    selectedCharacter.GetComponent<Character>().Deselect();
                    buttonScript.setAttackerName("");
                    usedCharacter = null;
                    wait = false;
                    break; 
                }

                
                if (selectedTargets == null) {
                    yield return new WaitUntil(() => (selectedTargets != null | finishedTurn));
                    if (finishedTurn) {
                        selectedCharacter.GetComponent<Character>().Deselect();
                        buttonScript.setAttackerName("");
                        usedCharacter = null;
                        wait = false;
                        break; 
                    }   
                }
                currentPhase = BattlePhase.WAITING;
                
                statusHandler();

                
                skillHandler();

                buttonScript.setAnnouncementText("Waiting...",2f);
                yield return new WaitForSeconds(2f);
                if (selectedCharacter != null)
                {
                    Character charac = selectedCharacter.GetComponent<Character>();
                    if (charac != null)
                    {
                        charac.Deselect();
                    }
                }
                
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
                if (teamDead(enemyList) | teamDead(playerList) | teamDead(player2List)){ 
                    finishedTurn = true;
                    wait = false;
                    buttonScript.setInstructionText("Fin du combat.");
                    
                } else if (turnCount == 2){ 
                    usedCharacter = null;
                    finishedTurn = true;
                    wait = false;

                }else if (turnCount >= maxActionsThisTurn)
                {
                    usedCharacter = null;
                    finishedTurn = true;
                    wait = false;
                }
                else {
                    
                    if (currentTeamList[0] != null)
                    {
                        Character chara0 = currentTeamList[0].GetComponent<Character>();
                        if (chara0 != null)
                        {
                            statusList = new List<(Status, int)>(chara0.getStatusList());

                            foreach ((Status, int) s in statusList)
                            {
                                if (s.Item1 == Status.FROZEN)
                                {
                                    usedCharacter = null;
                                    finishedTurn = true;
                                    wait = false;
                                }
                            }
                        }
                    }

                    if (currentTeamList.Length > 1 && currentTeamList[1] != null)
                    {
                        Character chara1 = currentTeamList[1].GetComponent<Character>();
                        if (chara1 != null)
                        {
                            statusList = new List<(Status, int)>(chara1.getStatusList());

                            foreach ((Status, int) s in statusList)
                            {
                                if (s.Item1 == Status.FROZEN)
                                {
                                    usedCharacter = null;
                                    finishedTurn = true;
                                    wait = false;
                                }
                            }
                        }
                    }

                    
                    usedCharacter = selectedCharacter;
                }
            }
        }

        
        if (PvM){
            statusUpdate(playerList);
            currentTeam = -1; 
        } else if (currentTeam == 1){
            statusUpdate(playerList);
            currentTeam = 2; 
        } else {
            statusUpdate(player2List);
            currentTeam = 1; 
        }
    }

    /// <summary>
    /// Executes enemy AI decisions and attacks for the enemy turn.
    /// </summary>
    private IEnumerator enemyTurn()
    {

        wait = true;
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Tour de l'ennemi.");
        buttonScript.setAnnouncementText("Tour de l'ennemi.");
        buttonScript.setAttackerName("");
        buttonScript.setInstructionText("L'ennemi attaque...");
        buttonScript.getButtonEndTurn().SetState(ButtonState.BLOCKED);
        wait = true;


        
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

        List<GameObject> targetsWithCircle = new List<GameObject>(); 
        yield return new WaitForSeconds(4f); 
        
        for (int i = 0; i < enemyList.Length; i++)
        {
            
            charactersAlive.RemoveAll(c => c == null || isDead(c));
            enemiesAlive.RemoveAll(e => e == null || isDead(e));
            if (charactersAlive.Count == 0)
            {
                break;
            }

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
            enemy.ShowActiveCircle(); 

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

            
            if (target == null || target.GetComponent<Character>() == null || isDead(target))
            {
                Debug.LogWarning("Target invalide, nouvelle sélection...");

                
                target = ai.DecideTarget(enemy, charactersAlive, enemiesAlive);

                
                if (target == null && charactersAlive.Count > 0)
                {
                    target = charactersAlive[Random.Range(0, charactersAlive.Count)];
                }
            }

            yield return new WaitForSeconds(2f); 

            
            effect = false;
            statusList = enemy.getStatusList();
            foreach ((Status,int) s in statusList){
                if (s.Item1 == Status.FROZEN) effect = true;
            }
            if (effect){
                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" est gelé et ne peut pas attaquer.");

            } else {

                
                if (target != null && target.GetComponent<Character>() != null){
                    effect = false;
                    statusList = target.GetComponent<Character>().getStatusList();
                    foreach ((Status,int) s in statusList){
                        if (s.Item1 == Status.SHIELDED) effect = true; 
                    }
                }

                
                selectedTargets = charactersAlive.Where(c => c != null && !isDead(c)).ToArray();

                
                if (target != null && target.GetComponent<Character>() != null){
                    for (int x=0; x<selectedTargets.Length; x++) {
                        if (selectedTargets[x] == null) continue;

                        Character selectedTargetCharacter = selectedTargets[x].GetComponent<Character>();
                        if (selectedTargetCharacter == null) continue;

                        statusList = selectedTargetCharacter.getStatusList();
                        foreach ((Status, int) s in statusList){
                            if (s.Item1 == Status.PROTECTED) {
                                if (charactersAlive.Count < 2)
                                {
                                    selectedTargets = charactersAlive.Where(c => c != null && !isDead(c)).ToArray();
                                    continue;
                                }

                                
                                if (charactersAlive.ElementAt(0) == null)
                                {
                                    charactersAlive.ElementAt(1).GetComponent<Character>().getStatusList().Remove(s);
                                }
                                else if (charactersAlive.ElementAt(1) == null)
                                {
                                    charactersAlive.ElementAt(0).GetComponent<Character>().getStatusList().Remove(s);

                                }
                                else
                                { 
                                    target = (charactersAlive.ElementAt(0).GetComponent<Protector>() != null) ? charactersAlive.ElementAt(0) : charactersAlive.ElementAt(1);
                                }

                                
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

                
                selectedTargets = selectedTargets
                    .Where(t => t != null && t.GetComponent<Character>() != null && !isDead(t))
                    .ToArray();


                
                if (target == null || target.GetComponent<Character>() == null || isDead(target))
                {
                    Debug.Log("Target invalide, nouvelle sélection...");

                    
                    target = ai.DecideTarget(enemy, charactersAlive, enemiesAlive);

                    
                    if (target == null && charactersAlive.Count > 0)
                    {
                        target = charactersAlive[Random.Range(0, charactersAlive.Count)];
                    }
                }

                switch (action)
                {
                    
                    case 1:
                        if (target != null)
                        {
                            if (!effect){ 
                                Character targetCharacter = target.GetComponent<Character>();
                                if (targetCharacter == null)
                                {
                                    break;
                                }

                                
                                targetCharacter.ShowTargetCircle(Color.red);
                                if (!targetsWithCircle.Contains(target))
                                    targetsWithCircle.Add(target);

                                enemy.TargetedAttack(target);

                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a attaqué "+target.name+".");

                            } else {
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a sauté son tour.");
                            }
                            
                            if (isDead(target))
                            {
                                charactersAlive.Remove(target);
                            }
                        }
                        break;

                    
                    case 2:
                        if (charactersAlive.Count > 0 && selectedTargets.Length > 0)
                        {
                            if (!effect){ 
                                
                                
                                foreach (var t in selectedTargets)
                                {
                                    if (t == null) continue;
                                    Character tCharacter = t.GetComponent<Character>();
                                    if (tCharacter == null) continue;

                                    tCharacter.ShowTargetCircle(Color.red);
                                    if (!targetsWithCircle.Contains(t))
                                        targetsWithCircle.Add(t);
                                }

                                enemy.AoeAttack(selectedTargets);
                                StartCoroutine(cameraShake(cameraShakeCurve));
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" fait une attaque de zone.");
                                
                            } else {
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a sauté son tour.");
                            }

                            
                            for (int idx = charactersAlive.Count - 1; idx >= 0; idx--)
                            {
                                if (isDead(charactersAlive[idx])) charactersAlive.RemoveAt(idx);
                            }
                        }
                        break;

                    
                    case 3:
                        Boss boss = enemy as Boss;
                        if (boss != null && charactersAlive.Count > 0 && selectedTargets.Length > 0)
                        {
                            if (!effect){ 
                                
                                foreach (var t in selectedTargets)
                                {
                                    if (t == null) continue;
                                    Character tCharacter = t.GetComponent<Character>();
                                    if (tCharacter == null) continue;

                                    tCharacter.ShowTargetCircle(Color.red);
                                    if (!targetsWithCircle.Contains(t))
                                        targetsWithCircle.Add(t);
                                }

                                boss.SpecialAttack(selectedTargets);
                                StartCoroutine(cameraShake(cameraShakeCurveLvl3));
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a utilisé son attaque spéciale.");
                               
                            } else {
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a sauté son tour.");
                            }

                            
                            for (int idx = charactersAlive.Count - 1; idx >= 0; idx--)
                            {
                                if (isDead(charactersAlive[idx])) charactersAlive.RemoveAt(idx);
                            }
                        }
                        break;

                    case 4:
                        
                        if (target != null)
                        {
                            Enemy targetEnemy = target.GetComponent<Enemy>();
                            
                            if (targetEnemy != null && targetEnemy.CurrentHP < targetEnemy.MaxHP)
                            {
                                
                                targetEnemy.ShowTargetCircle();
                                if (!targetsWithCircle.Contains(target))
                                    targetsWithCircle.Add(target);

                                enemy.Heal(target);

                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a soigné "+target.name+".");
                            }
                            else
                            {
                                
                                buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a tenté de soigner "+target.name+" mais il est déjà à pleine vie.");
                            }
                        }
                        break;

                    case 5:
                        
                        if (target != null)
                        {
                            
                            target.GetComponent<Enemy>().ShowTargetCircle();
                            if (!targetsWithCircle.Contains(target))
                                targetsWithCircle.Add(target);

                            enemy.BoostAttack(target);

                            target.GetComponent<Enemy>().AddBuffEffect(reinforcedEffectPrefab);

                            buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a renforcé les attaques de "+target.name+".");
                        }
                        break;

                    case 6:
                        
                        if (target != null)
                        {
                            
                            target.GetComponent<Enemy>().ShowTargetCircle();
                            if (!targetsWithCircle.Contains(target))
                                targetsWithCircle.Add(target);

                            enemy.Protection(target);

                            target.GetComponent<Enemy>().AddShieldEffect(shieldedEffectPrefab);

                            buttonScript.setInfoText("L'ennemi "+enemy.enemyName+" a protégé "+target.name+".");
                        }
                        break;
                }
            }

            
            enemy.EndTurnConsumeTemporaryEffects();

            yield return new WaitForSeconds(3f);

            foreach (GameObject c in targetsWithCircle)
            {
                if (c == null) continue; 
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
            
            if (member != null && !isDead(member)) {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Returns true when all members in the given team are dead or destroyed.
    /// </summary>
    private bool teamDead(GameObject[] team){
        
        if (team == null) return false;

        foreach(GameObject c in team){ 
            if (!isDead(c)){ 
                return false;
            }
        }
        buttonScript.setInfoText("L'équipe " + ((currentTeam == 1) ? "1" : (currentTeam == 2) ? "2" : "ennemie") + " est morte.");
        return true; 
    }

    /// <summary>
    /// Returns true when the provided target is dead or destroyed.
    /// </summary>
    private bool isDead(GameObject target){
        if (target == null) {
        return true; 
     }else if (target.GetComponent<Character>() != null){ 
            if (target.GetComponent<Character>().getCurrentHP() <= 0){ 
                Debug.Log(target.name + " is dead");
                buttonScript.setInfoText(target.name + " est mort.");
                return true; 
            }

        } else if (target.GetComponent<Enemy>() != null){ 
            if (target.GetComponent<Enemy>().CurrentHP <= 0){  
                Debug.Log(target.name + " is dead");
                buttonScript.setInfoText(target.name + " est mort.");
                return true; 
            }
        }
        return false; 
    }

    /// <summary>
    /// Resolves status interactions before applying skill effects.
    /// </summary>
    private void statusHandler(){

        if (selectedTargets == null || selectedTargets.Length == 0)
        {
            return;
        }
        
        if (selectedTargets.Length == 1 && selectedTargets[0] != null)
        { 
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

        for (int i=0; i<selectedTargets.Length; i++){ 

            if (selectedTargets[i] == null) continue;

            characterScript = selectedTargets[i].GetComponent<Character>();
            if (characterScript == null) continue; 

            statusList = new List<(Status,int)> (characterScript.getStatusList());

            foreach ((Status,int) s in statusList){
                if (s.Item1 == Status.PROTECTED){ 
                
                    if (currentTeam == 1 & PvP){ 
                        
                        GameObject player2Alive0 = (player2List != null && player2List.Length > 0) ? player2List[0] : null;
                        GameObject player2Alive1 = (player2List != null && player2List.Length > 1) ? player2List[1] : null;
                        if (player2Alive0 == null){
                            Character player2Character1 = player2Alive1 != null ? player2Alive1.GetComponent<Character>() : null;
                            if (player2Character1 != null) player2Character1.getStatusList().Remove(s);
                        } else if (player2Alive1 == null){
                            Character player2Character0 = player2Alive0.GetComponent<Character>();
                            if (player2Character0 != null) player2Character0.getStatusList().Remove(s);

                        } else { 
                            selectedTargets = new GameObject[] {(player2Alive0.GetComponent<Protector>() != null) ? player2Alive0 : player2Alive1};
                        }
                        
                    } else {
                        
                        GameObject playerAlive0 = (playerList != null && playerList.Length > 0) ? playerList[0] : null;
                        GameObject playerAlive1 = (playerList != null && playerList.Length > 1) ? playerList[1] : null;
                        if (playerAlive0 == null){
                            Character playerCharacter1 = playerAlive1 != null ? playerAlive1.GetComponent<Character>() : null;
                            if (playerCharacter1 != null) playerCharacter1.getStatusList().Remove(s);
                        } else if (playerAlive1 == null){
                            Character playerCharacter0 = playerAlive0.GetComponent<Character>();
                            if (playerCharacter0 != null) playerCharacter0.getStatusList().Remove(s);

                        } else { 
                            selectedTargets = new GameObject[] {(playerAlive0.GetComponent<Protector>() != null) ? playerAlive0 : playerAlive1};
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Applies the selected skill and related visual effects to targets.
    /// </summary>
    private void skillHandler(){
        if (selectedCharacter == null || selectedTargets == null || selectedTargets.Length == 0 || selectedTargets[0] == null)
        {
            return;
        }
        characterScript = selectedCharacter.GetComponent<Character>();
        if (characterScript == null)
        {
            return;
        }

        
        if ((selectedCharacter.GetComponent<Fighter>() & selectedSkill == 1) | (selectedCharacter.GetComponent<Mage>() & selectedSkill == 1) | (selectedCharacter.GetComponent<Protector>() & selectedSkill == 2)){
            StartCoroutine(cameraShake(cameraShakeCurve));
        } else if ((selectedCharacter.GetComponent<Fighter>() & selectedSkill == 3) | (selectedCharacter.GetComponent<Mage>() & selectedSkill == 3)){
            StartCoroutine(cameraShake(cameraShakeCurveLvl3));
        }

        
        GameObject effect;
            
        if (selectedCharacter.GetComponent<Mage>() & selectedSkill == 2){
            if (selectedTargets[0].GetComponent<Character>() != null){ 
                effect = Instantiate((currentTeam == 1) ? frozenEffectPrefab1 : frozenEffectPrefab2, selectedTargets[0].transform.position, Quaternion.identity);
                frozenEffectList.Add(effect);
                StartCoroutine(addIceAnimation());
            
            } else if (selectedTargets[0].GetComponent<Enemy>() != null){ 
                effect = Instantiate(frozenEffectPrefab1, selectedTargets[0].transform.position, Quaternion.identity);
                frozenEffectList.Add(effect);
                StartCoroutine(addIceAnimation());
            }
        }
            
        if (selectedCharacter.GetComponent<Protector>() & selectedSkill == 3){ 
            foreach (GameObject target in selectedTargets){
                effect = Instantiate(shieldedEffectPrefab, target.transform.position + Vector3.left, Quaternion.identity);
                shieldedEffectList.Add(effect);
                StartCoroutine(addShieldAnimation());
            }
        }
            
        if ((selectedCharacter.GetComponent<Fighter>() | selectedCharacter.GetComponent<Healer>()) & selectedSkill == 2){ 
            effect = Instantiate(reinforcedEffectPrefab, selectedTargets[0].transform.position, Quaternion.identity);
            reinforcedEffectList.Add(effect);
            StartCoroutine(addReinforcementAnimation());
        }

        
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

    private IEnumerator addIceAnimation(){
        SpriteRenderer sprite = frozenEffectList.Last().GetComponent<SpriteRenderer>();
        float opacity = 0f;
        while (opacity < .75f){
            opacity += .1f;
            sprite.color = new Color(1f, 1f, 1f, opacity);
            yield return new WaitForSeconds(.01f);
        }
        sprite.color = new Color(1f, 1f, 1f, .75f);
    }

    private IEnumerator addShieldAnimation(){
        SpriteRenderer sprite = shieldedEffectList.Last().GetComponent<SpriteRenderer>();
        float opacity = 0f;
        while (opacity < .75f){
            opacity += .1f;
            sprite.color = new Color(1f, 1f, 1f, opacity);
            yield return new WaitForSeconds(.01f);
        }
        sprite.color = new Color(1f, 1f, 1f, .75f);
    }

    private IEnumerator addReinforcementAnimation(){
        SpriteRenderer sprite = reinforcedEffectList.Last().GetComponent<SpriteRenderer>();
        float opacity = 0f;
        while (opacity < .75f){
            opacity += .1f;
            sprite.color = new Color(1f, 1f, 1f, opacity);
            yield return new WaitForSeconds(.01f);
        }
        sprite.color = new Color(1f, 1f, 1f, .75f);
    }

    /// <summary>
    /// Updates temporary statuses at end of turn for all provided units.
    /// </summary>
    private void statusUpdate(GameObject[] list){
        
        for (int i=0; i<list.Length; i++){ 

            if (list[i] != null){ 

                characterScript = list[i].GetComponent<Character>();
                if (characterScript != null) { 
                    statusList = new List<(Status,int)> (characterScript.getStatusList());

                    foreach ((Status,int) s in statusList){ 
                        switch (s.Item1){
                            case Status.FROZEN :        
                                characterScript.getStatusList().Remove(s);
                                StartCoroutine(removeIce());
                                break;

                            case Status.PROTECTED :     
                                characterScript.getStatusList().Remove(s);
                                if (s.Item2 > 0){
                                    characterScript.getStatusList().Add((Status.PROTECTED, s.Item2-1)); 
                                } else {
                                    StartCoroutine(removeProtectionAnimation(list[i]));
                                }
                                break;

                            case Status.SHIELDED :      
                                characterScript.getStatusList().Remove(s);
                                if (s.Item2 > 0){
                                    characterScript.getStatusList().Add((Status.SHIELDED, s.Item2-1)); 
                                } else {
                                    StartCoroutine(removeShield());
                                }
                                break;

                            case Status.STRENGTHENED :  
                                characterScript.getStatusList().Remove(s);
                                if (s.Item2 > 0){
                                    characterScript.getStatusList().Add((Status.STRENGTHENED, s.Item2-1)); 
                                } else {
                                    characterScript.setDamageMultiplier(characterScript.getDamageMultiplier()/1.5f); 
                                    StartCoroutine(removeReinforcement());
                                }
                                break;
                        }
                    }
                }

                enemyScript = list[i].GetComponent<Enemy>();
                if (enemyScript != null) { 
                    statusList = new List<(Status,int)> (enemyScript.getStatusList());

                    foreach ((Status,int) s in statusList){ 
                        switch (s.Item1){
                            case Status.FROZEN :
                                enemyScript.getStatusList().Remove(s);
                                Destroy(frozenEffectList.ElementAt(0));
                                frozenEffectList.RemoveAt(0);
                                break;
                        }
                    }
                    continue;
                }
            }
        }

    }

    private IEnumerator removeIce(){
        SpriteRenderer sprite = frozenEffectList.ElementAt(0).GetComponent<SpriteRenderer>();
        float opacity = .75f;
        while (opacity > 0f){
            opacity -= .1f;
            sprite.color = new Color(1f, 1f, 1f, opacity);
            yield return new WaitForSeconds(.01f);
        }
        Destroy(frozenEffectList.ElementAt(0));
        frozenEffectList.RemoveAt(0);
    }

    private IEnumerator removeShield(){
        SpriteRenderer sprite = shieldedEffectList.ElementAt(0).GetComponent<SpriteRenderer>();
        float opacity = .75f;
        while (opacity > 0f){
            opacity -= .1f;
            sprite.color = new Color(1f, 1f, 1f, opacity);
            yield return new WaitForSeconds(.01f);
        }
        Destroy(shieldedEffectList.ElementAt(0));
        shieldedEffectList.RemoveAt(0);
    }

    private IEnumerator removeProtectionAnimation(GameObject target){
        SpriteRenderer sprite = target.GetComponent<SpriteRenderer>();
        float opacity = .5f;
        while (opacity < 1f){
            opacity += .1f;
            sprite.color = new Color(1f, 1f, 1f, opacity);
            yield return new WaitForSeconds(.01f);
        }
        sprite.color = Color.white;
    }

    private IEnumerator removeReinforcement(){
        SpriteRenderer sprite = reinforcedEffectList.ElementAt(0).GetComponent<SpriteRenderer>();
        float opacity = .75f;
        while (opacity > 0f){
            opacity -= .1f;
            sprite.color = new Color(1f, 1f, 1f, opacity);
            yield return new WaitForSeconds(.01f);
        }
        Destroy(reinforcedEffectList.ElementAt(0));
        reinforcedEffectList.RemoveAt(0);
    }

    /// <summary>
    /// Selects default targets automatically for multi-target skills.
    /// </summary>
    public void automaticTargetSelection(){
        if (currentTeam == -1 || selectedCharacter == null) return;
        effect = false;
        
        if ((selectedCharacter.GetComponent<Mage>() != null & selectedSkill == 3)| 
            (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 2)){ 
            
            currentPhase = BattlePhase.WAITING;
            buttonScript.ButtonAccess();
            if (PvM){ 
                selectedTargets = enemyList.Where(e => e != null && !isDead(e)).ToArray();
                Debug.Log("Tous les ennemis ont étés ciblés. (La compétence affecte tous les ennemis)");
            } else if (currentTeam == 1){
                
                GameObject targetTeamMember = player2List.FirstOrDefault(a => a != null && !isDead(a));
                if (targetTeamMember != null)
                {
                    Character targetTeamCharacter = targetTeamMember.GetComponent<Character>();
                    if (targetTeamCharacter != null)
                    {
                        statusList = targetTeamCharacter.getStatusList();
                        foreach ((Status,int) s in statusList){
                            if (s.Item1 == Status.SHIELDED){
                                effect = true;
                            }
                        }
                    }
                }
                if (effect){ 
                    Debug.LogWarning("Les adversaires ont un bouclier et ne recevront pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                    buttonScript.setWarningText("Les adversaires ont un bouclier et ne recevront pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                    currentPhase = BattlePhase.SELECT_SKILL;
                    buttonScript.ButtonAccess();
                } else {
                    selectedTargets = player2List.Where(a => a != null && !isDead(a)).ToArray();
                    Debug.Log("Tous les ennemis ont étés ciblés. (La compétence affecte tous les ennemis)");
                }
            } else {
                
                GameObject targetTeamMember = playerList.FirstOrDefault(a => a != null && !isDead(a));
                if (targetTeamMember != null)
                {
                    Character targetTeamCharacter = targetTeamMember.GetComponent<Character>();
                    if (targetTeamCharacter != null)
                    {
                        statusList = targetTeamCharacter.getStatusList();
                        foreach ((Status,int) s in statusList){
                            if (s.Item1 == Status.SHIELDED){
                                effect = true;
                            }
                        }
                    }
                }
                if (effect){ 
                    Debug.LogWarning("Les adversaires ont un bouclier et ne recevront pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                    buttonScript.setWarningText("Les adversaires ont un bouclier et ne recevront pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                    currentPhase = BattlePhase.SELECT_SKILL;
                    buttonScript.ButtonAccess();
                } else {
                    selectedTargets = playerList.Where(a => a != null && !isDead(a)).ToArray();
                    Debug.Log("Tous les ennemis ont étés ciblés. (La compétence affecte tous les ennemis)");
                }
            }
        } else if ((selectedCharacter.GetComponent<Healer>() != null & selectedSkill == 3)| 
            (selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 3)){ 
            
            currentPhase = BattlePhase.WAITING;
            buttonScript.ButtonAccess();
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
            selectedTargets = allies.Where(a => a != null && !isDead(a)).ToArray();
            Debug.Log("Tous les alliés ont étés ciblés. (La compétence affecte tous les alliés)");
        } else if ((selectedCharacter.GetComponent<Fighter>() != null & selectedSkill == 2)){ 
            
            currentPhase = BattlePhase.WAITING;
            buttonScript.ButtonAccess();
            if(selectedCharacter.GetComponent<Fighter>().getStatusList().Contains((Status.STRENGTHENED,1))){
                Debug.LogWarning("Le personnage est déjà renforcé et ne peut pas utiliser cette compétence.");
                buttonScript.setWarningText("Le personnage est déjà renforcé et ne peut pas utiliser cette compétence.");
                currentPhase = BattlePhase.SELECT_SKILL;
                buttonScript.ButtonAccess();
                return;
            }
            selectedTargets = new GameObject[] {selectedCharacter};
            Debug.Log("Il n'y a pas besoin de sélectionner une cible. (La compétence n'a pas besoin de cible)");
            
        }else if ((selectedCharacter.GetComponent<Protector>() != null & selectedSkill == 1)){ 
            
            currentPhase = BattlePhase.WAITING;
            buttonScript.ButtonAccess();
            GameObject ally0 = (currentTeamList != null && currentTeamList.Length > 0) ? currentTeamList[0] : null;
            GameObject ally1 = (currentTeamList != null && currentTeamList.Length > 1) ? currentTeamList[1] : null;
            if (ally0 == null | ally1 == null){
                Debug.LogWarning("Le protecteur n'a pas d'allié et donc personne à protéger. Veuillez choisir une autre compétence ou passer le tour.");
                buttonScript.setWarningText("Le protecteur n'a pas d'allié et donc personne à protéger. Veuillez choisir une autre compétence ou passer le tour.");
                currentPhase = BattlePhase.SELECT_SKILL;
                buttonScript.ButtonAccess();
                return;
            }
            
            selectedTargets = new GameObject[] {(ally0.GetComponent<Protector>() != null) ? ally1 : ally0};
            Debug.Log("L'allié a été ciblé. (La compétence affecte l'allié)");
        }
    }
    /// <summary>
    /// Handles click-based character and target selection during combat.
    /// </summary>
    public void select(GameObject clickedObject){
        if (clickedObject == null)
        {
            return;
        }
        if (currentTeam == -1) {
            if(clickedObject.GetComponent<Character>() != null){
                clickedObject.GetComponent<Character>().Deselect();
            }
            else if (clickedObject.GetComponent<Enemy>() != null){
                clickedObject.GetComponent<Enemy>().Deselect();
            }
            return;
        }
        
        clickedTeam = (clickedObject.GetComponent<Character>() != null) ? clickedObject.GetComponent<Character>().getTeamID() : -1; 
        effect = false;

        if (currentPhase == BattlePhase.SELECT_CHARACTER){
            
            if (clickedObject.GetComponent<Character>() == null) {
                Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                if (clickedObject.GetComponent<Enemy>() != null) 
                    clickedObject.GetComponent<Enemy>().Deselect();
                return;
            }

            
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
                if (clickedObject != usedCharacter){ 
                    selectedCharacter = clickedObject;
                    buttonScript.setAttackerName(selectedCharacter.name);
                    Debug.Log("Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");
                    buttonScript.setInstructionText("Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");

                    currentPhase = BattlePhase.SELECT_SKILL;
                    buttonScript.ButtonAccess();


                } else { 
                    Debug.LogWarning("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                    buttonScript.setWarningText("Vous avez déjà utilisé ce personnage. Veuillez en choisir un autre ou passer le tour.");
                    Character character_selected = clickedObject.GetComponent<Character>();
                    if (character_selected != null){
                        character_selected.Deselect();
                    }

                }

            } else { 
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

            
            if (clickedObject.GetComponent<Character>() == null) {
                Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe. Veuillez choisir un autre personnage.");
                if (clickedObject.GetComponent<Enemy>() != null) 
                    clickedObject.GetComponent<Enemy>().Deselect();
                return;
            }

            
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


            } else if (clickedTeam == currentTeam & clickedObject != selectedCharacter){ 
                selectedCharacter.GetComponent<Character>().Deselect();
                selectedCharacter = clickedObject;
                buttonScript.setAttackerName(selectedCharacter.name);
                Debug.Log("Changement.Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");
                buttonScript.setInstructionText("Changement. Vous avez sélectionné le personnage " + selectedCharacter.name + ". Veuillez choisir une compétence.");
                currentPhase = BattlePhase.SELECT_SKILL;
                buttonScript.ButtonAccess();

            } else if (clickedTeam != currentTeam){ 
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
            if (clickedTeam == currentTeam){ 

                if ((selectedCharacter.GetComponent<Healer>() & selectedSkill != 0) | (selectedCharacter.GetComponent<Protector>() & selectedSkill != 2 & selectedSkill != 0)){ 
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


                } else if (clickedObject != selectedCharacter){ 
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


                } else { 
                    Debug.LogWarning("Ce personnage fait partie de votre équipe, vous ne pouvez pas l'attaquer. Veuillez choisir un autre personnage.");
                    buttonScript.setWarningText("Ce personnage fait partie de votre équipe, vous ne pouvez pas l'attaquer. Veuillez choisir un autre personnage.");
                    clickedObject.GetComponent<Character>().Deselect();
                }   

            } else { 

                
                
                Character c = clickedObject.GetComponent<Character>();
                if (c != null)
                {
                    statusList = new List<(Status, int)>(c.getStatusList());
                }
                else
                {
                    statusList = new List<(Status, int)>(); 
                }

                foreach ((Status,int) s in statusList){
                    if (s.Item1 == Status.SHIELDED){
                        effect = true;
                    }
                }
                if (effect){
                    if (!(selectedCharacter.GetComponent<Mage>() & selectedSkill == 2)){ 
                        Debug.LogWarning("Ce personnage a un bouclier et ne recevra pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                        buttonScript.setWarningText("Ce personnage a un bouclier et ne recevra pas de dégats. Veuillez choisir une autre compétence ou passer le tour.");
                        Character character_selected = clickedObject.GetComponent<Character>();
                        if(clickedObject.GetComponent<Character>() != null){
                            clickedObject.GetComponent<Character>().Deselect();
                        }
                        else if (clickedObject.GetComponent<Enemy>() != null){
                            clickedObject.GetComponent<Enemy>().Deselect();
                        }

                    } else { 
                    selectedTargets = new GameObject[] {clickedObject};
                    Debug.Log("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");
                    buttonScript.setInstructionText("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");

                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();
                    }

                } else if ((selectedCharacter.GetComponent<Healer>() & selectedSkill != 0)   | (selectedCharacter.GetComponent<Protector>() & selectedSkill != 2 & selectedSkill != 0)){ 
                    Debug.LogWarning("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le cibler (compétence aidant un allié). Veuillez choisir un autre personnage.");
                    buttonScript.setWarningText("Ce personnage ne fait pas partie de votre équipe, vous ne pouvez pas le cibler (compétence aidant un allié). Veuillez choisir un autre personnage.");

                    if(clickedObject.GetComponent<Character>() != null){
                        clickedObject.GetComponent<Character>().Deselect();
                    }
                    else if (clickedObject.GetComponent<Enemy>() != null){
                        clickedObject.GetComponent<Enemy>().Deselect();
                    }

                } else { 
                    
                    selectedTargets = new GameObject[] {clickedObject};
                    Debug.Log("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");
                    buttonScript.setInstructionText("Vous avez ciblé le personnage " + selectedTargets[0].name + ".");

                    currentPhase = BattlePhase.WAITING;
                    buttonScript.ButtonAccess();
                }
            }
        }
    }

    /// <summary>
    /// Applies a short camera shake effect for impactful actions.
    /// </summary>
    private IEnumerator cameraShake(AnimationCurve curve){
        Vector3 startPosition = mainCamera.transform.position;
        float elapsedTime = 0f;
        float strenght = curve.Evaluate(elapsedTime/1f);

        while (elapsedTime < 1f){
            elapsedTime += Time.deltaTime;
            strenght = curve.Evaluate(elapsedTime/1f);
            mainCamera.transform.position = startPosition + Random.insideUnitSphere * strenght;
            yield return null;
        }

        mainCamera.transform.position = startPosition;
    }

    /// <summary>
    /// Plays victory or defeat music when the battle ends.
    /// </summary>
    private void PlayEndMusic(bool playerWon) {
        
        if (MusicManager.instance == null)
        {
            return;
        }
        AudioClip clip;
        if (playerWon) {
            clip = victoryClip;
        } else {
            clip = defeatClip;
        }
        MusicManager.instance.SwitchTrack(clip, false);
    }
}