using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BattleUIController : MonoBehaviour
{
    public CombatButton simpleAttackButton;
    public CombatButton skillLvl1Button;
    public CombatButton skillLvl2Button;
    public CombatButton skillLvl3Button;
    public int teampPlaying = 1;
    public BattlePhase currentPhase = BattlePhase.SELECT_PLAYER;
    public GameObject currentTarget;
    public GameObject currentAttacker; // The entity targeted by the buttons

    // Update the combat buttons based on the current battle state.
    public void ButtonAccess()
    {
        
        if (currentPhase == BattlePhase.WAITING || currentAttacker == null)
        {
            simpleAttackButton.SetState(ButtonState.BLOCKED);
            skillLvl1Button.SetState(ButtonState.BLOCKED);
            skillLvl2Button.SetState(ButtonState.BLOCKED);
            skillLvl3Button.SetState(ButtonState.BLOCKED);
            return;
        }

        
    Character attackerStats = currentAttacker.GetComponent<Character>();

    
    simpleAttackButton.SetState(currentTarget != null ? ButtonState.SHOWN : ButtonState.BLOCKED);

    skillLvl1Button.SetState(currentTarget != null && attackerStats.getCurrentMP() >= attackerStats.getMpCostSkillLvl1() ? ButtonState.SHOWN : ButtonState.BLOCKED);

    skillLvl2Button.SetState(currentTarget != null && attackerStats.getCurrentMP() >= attackerStats.getMpCostSkillLvl2() ? ButtonState.SHOWN : ButtonState.BLOCKED);

    skillLvl3Button.SetState(currentTarget != null && attackerStats.getCurrentMP() >= attackerStats.getMpCostSkillLvl3() ? ButtonState.SHOWN : ButtonState.BLOCKED);
    }

    // Initialize the UI state when the scene starts.
    void Start()
    {
        ButtonAccess(); 
        Debug.Log("BattleUIController initialisé. Phase : " + currentPhase);
    }


    // Handle clicks on characters and enemies to set the attacker or target.
    public void HandleSelection(GameObject clickedObject)
    {
  
        if (currentPhase == BattlePhase.WAITING) return;

       
        Character clickedChar = clickedObject.GetComponent<Character>();
        Enemy clickedEnemy = clickedObject.GetComponent<Enemy>();

      
        int clickedTeam = (clickedChar != null) ? clickedChar.getTeamID() : (clickedEnemy != null ? clickedEnemy.TeamId : -1);

  
        if (currentPhase == BattlePhase.SELECT_PLAYER)
        {
        
            if (clickedChar != null && clickedTeam == teampPlaying)
            {
                currentAttacker = clickedObject;
                currentPhase = BattlePhase.SELECT_TARGET;
                Debug.Log("Attaquant choisi ! Maintenant, cliquez sur un ennemi.");
            }
            else
            {
                Debug.LogWarning("Sélection invalide : veuillez sélectionner un personnage de votre équipe.");
            }
        }
        else if (currentPhase == BattlePhase.SELECT_TARGET)
        {
           
            if (clickedTeam != teampPlaying && clickedTeam != -1)
            {
                currentTarget = clickedObject;

                Debug.Log("Cible choisie ! Cliquez sur une attaque.");
            }
            else if (clickedTeam == teampPlaying && clickedChar != null)
            {
                currentAttacker = clickedObject;
                currentTarget = null;            
                Debug.Log("Changement d'attaquant : " + clickedObject.name);
            }
            else
            {
                Debug.LogWarning("Sélection invalide : veuillez sélectionner un ennemi");
            }
        }
        ButtonAccess();
    }

    // Reset the selection state and switch to the other team.
    private void FinishTurn()
    {
        currentAttacker = null;
        currentTarget = null;
        teampPlaying = (teampPlaying == 1) ? 2 : 1; // Switch teams
        currentPhase = BattlePhase.SELECT_PLAYER;
        ButtonAccess(); 
        Debug.Log("Tour terminé. Au tour de l'équipe d'en face de jouer ! ");
    }

    // Resolve the basic attack button using the selected attacker and target.
    public void OnClickSimpleAttack()   
    {

    if (currentTarget == null || currentAttacker == null) return;


    
    int attackerTeam = currentAttacker.GetComponent<Character>().getTeamID();

  
    var enemy = currentTarget.GetComponent<Enemy>();
    var player = currentTarget.GetComponent<Character>();

   
    int targetTeam = (player != null) ? player.getTeamID() : (enemy != null ? enemy.TeamId : -1);

   
    if (targetTeam != -1 && targetTeam != attackerTeam)
    {
        currentPhase = BattlePhase.WAITING;
        ButtonAccess(); 
        if (enemy != null) currentAttacker.GetComponent<Character>().baseAttack(enemy.gameObject);
        if (player != null) currentAttacker.GetComponent<Character>().baseAttack(player.gameObject);
        
        Debug.Log("Attaque simple réussie sur : " + currentTarget.name);
        
        
        FinishTurn(); 
    }
    else 
    {
        Debug.LogWarning("Action impossible : cible alliée ou invalide.");
    }
    }

    // Resolve skill level 1 on the selected target.
    public void OnClickSkillLvl1()
    {
    
        if (currentTarget == null || currentAttacker == null) return;

        Character c = currentTarget.GetComponent<Character>();
        Enemy e = currentTarget.GetComponent<Enemy>();

        ButtonAccess(); 
        currentAttacker.GetComponent<Character>().skillLvl1(currentTarget); 
        Debug.Log("Attaque SkillLvl 1 lancé par " + currentAttacker.name + " sur " + currentTarget.name);
        FinishTurn();
    }



    // Resolve skill level 2 on the selected target.
    public void OnClickSkillLvl2()
    {
    if (currentAttacker == null) return; 

    currentPhase = BattlePhase.WAITING;
    ButtonAccess();

    List<GameObject> enemiesFound = new List<GameObject>();

    if (currentTarget != null)
    {
        enemiesFound.Add(currentTarget);
    }

    MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

    foreach (MonoBehaviour obj in allScripts)
    {

        Character c = obj.GetComponent<Character>();
        Enemy e = obj.GetComponent<Enemy>();


        if (c != null || e != null)
        {
           
            int targetTeam = (c != null) ? c.getTeamID() : e.TeamId;
            int hpTarget = (c != null) ? c.getCurrentHP() : e.CurrentHP;
            int attackerTeam = currentAttacker.GetComponent<Character>().getTeamID();

            if (targetTeam != attackerTeam && hpTarget > 0)
            {
                if (!enemiesFound.Contains(obj.gameObject)) {
                    enemiesFound.Add(obj.gameObject);
                }
            }
        }
    }

    
    GameObject[] targets = enemiesFound.ToArray();

   
    currentAttacker.GetComponent<Character>().skillLvl2(targets);
    Debug.Log("Attaque SkillLvl 2 lancée par " + currentAttacker.name);
    FinishTurn();
}
    

    // Resolve skill level 3 on the selected target.
    public void OnClickSkillLvl3()
{
    
    if (currentAttacker == null) return; 

    currentPhase = BattlePhase.WAITING;
    ButtonAccess();

    List<GameObject> enemiesFound = new List<GameObject>();

    if (currentTarget != null)
    {
        enemiesFound.Add(currentTarget);
    }

  
    MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

    foreach (MonoBehaviour obj in allScripts)
    {

        Character c = obj.GetComponent<Character>();
        Enemy e = obj.GetComponent<Enemy>();


        if (c != null || e != null)
        {
    
            int targetTeam = (c != null) ? c.getTeamID() : e.TeamId;
            int hpTarget = (c != null) ? c.getCurrentHP() : e.CurrentHP;
            int attackerTeam = currentAttacker.GetComponent<Character>().getTeamID();


            if (targetTeam != attackerTeam && hpTarget > 0)
            {
                if (!enemiesFound.Contains(obj.gameObject)) {
                enemiesFound.Add(obj.gameObject);
                }
            }
        }
    }

   
    GameObject[] targets = enemiesFound.ToArray();

    currentAttacker.GetComponent<Character>().skillLvl3(targets);
    Debug.Log("Attaque SkillLvl 3 lancée par " + currentAttacker.name);
    FinishTurn();
}

}