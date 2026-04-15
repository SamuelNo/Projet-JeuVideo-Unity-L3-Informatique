using UnityEngine;
using System;

public class Protector : Character
{
    // ---------- Initialisation ---------- //
    protected override void Awake(){
        base.Awake();
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.Elemental;
        this.gameObject.name = "Protector";
    }
    void Reset(){
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.Elemental;
        this.gameObject.name = "Protector";

    }

    // ---------- Methods ---------- //

    override public void receiveDamage(int attack, AttackType atkType, bool elemental){
        ///<param> attack : the amount of damage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Calculates the damage to be received depending on character's weaknesses and applies it </summary>

        // takes original amount of attack damage
        int damageReceived = attack;

        // if the attack is this character's weakness (elemental), damage worsens
        if (elemental){
            damageReceived = (int) ((float)damageReceived * weakenedMultiplier);
        }

        // applies damage (and tries dodging)
        receiveDamage(damageReceived);
    }

    override public void skillLvl1(GameObject target){
        ///<param> target : the target of the attack </param> 
        ///<summary> Protects the target </summary>
        
        // uses MP (10MP)
        useMP(mpCostSkillLvl1);

        // applies protection
        target.GetComponent<Character>().getStatusList().Add((Status.PROTECTED,1));
    }

    override public void skillLvl2(GameObject [] target){
        ///<param> attack : the amount of damage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Inflicts damage to the targets </summary>
        
        // uses MP (20MP)
        useMP(mpCostSkillLvl2);

        // increases damage if the healer's lvl2 skill is used
        int damageReceived = (int) ((float) baseAtk * damageMultiplier);

        // for each target
        for (int i=0; i<target.Length; i++){
            // applies basic attack to the target
            if (target[i].GetComponent<Character>() != null){ // if target is a character
                target[i].GetComponent<Character>().receiveDamage(damageReceived, attackType, false);
            } else if (target[i].GetComponent<Enemy>() != null){ // if target is an enemy
                target[i].GetComponent<Enemy>().ReceiveDamage(damageReceived, attackType, false);
            }
        }
    }

    override public void skillLvl3(GameObject [] target){
        ///<param> target : the targets of the attack </param> 
        ///<summary> Protects the target </summary>
        
        // uses MP (30MP)
        useMP(mpCostSkillLvl3);

        // applies shield to all allies
        for (int i=0; i<target.Length; i++){
            target[i].GetComponent<Character>().getStatusList().Add((Status.SHIELDED,1));
        }
    }

}

/* 
Niveau 1 - Protection :
Protège un personnage pendant un tour. Si celui-ci prend des dégâts, le protecteur sera touché à la place.

Niveau 3 - Barrière protectrice :
Déploie une barrière qui protège tous les alliés pendant un tour. Cette compétence ne peut pas être utilisée deux tours consécutifs.
*/