using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI
{
    protected int countWeakness;
    protected int totalMaxHP_Character;
    protected int totalCurrentHP_Character;
    protected int totalMaxHP_Enemy;
    protected int totalCurrentHP_Enemy;
    protected GameObject lowestHPCharacter;
    protected GameObject lowestHPEnemy;

    protected EnemyBehaviour behaviour;

    protected int lastAction;
    protected int repeatCountAOE = 0;
    protected int specialAtkCooldown = 0;
    protected GameObject perfectTarget;

    public int CountWeakness { get => countWeakness; set => countWeakness = value; }
    public EnemyBehaviour Behaviour { get => behaviour; set => behaviour = value; }


    public int DecideAction(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        Debug.Log("Enemy: " + enemy.name);

        int action = 1;

        // Analyse du contexte
        AnalyseContext(enemy, playerList, enemyList);

        // Choix du comportement a adopter
        Comportement(enemy, playerList, enemyList);

        ReduceCooldown();

        // Choix de l'action 
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
        if (action == 2)
        {
            repeatCountAOE++;
        }
        else
        {
            repeatCountAOE = 0;
        }
        lastAction = action;
        return action;
    }

    public void AnalyseContext(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        // Initialisation avant l'analyse
        totalCurrentHP_Character = 0;
        totalMaxHP_Character = 0;
        totalCurrentHP_Enemy = 0;
        totalMaxHP_Enemy = 0;
        countWeakness = 0;
        lowestHPCharacter = null;
        perfectTarget = null;

        lowestHPEnemy = null;


        // Analyse player team
        for (int i = 0; i < playerList.Count; i++)
        {
            Character c = playerList[i].GetComponent<Character>();

            // nb perso qui ont une faiblesse face aux types d'atk de l'ennemi
            if (string.Equals(c.getWeakness().ToString(), enemy.AttackTypeUsed.ToString()))
            {
                countWeakness++;
            }

            totalCurrentHP_Character += c.getCurrentHP();
            totalMaxHP_Character += c.getMaxHP();

            if (c.getCurrentHP() < c.getMaxHP() * 0.3f)
            {
                if (lowestHPCharacter == null || c.getCurrentHP() < lowestHPCharacter.GetComponent<Character>().getCurrentHP())
                {
                    lowestHPCharacter = playerList[i];
                }
            }
        }

        // Analyse enemies 
        for (int j = 0; j < enemyList.Count; j++)
        {
            Enemy currentEnemy = enemyList[j].GetComponent<Enemy>();

            totalCurrentHP_Enemy += currentEnemy.CurrentHP;
            totalMaxHP_Enemy += currentEnemy.MaxHP;

            if (currentEnemy.CurrentHP < currentEnemy.MaxHP * 0.4f)
            {
                lowestHPEnemy = enemyList[j];
            }
        }
    }

    public void Comportement(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        switch (enemy.Type)
        {
            case EnemyType.DPS:
                behaviour = DPSBehaviour(enemy, playerList);
                break;

            case EnemyType.Neutral:
                behaviour = NeutralBehaviour(enemy, playerList, enemyList);
                break;

            case EnemyType.Support:
                behaviour = SupportBehaviour(enemy, playerList, enemyList);
                break;

            default:
                behaviour = EnemyBehaviour.Targeted;
                break;
        }
    }

    private EnemyBehaviour DPSBehaviour(Enemy enemy, List<GameObject> playerList)
    {
        //compare weigth of the two attacks to determine which one should be dealt with first 
        int scoreTargeted = 10;
        int scoreAOE = 0;
        int scoreSpecial = 0;

        float mpRatio;

        switch (enemy.Rank)
        {
            case Rank.C:
                if (lowestHPCharacter != null)
                {
                    return EnemyBehaviour.LowHPTarget;
                }
                return EnemyBehaviour.Targeted;

            case Rank.B:
                if (lowestHPCharacter != null)
                {
                    return EnemyBehaviour.LowHPTarget;
                }
                if (playerList.Count >= 2 && totalCurrentHP_Character >= (int)(totalMaxHP_Character * 0.6f))
                {
                    if (CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
                    {
                        return EnemyBehaviour.AOE_pressure;
                    }
                }
                return EnemyBehaviour.Targeted;

            case Rank.A:
                if (lowestHPCharacter != null)
                {
                    scoreTargeted += 40;
                }
                if (playerList.Count >= 2)
                {
                    scoreAOE += 10;

                    // If the player team has high HP
                    if (totalCurrentHP_Character > (totalMaxHP_Character / 2))
                    {
                        scoreAOE += 20;
                    }
                }
                // Weakness
                if (countWeakness == 2)
                {
                    scoreAOE += 20;
                }
                else if (countWeakness == 1)
                {
                    scoreTargeted += 20;
                }

                // Random leger
                scoreTargeted += Random.Range(-10, 11);
                scoreAOE += Random.Range(-10, 11);

                // param MP
                mpRatio = (float)enemy.CurrentMP / enemy.MaxMP;
                // si peu de MP => eviter les gros couts
                if (mpRatio < 0.3f)
                {
                    scoreAOE -= 20;
                }

                // si beaucoup de MP => encourager gros skills
                if (mpRatio > 0.7f)
                {
                    scoreAOE += 10;
                }
                if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
                {
                    scoreAOE = int.MinValue;
                }

                /* Verification
                Debug.Log("ScoreTargeted = " + scoreTargeted);
                Debug.Log("ScoreAOE = " + scoreAOE);*/

                if (scoreAOE > scoreTargeted)
                {
                    return EnemyBehaviour.AOE_pressure;
                }
                return EnemyBehaviour.Targeted;

            case Rank.S:
                if (enemy.CurrentHP <= enemy.MaxHP * 0.6f && !enemy.IsPhase2)
                {
                    enemy.IsPhase2 = true;
                    enemy.BuffAttack = 1.5f;

                    scoreSpecial += 50;
                    Debug.Log("Le boss passe en phase 2.");
                }

                if (lowestHPCharacter != null)
                {
                    scoreTargeted += 40;

                }
                if (playerList.Count >= 2)
                {
                    scoreAOE += 10;

                    // If the player team has high HP
                    if (totalCurrentHP_Character > (totalMaxHP_Character / 2))
                    {
                        scoreAOE += 20;
                    }
                }
                // Weakness
                if (countWeakness == 2)
                {
                    scoreAOE += 20;
                }
                else if (countWeakness == 1)
                {
                    scoreTargeted += 20;
                }

                if (enemy.IsBuffed)
                {
                    scoreTargeted += 20;
                }

                if (enemy.IsPhase2)
                {
                    if (specialAtkCooldown == 0)
                    {
                        scoreSpecial += 100;
                    }

                    // si le special a ete lance juste avant, choisir l'attaque cible pour faire plus de degats
                    if (lastAction == 3)
                    {
                        scoreTargeted += 25;
                    }
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        Character c = playerList[i].GetComponent<Character>();
                        /*if (lowestHP != null)
                        {
                            if (c.getDamageMultiplier() * c.getBaseAtk() >= 0.15f * enemy.CurrentHP ||
                                c.getStrengthenedMultiplier() * c.getBaseAtk() >= 0.15f * enemy.CurrentHP)
                            {
                                perfectTarget = playerList[i];
                                scoreTargeted += 40;
                            }
                        }*/
                        float potentialDamage = Mathf.Max(c.getDamageMultiplier() * c.getBaseAtk(), c.getStrengthenedMultiplier() * c.getBaseAtk());

                        if (potentialDamage >= enemy.CurrentHP * 0.3f)
                        {
                            perfectTarget = playerList[i];
                            scoreTargeted += 20;
                        }
                    }
                }

                if (lastAction == 2) //AOE
                {
                    scoreAOE -= 20 * repeatCountAOE;
                }

                // Light random (a enlever pour les tests)
                scoreTargeted += Random.Range(-10, 11);
                scoreAOE += Random.Range(-10, 11);

                // param MP
                mpRatio = (float)enemy.CurrentMP / enemy.MaxMP;
                // si peu de MP => eviter les gros couts
                if (mpRatio < 0.3f)
                {
                    scoreAOE -= 30;
                    scoreSpecial -= 50;
                }

                // si beaucoup de MP => encourager gros skills
                if (mpRatio > 0.7f)
                {
                    scoreAOE += 20;
                }

                if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
                {
                    scoreAOE = int.MinValue;
                }

                if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostSpecial)))
                {
                    scoreSpecial = int.MinValue;
                }


                /*
                Verification
                Debug.Log("ScoreTargeted = " + scoreTargeted);
                Debug.Log("ScoreAOE = " + scoreAOE);
                Debug.Log("ScoreSpecial = " + scoreSpecial);*/

                if (scoreSpecial >= scoreTargeted && scoreSpecial >= scoreAOE)
                {
                    return EnemyBehaviour.Special;
                }
                else if (scoreAOE > scoreTargeted)
                {
                    return EnemyBehaviour.AOE_pressure;
                }
                return EnemyBehaviour.Targeted;

            default:
                return EnemyBehaviour.Targeted;
        }
    }

    private EnemyBehaviour NeutralBehaviour(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        //compare weigth of the attacks/action to determine which one should be dealt with first 
        int scoreTargeted = 0;
        int scoreAOE = 0;
        int scoreSpecial = 0;
        int scoreHeal = 0;
        int scoreProtection = 0;

        // ratio mp et hp pour aider a la decision
        float enemyHPRatio = (float)totalCurrentHP_Enemy / totalMaxHP_Enemy;
        float playerHPRatio = (float)totalCurrentHP_Character / totalMaxHP_Character;
        float mpRatio = (float)enemy.CurrentMP / enemy.MaxMP;

        switch (enemy.Rank)
        {
            case Rank.C:
                if (lowestHPCharacter != null)
                {
                    return EnemyBehaviour.LowHPTarget;
                }
                if (lowestHPEnemy != null)
                {
                    if (CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
                    {
                        return EnemyBehaviour.Heal;
                    }
                }
                return EnemyBehaviour.Targeted;

            case Rank.B:
                if (lowestHPCharacter != null)
                {
                    return EnemyBehaviour.LowHPTarget;
                }
                if (lowestHPEnemy != null)
                {
                    if (CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
                    {
                        return EnemyBehaviour.Heal;
                    }
                }
                if (playerList.Count >= 2 && totalCurrentHP_Character >= (int)(totalMaxHP_Character * 0.6f))
                {
                    if (CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
                    {
                        return EnemyBehaviour.AOE_pressure;
                    }
                }
                return EnemyBehaviour.Targeted;

            case Rank.A:

                if (playerHPRatio < 0.4f)
                {
                    // mode agression
                    scoreTargeted += 40;
                    scoreAOE += 20;
                }
                else if (enemyHPRatio < 0.5f)
                {
                    // mode survie
                    scoreHeal += 30;
                    scoreProtection += 40;
                }
                else
                {
                    // mode controle
                    scoreAOE += 20;
                }

                if (lowestHPCharacter != null)
                {
                    scoreTargeted += 40;
                }
                if (lowestHPEnemy != null)
                {
                    scoreHeal += 20;
                    Enemy low = lowestHPEnemy.GetComponent<Enemy>();
                    if ((low.CurrentHP / low.MaxHP) < 0.3f)
                    {
                        scoreHeal += 30;
                    }
                }
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

                    if (lowestHPEnemy != null)
                    {
                        float hpRatio = (float)lowestHPEnemy.GetComponent<Enemy>().CurrentHP /
                                        lowestHPEnemy.GetComponent<Enemy>().MaxHP;

                        if (hpRatio < 0.5f)
                            scoreProtection += 30;
                    }
                }
                if (enemy.IsBuffed)
                {
                    scoreTargeted += 20;
                }

                if (lastAction == 4)
                {
                    scoreHeal -= 40;
                }

                // si peu de MP => eviter les gros couts
                if (mpRatio < 0.3f)
                {
                    scoreAOE -= 20;
                }

                // si beaucoup de MP => encourager gros skills
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
                if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
                {
                    scoreAOE = int.MinValue;
                }
                if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
                {
                    scoreHeal = int.MinValue;
                }
                if (!CanUseMP(enemy, enemy.GetMPCost(enemy.CostProtection)))
                {
                    scoreProtection = int.MinValue;
                }

                /*Debug.Log("ScoreTargeted = " + scoreTargeted);
                Debug.Log("ScoreAOE = " + scoreAOE);*/

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

            case Rank.S:
                // to do
                return EnemyBehaviour.Targeted;

            default:
                return EnemyBehaviour.Targeted;
        }
    }

    private EnemyBehaviour SupportBehaviour(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {
        //compare weigth of the attacks/action to determine which one should be dealt with first 
        int scoreTargeted = 0;
        int scoreAOE = 0;
        int scoreSpecial = 0;
        int scoreHeal = 10;
        int scoreBuff = 0;
        int scoreProtection = 0;

        // ratio mp et hp pour aider a la decision
        float enemyHPRatio = (float)totalCurrentHP_Enemy / totalMaxHP_Enemy;
        float playerHPRatio = (float)totalCurrentHP_Character / totalMaxHP_Character;
        float mpRatio = (float)enemy.CurrentMP / enemy.MaxMP;

        // reset des buffs/debuffs temporaires de l'ennemi (protection, buff attaque)
        // l'ennemi a une protection qui a bloque une attaque du joueur au tour precedent => la retirer pour le prochain tour
        if (enemy.IsProtected)
        {
            enemy.IsProtected = false;
        }
        // l'ennemi a un buff qui a augmente les degats d'une attaque du joueur au tour precedent => la retirer pour le prochain tour
        if (enemy.IsBuffed)
        {
            enemy.IsBuffed = false;
            enemy.BuffAttack = (enemy.IsPhase2) ? 1.5f : 1f;
        }



        switch (enemy.Rank)
        {
            case Rank.C:
                if (lowestHPEnemy != null)
                {
                    if (CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
                    {
                        return EnemyBehaviour.Heal;
                    }
                }
                return EnemyBehaviour.Targeted;

            case Rank.B:
                if (lowestHPEnemy != null)
                {
                    if (CanUseMP(enemy, enemy.GetMPCost(enemy.CostHeal)))
                    {
                        return EnemyBehaviour.Heal;
                    }
                }
                if (lowestHPEnemy.GetComponent<Enemy>().CurrentHP > lowestHPEnemy.GetComponent<Enemy>().MaxHP * 0.6f &&
                    totalCurrentHP_Character <= totalMaxHP_Character * 0.6f)
                {
                    if (CanUseMP(enemy, enemy.GetMPCost(enemy.CostBoost)))
                    {
                        return EnemyBehaviour.Heal;
                    }
                }
                if (playerList.Count == 2)
                {
                    if (CanUseMP(enemy, enemy.GetMPCost(enemy.CostAOE)))
                    {
                        return EnemyBehaviour.AOE_pressure;
                    }
                }
                return EnemyBehaviour.Targeted;

            case Rank.A:
                // si y'a un ennemi qui a ses PV inferieur a 40% => prioriser le soin
                if (lowestHPEnemy != null)
                {
                    scoreHeal += 30;

                    // si cet ennemi a ses PV inferieur a 25% => encore plus prioriser le soin
                    Enemy low = lowestHPEnemy.GetComponent<Enemy>();
                    if ((low.CurrentHP / low.MaxHP) < 0.25f)
                    {
                        scoreHeal += 50;
                    }

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

                // Protection
                if (playerHPRatio > 0.6f) // player team has high hp => player team may attack 
                {
                    scoreProtection += 30;

                    if (lowestHPEnemy != null)
                    {
                        float hpRatio = (float)lowestHPEnemy.GetComponent<Enemy>().CurrentHP /
                                        lowestHPEnemy.GetComponent<Enemy>().MaxHP;

                        if (hpRatio < 0.5f)
                            scoreProtection += 30;
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

                if (mpRatio < 0.3f)
                {
                    scoreBuff -= 20;
                    scoreProtection -= 20;
                }

                if (lastAction == 4)
                {
                    scoreHeal -= 20;
                }

                // Light random
                scoreTargeted += Random.Range(-5, 6);
                scoreHeal += Random.Range(-10, 11);
                scoreBuff += Random.Range(-10, 11);
                scoreProtection += Random.Range(-10, 11);

                // Impossible actions
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

                // Check if it's ok
                /*Debug.Log("ScoreTargeted = " + scoreTargeted);
                Debug.Log("ScoreHeal = " + scoreHeal);
                Debug.Log("ScoreBuff = " + scoreBuff);
                Debug.Log("ScoreProtection = " + scoreProtection);*/

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

            case Rank.S:
                // to do
                /* bonus
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
                return EnemyBehaviour.Targeted;

            default:
                return EnemyBehaviour.Targeted;
        }
    }


    public GameObject DecideTarget(Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList)
    {

        GameObject target = null;
        if (enemy.Type == EnemyType.Support)
        {
            if (behaviour == EnemyBehaviour.Targeted)
            {
                if (lowestHPCharacter != null)
                {
                    target = lowestHPCharacter;
                }
                else
                {
                    target = GetLowestHP(playerList);
                }
            }
            if (behaviour == EnemyBehaviour.Heal)
            {
                target = lowestHPEnemy;
            }
            else if (behaviour == EnemyBehaviour.Buff)
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
        }
        else if (behaviour == EnemyBehaviour.Defensive)
        {
            target = lowestHPEnemy;

        }
        else if (enemy.Type == EnemyType.DPS)
        {
            if (enemy.Rank == Rank.C && lowestHPCharacter == null)
            {
                // Random character (50/50)
                if (playerList.Count == 2)
                {
                    int value = Random.Range(0, 2);
                    target = playerList[value];
                }
                else
                {
                    target = playerList[0];
                }
            }
            else
            {
                if (perfectTarget != null)
                {
                    return perfectTarget;

                }
                else if (lowestHPCharacter != null)
                {
                    return lowestHPCharacter;
                }
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
            }
        }

        return target;

    }

    private void ReduceCooldown()
    {
        if (specialAtkCooldown > 0)
        {
            specialAtkCooldown--;
        }
    }

    private bool CanUseMP(Enemy enemy, int cost)
    {
        return enemy.CurrentMP >= cost;
    }

    public GameObject GetLowestHP(List<GameObject> team)
    {
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