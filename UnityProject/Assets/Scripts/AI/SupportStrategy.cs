using UnityEngine;
using System.Collections.Generic;

public class SupportStrategy : IEnemyStrategy
{
    public EnemyBehaviour Decide(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        /// <summary> Decides the behavior for a Support type enemy based on its rank and the current context.</summary>
        switch (enemy.Rank)
        {
            case Rank.C:
                return DecideC(ai, enemy, playerList, enemyList);
            case Rank.B:
                return DecideB(ai, enemy, playerList, enemyList);
            case Rank.A:
                return DecideA(ai, enemy, playerList, enemyList);
            case Rank.S:
                return DecideS(ai, enemy, playerList, enemyList);
            default:
                return EnemyBehaviour.Targeted;
        }
    }

    public GameObject DecideTarget(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        /// <summary> Decides the target for the enemy's action based on its behavior and the current context.</summary>
        GameObject target = null;

        // if the behavior is targeted, the enemy will attack the character with the lowest HP
        if (ai.Behaviour == EnemyBehaviour.Targeted)
        {
            if (ai.LowestHPCharacter != null)
            {
                target = ai.LowestHPCharacter;
            }
            else
            {
                target = ai.GetLowestHP(playerList);
            }
        }

        // if the behavior is heal, the enemy will heal the ally with the lowest HP
        else if (ai.Behaviour == EnemyBehaviour.Heal)
        {
            target = GetPriorityLowHP(ai, enemyList, 0.15f);
        }

        // if the behavior is buff, the enemy will buff the ally with the highest base power
        else if (ai.Behaviour == EnemyBehaviour.Buff)
        {
            int max = 0;
            for (int i = 0; i < enemyList.Count; i++)
            {
                Enemy e = enemyList[i].GetComponent<Enemy>();
                if (e.Type == EnemyType.DPS || e.Type == EnemyType.Neutral)
                {
                    if (e.BasePower > max)
                    {
                        max = e.BasePower;
                        target = enemyList[i];
                    }
                }
            }
        }

        // if the behavior is defensive, the enemy will protect the ally with the lowest HP
        else if (ai.Behaviour == EnemyBehaviour.Defensive)
        {
            target = GetPriorityLowHP(ai, enemyList, 0.15f);

        }

        return target;
    }


    // -- Decide the behavior of the enemy depending on its rank and the situation of the fight --
    public EnemyBehaviour DecideC(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        ///<summary> Decide the behavior for Rank C enemies </summary>
        // if an ally has low HP, the enemy will heal it, otherwise it will do a targeted attack
        if (ai.LowestHPEnemy != null)
        {
            if (ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
            {
                return EnemyBehaviour.Heal;
            }
        }
        return EnemyBehaviour.Targeted;
    }

    public EnemyBehaviour DecideB(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        ///<summary> Decide the behavior for Rank B enemies </summary>

        // if an ally has low HP, the enemy will heal it
        if (ai.LowestHPEnemy != null)
        {
            if (ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
            {
                return EnemyBehaviour.Heal;
            }
        }

        // if the enemy team has more than 60% HP and player team has less than 60% HP => the enemy will buff an ally
        if (ai.TotalCurrentHP_Enemy > (ai.TotalMaxHP_Enemy * 0.6f) &&
            ai.TotalCurrentHP_Character <= ai.TotalMaxHP_Character * 0.6f)
        {
            if (ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostBoost)))
            {
                return EnemyBehaviour.Buff;
            }
        }

        return EnemyBehaviour.Targeted;
    }

    public EnemyBehaviour DecideA(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        ///<summary> Decide the behavior for Rank A enemies </summary>

        //compare weigth of the attacks/action to determine which one should be dealt with first 
        int scoreTargeted = 0;
        int scoreHeal = 10;
        int scoreBuff = 0;
        int scoreProtection = 0;

        // Ratios for decision making
        float enemyHPRatio = (float)ai.TotalCurrentHP_Enemy / ai.TotalMaxHP_Enemy;
        float playerHPRatio = (float)ai.TotalCurrentHP_Character / ai.TotalMaxHP_Character;
        float playerMPRatio = (float)ai.TotalCurrentMP_Character / ai.TotalMP_Character;
        float mpRatio = (float)enemy.CurrentMP / enemy.MaxMP;

        // Prioritize healing if an ally has low HP
        if (ai.LowestHPEnemy != null)
        {
            scoreHeal += 20;

            // if this enemy has HP below 25% => prioritize healing even more
            Enemy low = ai.LowestHPEnemy.GetComponent<Enemy>();
            if ((low.CurrentHP / low.MaxHP) < 0.25f)
            {
                scoreHeal += 50;
            }

        }

        // if the enemy team has more than 60% HP => encourage buffing
        if (ai.TotalCurrentHP_Enemy > ai.TotalMaxHP_Enemy * 0.6f)
        {
            scoreBuff += 30;
        }

        // if the player team has less than 50% HP => encourage buffing
        if (ai.TotalCurrentHP_Character <= ai.TotalMaxHP_Character * 0.5f)
        {
            scoreBuff += 30;
        }

        // Protection
        if (enemyHPRatio <= 0.5f) // enemy has low HP => encourage protection
        {
            scoreProtection += 40;
        }

        // -- MP --
        // If the enemy has low MP => avoid big costs
        if (mpRatio < 0.3f)
        {
            scoreTargeted += 20;
            scoreBuff -= 20;
            scoreProtection -= 20;
        }

        // If player team has low MP => encourage buffing to take advantage of it
        if (playerMPRatio < 0.35f)
        {
            if (enemyList.Count == 1)
            {
                scoreBuff += 20;
            }
        }

        // Memory of the last action : to avoid repetition and encourage variety in the enemy's behavior
        switch (ai.LastAction)
        {
            case 4: scoreHeal -= 40; break;
            case 5: 
                scoreBuff -= 40;

                // if the last action was a buff and there is only one enemy alive, encourage a targeted attack to finish the fight
                if (enemyList.Count == 1)
                {
                    scoreTargeted += 40;
                }

                break;
            case 6: 
                scoreProtection -= 40; 
                scoreHeal += 20;
                break;
        }

        // Light random
        scoreTargeted += Random.Range(-5, 6);
        scoreHeal += Random.Range(-5, 6);
        scoreBuff += Random.Range(-5, 6);
        scoreProtection += Random.Range(-5, 6);

        // Impossible actions
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
        {
            scoreHeal = int.MinValue;
        }
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostBoost)))
        {
            scoreBuff = int.MinValue;
        }
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostProtection)))
        {
            scoreProtection = int.MinValue;
        }
        if (ai.IsProtected || ai.IsShielded)
        {
            if (ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)) || ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostProtection)) || ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostBoost)))
            {
                scoreTargeted = int.MinValue;
            }
        }

        // Check if it's ok
        Debug.Log("ScoreTargeted = " + scoreTargeted);
        Debug.Log("ScoreHeal = " + scoreHeal);
        Debug.Log("ScoreBuff = " + scoreBuff);
        Debug.Log("ScoreProtection = " + scoreProtection);

        // Decision
        if (scoreHeal >= scoreTargeted && scoreHeal >= scoreBuff && scoreHeal >= scoreProtection)
        {
            return EnemyBehaviour.Heal;
        }
        else if (scoreBuff >= scoreTargeted && scoreBuff >= scoreHeal && scoreBuff >= scoreProtection)
        {
            return EnemyBehaviour.Buff;
        }
        else if (scoreProtection >= scoreTargeted && scoreProtection >= scoreHeal && scoreProtection >= scoreBuff)
        {
            return EnemyBehaviour.Defensive;
        }
        return EnemyBehaviour.Targeted;

    }

    public EnemyBehaviour DecideS(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        ///<summary> Decide the behavior for Rank S enemies </summary>

        //DPS Boss
        //compare weigth of the two attacks to determine which one should be dealt with first 
        int scoreTargeted = 10;
        int scoreAOE = 0;
        int scoreSpecial = 0;

        float mpRatio;

        // Phase 2 activation
        if (enemy.CurrentHP <= enemy.MaxHP * 0.6f && !enemy.IsPhase2)
        {
            enemy.IsPhase2 = true;
            enemy.BuffAttack = 1.5f; // the boss becomes stronger in phase 2

            scoreSpecial += 50; // The special attack priority increases when phase 2 is activated
            Debug.Log("Le boss passe en phase 2.");
        }

        if (ai.LowestHPCharacter != null)
        {
            scoreTargeted += 40;

        }

        // If there are at least 2 characters alive, the boss will be more likely to use its AOE attack
        if (playerList.Count >= 2)
        {
            scoreAOE += 10;

            // If the player team has high HP
            if (ai.TotalCurrentHP_Character > (ai.TotalMaxHP_Character / 2))
            {
                scoreAOE += 20;
            }
        }

        // Weakness
        if (ai.CountWeakness == 2)
        {
            scoreAOE += 20;
        }
        else if (ai.CountWeakness == 1)
        {
            scoreTargeted += 20;
        }

        // If the boss is buffed, target the most vulnerable character 
        if (enemy.IsBuffed)
        {
            scoreTargeted += 20;
        }

        // -- Phase 2 --
        if (enemy.IsPhase2)
        {
            // if the special attack is available, it will be more likely to be used
            if (ai.SpecialAtkCooldown == 0)
            {
                scoreSpecial += 100;
            }

            // if the enemy used the special attack in the last turn, use combo to target the most vulnerable character
            if (ai.LastAction == 3)
            {
                scoreTargeted += 25;
            }

            for (int i = 0; i < playerList.Count; i++)
            {
                Character c = playerList[i].GetComponent<Character>();

                // If the character is likely to deal a lot of damage on the next turn, they become an ideal target for the boss
                float potentialDamage = Mathf.Max(c.getDamageMultiplier() * c.getBaseAtk(), c.getStrengthenedMultiplier() * c.getBaseAtk());

                if (potentialDamage >= enemy.CurrentHP * 0.3f)
                {
                    ai.PerfectTarget = playerList[i];
                    scoreTargeted += 20;
                }
            }
        }

        // if the enemy used AOE in the last turn, it will be less likely to use it again immediately
        if (ai.LastAction == 2)
        {
            scoreAOE -= 20 * ai.RepeatCountAOE;
        }

        // -- check MP --
        mpRatio = (float)enemy.CurrentMP / enemy.MaxMP;
        // low MP => avoid big costs
        if (mpRatio < 0.3f)
        {
            scoreAOE -= 30;
            scoreSpecial -= 50;
        }

        // high MP => encourage big skills
        if (mpRatio > 0.7f)
        {
            scoreAOE += 20;
        }

        // Light random
        scoreTargeted += Random.Range(-10, 11);
        scoreAOE += Random.Range(-10, 11);

        // Impossible actions
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
        {
            scoreAOE = int.MinValue;
        }
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostSpecial)))
        {
            scoreSpecial = int.MinValue;
        }

        /* Verification
		Debug.Log("ScoreTargeted = " + scoreTargeted);
		Debug.Log("ScoreAOE = " + scoreAOE);
		Debug.Log("ScoreSpecial = " + scoreSpecial);*/


        // Priotize the attack with the highest score depending on the context
        if (scoreSpecial >= scoreTargeted && scoreSpecial >= scoreAOE)
        {
            return EnemyBehaviour.Special;
        }
        else if (scoreAOE > scoreTargeted)
        {
            return EnemyBehaviour.AOE_pressure;
        }
        return EnemyBehaviour.Targeted;
    

        /* bonus Support Boss
            if (enemy.CurrentHP <= enemy.MaxHP * 0.6f && !enemy.IsPhase2)
            {
                enemy.IsPhase2 = true;
                enemy.BuffAttack = 1.5f;

                scoreSpecial += 50;
                Debug.Log("Le boss passe en phase 2.");
            }
            // si y'a un ennemi qui a ses PV inferieur a 40% => prioriser le soin
            if (lowestHPEnemy != null)
            {
                scoreHeal += 50;
            }
            // si l'equipe ennemi a ses PV plus haut que 60%
            if (totalCurrentHP_Enemy > totalMaxHP_Enemy * 0.6f)
            {
                scoreBuff += 20;
            }
            // si player team a ses PV plus bas que 50%
            if (totalCurrentHP_Character <= totalMaxHP_Character * 0.5f)
            {
                scoreBuff += 30;
            }
            // si les 2 personnages du joueur sont en vie => pression AOE
            if (playerList.Count >= 2)
            {
                scoreAOE += 5;
            }
            // If the player team has high HP
            if (totalCurrentHP_Character > (totalMaxHP_Character / 2))
            {
                scoreAOE += 5;
            }


            if (totalCurrentHP_Character > (totalMaxHP_Character * 0.6f))
            {
                scoreProtection += 20;

                if (enemy.CurrentMP > enemy.MaxMP * 0.6f)
                {
                    scoreProtection += 20;
                }
            }

            if (enemy.CurrentMP < enemy.MaxMP * 0.2f)
            {
                scoreTargeted += 20;
            }

            if (enemyList.Count == 1)
            {
                scoreTargeted += 30;
                if (lastAction == 5)
                {
                    scoreTargeted += 20;
                }
                else
                {
                    scoreBuff += 20;
                }
            }

            if (enemy.IsPhase2)
            {
                if (specialAtkCooldown == 0)
                {
                    scoreSpecial += 50;
                }

                if (lastAction == 3) // Special
                {
                    scoreHeal -= 30;
                }

            }

            // Random leger
            scoreAOE += Random.Range(-10, 11);
            scoreHeal += Random.Range(-10, 11);
            scoreBuff += Random.Range(-10, 11);
            scoreProtection += Random.Range(-10, 11);

            if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
            {
                scoreAOE = int.MinValue;
            }
            if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
            {
                scoreHeal = int.MinValue;
            }
            if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostBoost)))
            {
                scoreBuff = int.MinValue;
            }
            if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostProtection)))
            {
                scoreProtection = int.MinValue;
            }

            Debug.Log("ScoreTargeted = " + scoreTargeted);
            Debug.Log("ScoreAOE = " + scoreAOE);
            Debug.Log("ScoreHeal = " + scoreHeal);
            Debug.Log("ScoreBuff = " + scoreBuff);
            Debug.Log("ScoreProtection = " + scoreProtection);

            if (scoreHeal >= scoreTargeted && scoreHeal >= scoreAOE && scoreHeal >= scoreBuff && scoreHeal >= scoreProtection)
            {
                return EnemyBehaviour.Heal;
            }
            else if (scoreBuff >= scoreTargeted && scoreBuff >= scoreAOE && scoreBuff >= scoreHeal && scoreBuff >= scoreProtection)
            {
                return EnemyBehaviour.Buff;
            }
            else if (scoreProtection >= scoreTargeted && scoreProtection >= scoreAOE && scoreProtection >= scoreHeal && scoreProtection >= scoreBuff)
            {
                return EnemyBehaviour.Defensive;
            }
            else if (scoreAOE > scoreTargeted)
            {
                return EnemyBehaviour.AOE_pressure;
            }
            */
    }
    private GameObject GetPriorityLowHP(EnemyAI ai, List<GameObject> enemyList, float tolerance)
    {
        /// <summary> Returns the GameObject with the lowest HP, giving priority to DPS and Neutral types if their HP are close to the lowest one.</summary>

        GameObject lowestObj = ai.GetLowestHP(enemyList);
        Enemy lowestEnemy = lowestObj.GetComponent<Enemy>();

        float lowestRatio = (float)lowestEnemy.CurrentHP / lowestEnemy.MaxHP;

        GameObject bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (var obj in enemyList)
        {
            Enemy e = obj.GetComponent<Enemy>();
            float hpRatio = (float)e.CurrentHP / e.MaxHP;

            // base score is the HP ratio (lower is more urgent)
            float score = hpRatio;

            // add a priority bonus for certain types of enemies (DPS and Neutral are more important to heal)
            float priority = 0f;
            if (e.Type == EnemyType.DPS)
                priority = -0.1f; // very important
            else if (e.Type == EnemyType.Neutral)
                priority = -0.04f;

            // apply the bonus "priority" only if HP are close
            if (Mathf.Abs(hpRatio - lowestRatio) < tolerance)
            {
                score += priority;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = obj;
            }
        }

        return bestTarget;
    }
    
}