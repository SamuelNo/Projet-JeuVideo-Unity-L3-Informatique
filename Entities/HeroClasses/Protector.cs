using UnityEngine;

public class Protector : Character
{
    // ---------- Initialisation ---------- //
    void Start(){
        this.attackType = AttackType.Neutral;
        this.weakness = Weakness.Elemental;
    }

    // ---------- Methods ---------- //
    
    override public void onClick(){}

    override public void receiveDammage(int attack, AttackType atkType, bool elemental){
        ///<param> attack : the amount of dammage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Calculates the dammage to be received depending on character's weaknesses and applies it </summary>

        // takes original amount of attack dammage
        int dammageReceived = attack;

        // if the attack is this character's weakness (elemental), dammage worsens
        if (elemental){
            dammageReceived = (int) ((float)dammageReceived * weakenedMultiplier);
        }

        // applies dammage (and tries dodging)
        receiveDammage(dammageReceived);
    }

    override public void skillLvl1(GameObject target){} // to do (needs information from class Combat)

    override public void skillLvl2(GameObject [] target){
        ///<param> attack : the amount of dammage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Inflicts dammage to the targets </summary>
        
        // increases dammage if the healer's lvl2 skill is used
        int dammageReceived = (int) ((float) baseAtk * dammageMultiplier);

        // for each target
        for (int i=0; i<target.Length; i++){
            // applies basic attack to the target
            target[i].GetComponent<Character>().receiveDammage(dammageReceived, attackType, false); 
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