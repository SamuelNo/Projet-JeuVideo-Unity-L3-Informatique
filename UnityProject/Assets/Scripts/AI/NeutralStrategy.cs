using UnityEngine;
using System.Collections.Generic;

public class NeutralStrategy : IEnemyStrategy
{
    public EnemyBehaviour Decide(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        /// <summary>Decides the behavior for a Neutral type enemy based on its rank and the current context.</summary>
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
        /// <summary>Decides the target for a Neutral type enemy based on the current context.</summary>
        
        GameObject target = null;
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
        else if (ai.Behaviour == EnemyBehaviour.Heal)
        {
            target = ai.LowestHPEnemy;
        }
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
        else if (ai.Behaviour == EnemyBehaviour.Defensive)
        {
            target = ai.LowestHPEnemy;

        }
        return target;
    }


    // -- Decide the behavior of the enemy depending on its rank and the situation of the fight --
    public EnemyBehaviour DecideC(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        ///<summary> Decide the behavior for Rank C enemies </summary>

        // if there is a low HP character, target it
        if (ai.LowestHPCharacter != null)
        {
            return EnemyBehaviour.LowHPTarget;
        }
        // if there is a low HP ally, heal it
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

        // if there is a low HP character, target it
        if (ai.LowestHPCharacter != null)
        {
            return EnemyBehaviour.LowHPTarget;
        }
        // if there is a low HP ally, heal it
        if (ai.LowestHPEnemy != null)
        {
            if (ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
            {
                return EnemyBehaviour.Heal;
            }
        }
        // if there are at least 2 characters alive and player team has more than 60% of its HP, it will use its AOE attack
        if (playerList.Count >= 2 && ai.TotalCurrentHP_Character >= (int)(ai.TotalMaxHP_Character * 0.6f))
        {
            if (ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
            {
                return EnemyBehaviour.AOE_pressure;
            }
        }
        return EnemyBehaviour.Targeted;
    }

    public EnemyBehaviour DecideA(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        ///<summary> Decide the behavior for Rank A enemies </summary>

        //compare weigth of the attacks/action to determine which one should be dealt with first 
        int scoreTargeted = 0;
        int scoreAOE = 0;
        int scoreHeal = 0;
        int scoreProtection = 0;

        // Ratios for decision making
        float enemyHPRatio = (float)ai.TotalCurrentHP_Enemy / ai.TotalMaxHP_Enemy;
        float playerHPRatio = (float)ai.TotalCurrentHP_Character / ai.TotalMaxHP_Character;
        float mpRatio = (float)enemy.CurrentMP / enemy.MaxMP;

        if (playerHPRatio < 0.4f)
        {
            // agressive mode
            scoreTargeted += 40;
            scoreAOE += 20;
        }
        else if (enemyHPRatio < 0.5f)
        {
            // survival mode
            scoreHeal += 30;
            scoreProtection += 40;
        }
        else
        {
            // balanced mode
            scoreAOE += 20;
        }

        // If there is a low HP character, it becomes a priority target
        if (ai.LowestHPCharacter != null)
        {
            scoreTargeted += 40;
        }

        // If there is a low HP ally, it becomes a priority to heal
        if (ai.LowestHPEnemy != null)
        {
            scoreHeal += 20;
            Enemy low = ai.LowestHPEnemy.GetComponent<Enemy>();

            // If the lowest HP ally is under 30% HP, healing becomes a priority
            if ((low.CurrentHP / low.MaxHP) < 0.3f)
            {
                scoreHeal += 30;
            }
        }

        // If there are at least 2 characters alive, the enemy will be more likely to use its AOE attack
        if (playerList.Count >= 2)
        {
            scoreAOE += 10;

            // If the player team has high HP
            if (playerHPRatio > 0.5f)
            {
                scoreAOE += 20;
            }
        }

        // Protection
        if (playerHPRatio > 0.6f) // player team has high hp => player team may attack 
        {
            scoreProtection += 40;

            // If the lowest HP ally is under 50% HP, protection becomes a priority
            if (ai.LowestHPEnemy != null)
            {
                float hpRatio = (float)ai.LowestHPEnemy.GetComponent<Enemy>().CurrentHP /
                                ai.LowestHPEnemy.GetComponent<Enemy>().MaxHP;

                if (hpRatio < 0.5f)
                    scoreProtection += 30;
            }
        }

        // If the enemy is buffed => priority to targeted attack to take advantage of the buff
        if (enemy.IsBuffed)
        {
            scoreTargeted += 20;
        }

        // if the enemy used heal in the last turn, it will be less likely to use it again immediately
        if (ai.LastAction == 4)
        {
            scoreHeal -= 40;
        }

        // -- check MP --
        // low MP => avoid big costs
        if (mpRatio < 0.3f)
        {
            scoreAOE -= 20;
        }

        // high MP => encourage big skills
        if (mpRatio > 0.7f)
        {
            scoreAOE += 10;
            scoreHeal += 10;
            scoreProtection += 10;
        }

        // light random
        scoreTargeted += Random.Range(-5, 6);
        scoreAOE += Random.Range(-5, 6);
        scoreHeal += Random.Range(-5, 6);
        scoreProtection += Random.Range(-5, 6);


        // Impossible actions
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
        {
            scoreAOE = int.MinValue;
        }
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
        {
            scoreHeal = int.MinValue;
        }
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostProtection)))
        {
            scoreProtection = int.MinValue;
        }

        /*Debug.Log("ScoreTargeted = " + scoreTargeted);
        Debug.Log("ScoreAOE = " + scoreAOE);
        Debug.Log("ScoreHeal = " + scoreHeal);
        Debug.Log("ScoreProtection = " + scoreProtection)*/

        if (scoreHeal >= scoreTargeted && scoreHeal >= scoreAOE && scoreHeal >= scoreProtection)
        {
            return EnemyBehaviour.Heal;
        }
        else if (scoreProtection >= scoreTargeted && scoreProtection >= scoreAOE && scoreProtection >= scoreHeal)
        {
            return EnemyBehaviour.Defensive;
        }
        else if (scoreAOE > scoreTargeted)
        {
            return EnemyBehaviour.AOE_pressure;
        }
        return EnemyBehaviour.Targeted;

    }

    public EnemyBehaviour DecideS(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        ///<summary> Decide the behavior for Rank S enemies </summary>
        // DPS boss
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
    }
}