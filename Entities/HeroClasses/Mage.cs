using UnityEngine;

public class Mage : Character
{
    // ---------- Initialisation ---------- //
    void Start(){
        this.attackType = AttackType.Ranged;
        this.weakness = Weakness.Melee;
    }

    // ---------- Methods ---------- //
    
    override public void onClick(){}

    override public void receiveDammage(int attack, AttackType atkType, bool elemental){
        ///<param> attack : the amount of dammage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param> 
        ///<summary> Calculates the dammage to be received depending on character's weaknesses and applies it </summary>

        // takes original amount of attack dammage
        int dammageReceived = attack;

        // if the attack type is this character's weakness (ranged), dammage worsens 
        if (string.Equals(this.weakness.ToString(), atkType.ToString())){ 
            dammageReceived = (int) ((float)dammageReceived * weakenedMultiplier); 
        }

        // applies dammage (and tries dodging)
        receiveDammage(dammageReceived);
    }

    override public void skillLvl1(GameObject target){
        ///<param> target : the target of the attack </param> 
        ///<summary> Calculates the amount of dammage and attacks the target (the amount of dammage is higher than the base attack) </summary>

        // increases dammage if the healer's lvl2 skill is used
        int dammageReceived = (int) ((float) baseAtk * dammageMultiplier);
        
        // increases dammage because the skill has to be stronger than a basic attack
        dammageReceived = (int) ((float) dammageReceived * strengthenedMultiplier); 

        // uses target's receiveDammage() to apply dammage
        target.GetComponent<Character>().receiveDammage(dammageReceived, attackType, false); // might be an elemental attack ? not sure, so not elemental for now. 
    }

    override public void skillLvl2(GameObject [] target){} // to do (needs information from class Combat)

    override public void skillLvl3(GameObject [] target){
        ///<param> target : the targets of the attack </param> 
        ///<summary> Calculates the amount of dammage and attacks the targets (the amount of dammage is higher than skillLvl1()) </summary>
         
        // increases dammage if the healer's lvl2 skill is used
        int dammageReceived = (int) ((float) baseAtk * dammageMultiplier);
        
        // increases dammage because the skill has to be stronger than a basic attack
        dammageReceived = (int) ((float) dammageReceived * strengthenedMultiplier); 
        
        /*
        // increases dammage x3 (multiple targets)
        dammageReceived *= 3; 

        // damage is shared equally by all targets
        dammageReceived /= target.Lenght
        */

        // for each target
        for (int i=0; i<target.Length; i++){
            // applies dammage to the target
            target[i].GetComponent<Character>().receiveDammage(dammageReceived, attackType, false); 
        }
    }
}

/* 
Niveau 2 - Entrave de givre :  
Entoure l’ennemi de glace. Peut geler l’ennemi et l’empêcher d’agir pendant un tour. 
*/
