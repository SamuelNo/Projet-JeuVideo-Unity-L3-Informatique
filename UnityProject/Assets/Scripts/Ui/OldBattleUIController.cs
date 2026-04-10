using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class OldBattleUIController : MonoBehaviour
{    public CombatButton simpleAttackButton;
    public CombatButton skillLvl1Button;
    public CombatButton skillLvl2Button;
    public CombatButton skillLvl3Button;
    public CombatButton endTurnButton;
    public GameObject attackerNameObject;
    public int teamPlaying = 1;
    public BattlePhase currentPhase;
    public GameObject currentTarget;
    public GameObject currentAttacker;
    private GameObject firstAttacker;
    private bool turnEnded = false;


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
        if(endTurnButton != null) 
        { 
            endTurnButton.SetState(currentPhase != BattlePhase.WAITING ? ButtonState.SHOWN : ButtonState.BLOCKED);
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

  
        if (currentPhase == BattlePhase.SELECT_CHARACTER)
        {
        
            if (clickedChar != null && clickedTeam == teamPlaying)
            {
                if(firstAttacker != null && clickedObject == firstAttacker) 
                {
                    Debug.LogWarning("Vous avez déjà choisi ce personnage comme attaquant. Veuillez en choisir un autre ou passer le tour.");
                }
                else
                {
                    currentAttacker = clickedObject;
                    firstAttacker = currentAttacker;
                    attackerNameObject.GetComponent<UnityEngine.Component>().SendMessage("set_text", currentAttacker.name);
                    currentPhase = BattlePhase.SELECT_TARGET;
                    Debug.Log("Attaquant: " + currentAttacker.name + ". Maintenant, cliquez sur un ennemi.");
                }
            }
            else
            {
                Debug.LogWarning("Sélection invalide : veuillez sélectionner un personnage de votre équipe.");
            }
        }
        else if (currentPhase == BattlePhase.SELECT_TARGET)
        {
           
            if (clickedTeam != teamPlaying && clickedTeam != -1)
            {
                currentTarget = clickedObject;

                Debug.Log("Cible : " + currentTarget.name + " ! Cliquez sur une attaque.");
            }
            else if (clickedTeam == teamPlaying && clickedChar != null)
            {
                if(turnEnded) 
                {
                    Debug.LogWarning(clickedObject.name + " a déjà été sélectionné comme attaquant.");
                }
                else if(clickedObject == firstAttacker)
                {
                        Debug.LogWarning("Vous avez déjà choisi " + clickedObject.name + " comme attaquant. Sélectionnez un enemi ou cliquez sur 'Fin du tour'.");
                    }
                else
                {
                    currentAttacker = clickedObject;
                    firstAttacker = currentAttacker;
                    attackerNameObject.GetComponent<UnityEngine.Component>().SendMessage("set_text", currentAttacker.name);
                    currentTarget = null;            
                    Debug.Log("Changement d'attaquant : " + clickedObject.name+". Maintenant, cliquez sur un ennemi.");
                }
            }
        }
        ButtonAccess();
    }

/*
    // Reset the selection state and switch to the other team.
    private void FinishTurn()
    {
        if(turnEnded) 
        {
            firstAttacker = null;
            currentAttacker = null;
            currentTarget = null;
            turnEnded = false;
            teamPlaying = (teamPlaying == 1) ? 2 : 1; // Switch teams
            attackerNameObject.GetComponent<UnityEngine.Component>().SendMessage("set_text", "");
            currentPhase = BattlePhase.SELECT_CHARACTER;
            ButtonAccess(); 
            Debug.Log("Tour terminé. Au tour de l'équipe " + teamPlaying + " de jouer ! ");
        }
        else
        {
            currentAttacker = null;
            currentTarget = null;
            attackerNameObject.GetComponent<UnityEngine.Component>().SendMessage("set_text", "");
            turnEnded = true;
            currentPhase = BattlePhase.SELECT_CHARACTER;
            ButtonAccess(); 
            Debug.Log("Sélectionner votre deuxième attaquant ou cliquez : 'fin du tour.'");
        }
        
    }
    // Resolve the basic attack button using the selected attacker and target.
    public void OnClickSimpleAttack()   
    {

    if (currentTarget == null || currentAttacker == null) return;

    currentPhase = BattlePhase.WAITING;
    ButtonAccess(); 
    currentAttacker.GetComponent<Character>().baseAttack(currentTarget);
    Debug.Log("Attaque simple réussie lancé par " + currentAttacker.name + " sur : " + currentTarget.name);
    FinishTurn();
    }

    // Resolve skill level 1 on the selected target.
    public void OnClickSkillLvl1()
    {
    
        if (currentTarget == null || currentAttacker == null) return;
        currentPhase = BattlePhase.WAITING;
        ButtonAccess();
        currentAttacker.GetComponent<Character>().skillLvl1(currentTarget); 
        Debug.Log("Attaque SkillLvl 1 lancé par " + currentAttacker.name + " sur " + currentTarget.name);
        FinishTurn();
    }



    // Resolve skill level 2 on the selected target.
    public void OnClickSkillLvl2()
    {
    if (currentAttacker == null || currentTarget == null) return; 
    currentPhase = BattlePhase.WAITING;
    ButtonAccess();

    List<GameObject> enemiesFound = new List<GameObject>();
    enemiesFound.Add(currentTarget);
    

    MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);

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
    Debug.Log("Attaque SkillLvl 2 lancée par " + currentAttacker.name + " sur " + currentTarget.name);
    FinishTurn();
}
    

    // Resolve skill level 3 on the selected target.
    public void OnClickSkillLvl3()
{
    
    if (currentAttacker == null || currentTarget == null) return; 

    currentPhase = BattlePhase.WAITING;
    ButtonAccess();

    List<GameObject> enemiesFound = new List<GameObject>();
    enemiesFound.Add(currentTarget);


  
    MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);

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
    Debug.Log("Attaque SkillLvl 3 lancée par " + currentAttacker.name + " sur " + currentTarget.name);
    FinishTurn();
}
public void OnClickEndTurn()
{
    Debug.Log("Le joueur a cliqué sur Fin de Tour.");
    turnEnded = true;
    FinishTurn();
}
*/
}