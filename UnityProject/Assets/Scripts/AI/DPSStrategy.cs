using UnityEngine;
using System.Collections.Generic;

public class DPSStrategy : IEnemyStrategy
{
	public EnemyBehaviour Decide(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
	{
        /// <summary>Decides the behavior for a DPS type enemy based on its rank and the current context.</summary>
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
        /// <summary>Decides the target for a DPS type enemy based on the current context.</summary>
        GameObject target = null;

        // if a perfect target has been identified
        if (ai.PerfectTarget != null)
		{
			return ai.PerfectTarget;
		}
        // if a low HP target has been identified
        else if (ai.LowestHPCharacter != null)
		{
			return ai.LowestHPCharacter;
		}
		else if (ai.IsProtected)
		{
			// if the enemy is protected, it will attack the protector
			for (int i = 0; i < playerList.Count; i++)
			{
				Character c = playerList[i].GetComponent<Character>();
                if (c.TryGetComponent<Protector>(out var charac))
                {
                    return playerList[i];
                }
            }
        }
		// otherwise, the enemy will attack the character with the lowest HP, and if it's a boss, it will also take into account the weaknesses of the characters
		else
		{
			int bestTarget = -1;
			int currentScore = 0;
			int value_lowestHP = int.MaxValue;

			for (int i = 0; i < playerList.Count; i++)
			{

				Character c = playerList[i].GetComponent<Character>();
				// the enemy want to attack the character with the lowest HP
				if (c.getCurrentHP() < value_lowestHP)
				{
					currentScore += 20;
					value_lowestHP = c.getCurrentHP();
				}

				// Boss only
				if (enemy.Rank == Rank.S)
				{
					// if the enemy is weak to the boss => more chance to be attacked
					if (string.Equals(c.getWeakness().ToString(), enemy.AttackTypeUsed.ToString()))
					{
						currentScore += 20;
					}
				}

				// compare to choose the best target
				if (bestTarget < currentScore)
				{
					bestTarget = currentScore;
					target = playerList[i];
				}
				currentScore = 0;
			}
		}
		return target;
	}


	// -- Decide the behavior of the enemy depending on its rank and the situation of the fight --
	public EnemyBehaviour DecideC(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
	{
        ///<summary> Decide the behavior for Rank C enemies </summary>
        if (ai.LowestHPCharacter != null)
		{
			return EnemyBehaviour.LowHPTarget;
		}
		return EnemyBehaviour.Targeted;
	}

	public EnemyBehaviour DecideB(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
	{
		///<summary> Decide the behavior for Rank B enemies </summary>
		if (ai.LowestHPCharacter != null)
		{
			return EnemyBehaviour.LowHPTarget;
		}
        // if there are at least 2 characters alive and the enemy has more than 60% of its HP, it will use its AOE attack
        if (playerList.Count >= 2 && ai.TotalCurrentHP_Character >= (int)(ai.TotalMaxHP_Character * 0.6f) && ai.IsShielded == false)
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
		
		//compare weigth of the two attacks to determine which one should be dealt with first 
		int scoreTargeted = 10;
		int scoreAOE = 0;

		float mpRatio;

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

		if (playerList.Count == 1)
		{
			scoreTargeted += 30;
			scoreAOE -= 30;
        }

        // -- check MP --
        mpRatio = (float)enemy.CurrentMP / enemy.MaxMP;
        // low MP => avoid big costs
        if (mpRatio < 0.3f)
        {
            scoreAOE -= 20;
        }
        // high MP => encourage big skills
        if (mpRatio > 0.7f)
        {
            scoreAOE += 10;
        }

        // Light random
        scoreTargeted += Random.Range(-10, 11);
		scoreAOE += Random.Range(-10, 11);

        // Impossible action
        if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
		{
			scoreAOE = int.MinValue;
		}
		if (ai.IsShielded || ai.IsProtected)
		{
			scoreAOE = int.MinValue;
        }

        /* Verification
        Debug.Log("ScoreTargeted = " + scoreTargeted);
        Debug.Log("ScoreAOE = " + scoreAOE); */

        // Priotize the attack with the highest score depending on the context
        if (scoreAOE > scoreTargeted)
		{
			return EnemyBehaviour.AOE_pressure;
		}
		return EnemyBehaviour.Targeted;

	}

	public EnemyBehaviour DecideS(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
	{
		///<summary> Decide the behavior for Rank S enemies </summary>

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
        scoreTargeted += Random.Range(-5, 6);
		scoreAOE += Random.Range(-5, 6);

		// Impossible actions
		if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
		{
			scoreAOE = int.MinValue;
		}
		if (!ai.CanUseMP(enemy, enemy.GetMPCost(enemy.CostSpecial)))
		{
			scoreSpecial = int.MinValue;
		}
		if (ai.IsShielded || ai.IsProtected)
		{
			scoreAOE = int.MinValue;
			scoreSpecial -= 50;
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