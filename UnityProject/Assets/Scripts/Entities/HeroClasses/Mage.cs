using UnityEngine;
using System;

public class Mage : Character
{
    // ---------- Initialisation ---------- //
    protected override void Awake(){
        base.Awake();
        this.attackType = AttackType.Ranged;
        this.weakness = Weakness.Melee;
        this.gameObject.name = "Mage";
    }
    void Reset(){
        this.attackType = AttackType.Ranged;
        this.weakness = Weakness.Melee;
        this.gameObject.name = "Mage";
    }

    // ---------- Methods ---------- //

    override public void receiveDamage(int attack, AttackType atkType, bool elemental){
        ///<param> attack : the amount of damage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Calculates the damage to be received depending on character's weaknesses and applies it </summary>

        // takes original amount of attack damage
        int damageReceived = attack;

        // if the attack type is this character's weakness (ranged), damage worsens 
        if (string.Equals(this.weakness.ToString(), atkType.ToString())){ 
            damageReceived = (int) ((float)damageReceived * weakenedMultiplier); 
        }

        // applies damage (and tries dodging)
        receiveDamage(damageReceived);
    }

    public new void baseAttack(GameObject target){
        ///<param> target : the target of the attack </param> 
        ///<summary> Calculate the amount of damage and attacks the target </summary>

        int damageReceived = (int) ((float) baseAtk * damageMultiplier); 

        if (target.GetComponent<Character>() != null){ // if target is a character
            target.GetComponent<Character>().receiveDamage(damageReceived, attackType, true); // override so the attack is elemental
        } else if (target.GetComponent<Enemy>() != null){ // if target is an enemy
            target.GetComponent<Enemy>().ReceiveDamage(damageReceived, attackType, true); // override so the attack is elemental
        }
    }

    override public void skillLvl1(GameObject target){
        ///<param> target : the target of the attack </param> 
        ///<summary> Calculates the amount of damage and attacks the target (the amount of damage is higher than the base attack) </summary>

        // uses MP (10MP)
        useMP(mpCostSkillLvl1);

        // increases damage if the healer's lvl2 skill is used
        int damageReceived = (int) ((float) baseAtk * damageMultiplier);
        
        // increases damage because the skill has to be stronger than a basic attack
        damageReceived = (int) ((float) damageReceived * strengthenedMultiplier); 

        // uses target's receiveDamage() to apply damage
        if (target.GetComponent<Character>() != null){ // if target is a character
            target.GetComponent<Character>().receiveDamage(damageReceived, attackType, true);
        } else if (target.GetComponent<Enemy>() != null){ // if target is an enemy
            target.GetComponent<Enemy>().ReceiveDamage(damageReceived, attackType, true);
        }
    }

    override public void skillLvl2(GameObject [] target){
        ///<param> target : the targets of the attack </param> 
        ///<summary> Freezes the target </summary>
        
        // uses MP (20MP)
        useMP(mpCostSkillLvl2);

        if (target[0].GetComponent<Character>() != null){ // if target is a character
            target[0].GetComponent<Character>().getStatusList().Add((Status.FROZEN,0));
        } else if (target[0].GetComponent<Enemy>() != null){ // if target is an enemy
            target[0].GetComponent<Enemy>().getStatusList().Add((Status.FROZEN,0));
        }
    }

    override public void skillLvl3(GameObject [] target){
        ///<param> target : the targets of the attack </param> 
        ///<summary> Calculates the amount of damage and attacks the targets (the amount of damage is higher than skillLvl1()) </summary>
         
        // uses MP (30MP)
        useMP(mpCostSkillLvl3);

        // increases damage if the healer's lvl2 skill is used
        int damageReceived = (int) ((float) baseAtk * damageMultiplier);
        
        // increases damage because the skill has to be stronger than a basic attack
        damageReceived = (int) ((float) damageReceived * strengthenedMultiplier); 
        
        // damage is increased (x3 for now) (multiple targets) and shared equally by all targets
        damageReceived *= 3; 
        damageReceived /= target.Length;

        // for each target
        for (int i=0; i<target.Length; i++){
            // applies damage to the target
            if (target[i] != null && target[i].GetComponent<Character>() != null){ // if target is a character
                target[i].GetComponent<Character>().receiveDamage(damageReceived, attackType, true);
            } else if (target[i] != null && target[i].GetComponent<Enemy>() != null){ // if target is an enemy
                target[i].GetComponent<Enemy>().ReceiveDamage(damageReceived, attackType, true);
            }
        }
    }
}