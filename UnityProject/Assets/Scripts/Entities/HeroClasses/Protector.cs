using UnityEngine;
using System;

public class Protector : Character
{
    // ---------- Initialisation ---------- //
    void Awake(){
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.Elemental;
    }
    void Reset(){
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.Elemental;
    }

    // ---------- Methods ---------- //
    
    override public void onClick(){}

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

    override public void skillLvl1(GameObject target){} // to do (needs information from class Combat)

    override public void skillLvl2(GameObject [] target){
        ///<param> attack : the amount of damage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Inflicts damage to the targets </summary>
        
        // uses MP (20MP)
        useMP(20);

        // increases damage if the healer's lvl2 skill is used
        int damageReceived = (int) ((float) baseAtk * damageMultiplier);

        // for each target
        for (int i=0; i<target.Length; i++){
            // applies basic attack to the target
            target[i].GetComponent<Character>().receiveDamage(damageReceived, attackType, false); 
        }
    }

    override public void skillLvl3(GameObject [] target){} // to do (needs information from class Combat)

}

/* 
Niveau 1 - Protection :
Protège un personnage pendant un tour. Si celui-ci prend des dégâts, le protecteur sera touché à la place.

Niveau 3 - Barrière protectrice :
Déploie une barrière qui protège tous les alliés pendant un tour. Cette compétence ne peut pas être utilisée deux tours consécutifs.
*/