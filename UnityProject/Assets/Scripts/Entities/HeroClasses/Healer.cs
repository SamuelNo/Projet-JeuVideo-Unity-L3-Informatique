using UnityEngine;
using System;

public class Healer : Character
{
    // ---------- Initialisation ---------- //
    void Awake(){
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.All;
    }
    void Reset(){
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.All;
    }

    // ---------- Methods ---------- //
    
    override public void onClick()
    {
        BattleUIController controller = FindFirstObjectByType<BattleUIController>();
    if (controller != null)
    {
        controller.HandleSelection(this.gameObject);
    }
    else
    {
        Debug.LogError("BattleUIController not found in the scene.");
    }
    } 

    override public void receiveDamage(int attack, AttackType atkType, bool elemental){
        ///<param> attack : the amount of damage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Calculates the damage to be received depending on character's weaknesses and applies it </summary>

        // takes original amount of attack damage
        int damageReceived = attack;
        
        // character is weak to all attack types (except neutral ?), so damage worsens
        if (atkType != AttackType.Neutral){
            damageReceived = (int) ((float)damageReceived * weakenedMultiplier); 
        }

        // if the attack is elemental, damage worsens
        if (elemental){
            damageReceived = (int) ((float)damageReceived * weakenedMultiplier);
        }

        // applies damage (and tries dodging)
        receiveDamage(damageReceived);
    }

    override public void skillLvl1(GameObject target){
        ///<param> target : the target of the skill </param> 
        ///<summary> Restores a certain amount of HP to the target. For now, 10% of the target's maxHP will be restored </summary>
        
        // uses MP (10MP)
        useMP(10);

        // calcultates 10% of target's maxHP
        int healAmount = target.GetComponent<Character>().getMaxHP(); 
        healAmount = (int) ((float) healAmount * .1f);

        // heals target
        target.GetComponent<Character>().receiveHeal(healAmount);
    }

    override public void skillLvl2(GameObject [] target){} // to do (needs information from class Combat)

    override public void skillLvl3(GameObject [] target){
        ///<param> target : the target of the skill </param> 
        ///<summary> Restores 50% of HP to the target and themselves </summary>
        
        // checks that only 1 taget is chosen (only 2 characters can fight in a battle, so 2 characters to heal : the ally and themselves)
        if (target.Length != 1){
            Debug.LogException(new Exception("[Classe Healer] Only the ally should be entered as a parameter"));
            return;
        }

        // uses MP (30MP)
        useMP(30);

        // heals themselves
        int healAmount = (int) ((float) this.maxHP * .5f);
        receiveHeal(healAmount);

        // if there's an ally 
        if (target.Length == 1){
            // calcultates 50% of ally's maxHP
            healAmount = target[0].GetComponent<Character>().getMaxHP(); 
            healAmount = (int) ((float) healAmount * .5f);

            // heals target
            target[0].GetComponent<Character>().receiveHeal(healAmount);
        }
    }
}

/* 
Niveau 2 - Encourager :
Augmente les dégâts d'un allié pendant deux tours
*/