using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI
{
    // ----- Context data -----
    protected int countWeakness;
    protected int totalMaxHP_Character;
    protected int totalCurrentHP_Character;
    protected int totalMaxHP_Enemy;
    protected int totalCurrentHP_Enemy;
    protected int totalMP_Character;
    protected int totalCurrentMP_Character;

    protected GameObject lowestHPCharacter;
    protected GameObject lowestHPEnemy;

    protected EnemyBehaviour behaviour;

    protected int lastAction;
    protected int repeatCountAOE = 0;
    protected int specialAtkCooldown = 0;
    protected GameObject perfectTarget;

    protected IEnemyStrategy strategy;

    protected bool isShielded = false;
    protected bool isProtected = false;

    // ----- Getters and setters -----
    public int CountWeakness { get => countWeakness; set => countWeakness = value; }
    public int TotalMaxHP_Character { get => totalMaxHP_Character; set => totalMaxHP_Character = value; }
    public int TotalCurrentHP_Character { get => totalCurrentHP_Character; set => totalCurrentHP_Character = value; }
    public int TotalMaxHP_Enemy { get => totalMaxHP_Enemy; set => totalMaxHP_Enemy = value; }
    public int TotalCurrentHP_Enemy { get => totalCurrentHP_Enemy; set => totalCurrentHP_Enemy = value; }
    public int TotalMP_Character { get => totalMP_Character; set => totalMP_Character = value; }
    public int TotalCurrentMP_Character { get => totalCurrentMP_Character; set => totalCurrentMP_Character = value; }

    public GameObject LowestHPCharacter { get => lowestHPCharacter; set => lowestHPCharacter = value; }
    public GameObject LowestHPEnemy { get => lowestHPEnemy; set => lowestHPEnemy = value; }

    public EnemyBehaviour Behaviour { get => behaviour; set => behaviour = value; }

    public int LastAction { get => lastAction; set => lastAction = value; }
    public int RepeatCountAOE { get => repeatCountAOE; set => repeatCountAOE = value; }
    public int SpecialAtkCooldown { get => specialAtkCooldown; set => specialAtkCooldown = value; }
    public GameObject PerfectTarget { get => perfectTarget; set => perfectTarget = value; }

    public IEnemyStrategy Strategy { get => strategy; set => strategy = value; }

    public bool IsShielded { get => isShielded; set => isShielded = value; }
    public bool IsProtected { get => isProtected; set => isProtected = value; }

    // ----- Methods -----
    public int DecideAction(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        ///<summary>Decides the next action for the enemy based on the current context and behaviour.</summary>
        Debug.Log("Enemy: " + enemy.name);

        // Default action is targeted attack
        int action = 1; 

        // Context analysis
        AnalyseContext(enemy, playerList, enemyList);

        // Strategy selection based on enemy type
        switch (enemy.Type)
        {
            case EnemyType.DPS:
                strategy = new DPSStrategy();
                break;
            case EnemyType.Neutral:
                strategy = new NeutralStrategy();
                break;
            case EnemyType.Support:
                strategy = new SupportStrategy();
                break;
            default:
                strategy = new DPSStrategy();
                break;
        }
        behaviour = strategy.Decide(this, enemy, playerList, enemyList);

        // Cooldown management
        ReduceCooldown();

        // Convert behaviour to action
        // action : { 1 = targeted attack; 2 = aoe attack; 3 = special attack; 4 = heal; 5 = buff; 6 = protection}
        switch (behaviour)
        {
            case EnemyBehaviour.Targeted:
            case EnemyBehaviour.LowHPTarget:
                action = 1;
                break;
            case EnemyBehaviour.AOE_pressure:
                action = 2;
                break;
            case EnemyBehaviour.Special:
                action = 3;
                if (specialAtkCooldown == 0)
                {
                    specialAtkCooldown = 3;
                }
                break;
            case EnemyBehaviour.Weakness:
                action = (countWeakness == 2) ? 2 : 1;
                break;
            case EnemyBehaviour.Heal:
                action = 4;
                break;
            case EnemyBehaviour.Buff:
                action = 5;
                break;
            case EnemyBehaviour.Defensive:
                action = 6;
                break;
            default:
                action = 1;
                break;
        }

        // Avoid repeating AOE attack more than 2 times in a row
        if (action == 2)
            repeatCountAOE++;
        else
            repeatCountAOE = 0;

        // History management
        lastAction = action;

        return action;
    }

    public void AnalyseContext(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        /// <summary>Analyzes the current context for the enemy, including player and enemy stats.</summary>

        // Initialisation of context variables
        totalCurrentHP_Character = 0;
        totalMaxHP_Character = 0;
        totalCurrentHP_Enemy = 0;
        totalMaxHP_Enemy = 0;
        totalCurrentMP_Character = 0;
        totalMP_Character = 0;
        countWeakness = 0;
        lowestHPCharacter = null;
        perfectTarget = null;
        lowestHPEnemy = null;
        

        // Analyse player team
        for (int i = 0; i < playerList.Count; i++)
        {
            Character c = playerList[i].GetComponent<Character>();

            // count the number of characters weak to the enemy's attack type
            if (string.Equals(c.getWeakness().ToString(), enemy.AttackTypeUsed.ToString()))
            {
                countWeakness++;
            }

            // calculate total HP of the player team
            totalCurrentHP_Character += c.getCurrentHP();
            totalMaxHP_Character += c.getMaxHP();

            // calculate total MP of the player team
            totalCurrentMP_Character += c.getCurrentMP();   
            totalMP_Character += c.getMaxMP();

            // find the character with the lowest HP under a certain threshold (30% of max HP)
            if (c.getCurrentHP() < c.getMaxHP() * 0.3f)
            {
                if (lowestHPCharacter == null || c.getCurrentHP() < lowestHPCharacter.GetComponent<Character>().getCurrentHP())
                {
                    lowestHPCharacter = playerList[i];
                }
            }

            foreach (var status in c.getStatusList())
            {
                if (status.Item1 == Status.SHIELDED)
                {
                    isShielded = true;
                }

                if (status.Item1 == Status.PROTECTED)
                {
                    isProtected = true;
                }
            }
        }

        // Analyse enemies 
        for (int j = 0; j < enemyList.Count; j++)
        {
            Enemy currentEnemy = enemyList[j].GetComponent<Enemy>();

            // calculate total HP of the enemy team
            totalCurrentHP_Enemy += currentEnemy.CurrentHP;
            totalMaxHP_Enemy += currentEnemy.MaxHP;

            // find the ally with the lowest HP under a certain threshold (40% of max HP)
            if (currentEnemy.CurrentHP <= currentEnemy.MaxHP * 0.4f)
            {
                lowestHPEnemy = enemyList[j];
            }
        }
    }

    public GameObject DecideTarget(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        /// <summary>Decides the target for the enemy's action based on the current context and behaviour.</summary>
        return strategy.DecideTarget(this, enemy, playerList, enemyList);
    
    }

    private void ReduceCooldown()
    {
        /// <summary>Reduces the cooldown of the special attack if it's greater than 0.</summary>
        if (specialAtkCooldown > 0)
        {
            specialAtkCooldown--;
        }
    }

    public bool CanUseMP(Enemy enemy, int cost)
    {
        /// <summary>Checks if the enemy has enough MP to use a skill with the given cost.</summary>
        return enemy.CurrentMP >= cost;
    }

    public GameObject GetLowestHP(List<GameObject> team)
    {
        /// <param>The list of GameObjects representing the team to analyze.</param>
        /// <summary>Returns the GameObject with the lowest HP in the given team.</summary>
        
        GameObject lowest = null;
        int HP = int.MaxValue;

        foreach (var charac in team)
        {
            int current = int.MaxValue;
            if (charac.TryGetComponent<Character>(out var character))
            {
                current = character.getCurrentHP();
            }
            else if (charac.TryGetComponent<Enemy>(out var enemy))
            {
                current = enemy.CurrentHP;
            }

            if (current < HP)
            {
                HP = current;
                lowest = charac;
            }
        }

        return lowest;
    }
}