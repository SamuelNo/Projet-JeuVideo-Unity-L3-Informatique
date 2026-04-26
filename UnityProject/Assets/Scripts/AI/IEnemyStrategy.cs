using UnityEngine;
using System.Collections.Generic;

public interface IEnemyStrategy
{
	public EnemyBehaviour Decide(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList);
	public GameObject DecideTarget(EnemyAI ai, Enemy enemy, List<GameObject> playerList, List<GameObject> enemyList);
}