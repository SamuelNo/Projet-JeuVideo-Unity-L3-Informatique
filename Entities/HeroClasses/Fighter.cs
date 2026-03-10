using UnityEngine;

public class Fighter : Character
{
    // ---------- Initialisation ---------- //
    void Start(){
        this.attackType = AttackType.Melee;
        this.weakness = Weakness.Ranged;
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
        ///<summary> Attacks the target a certain amount of times </summary>

        // increases dammage if the healer's lvl2 skill is used
        int dammageReceived = (int) ((float) baseAtk * dammageMultiplier);
        
        // attacks target 2 to 4 times
        for (int i=0; i<Random.Range(2,4); i++){
            target.GetComponent<Character>().receiveDammage(dammageReceived, attackType, false); 
        }
    }

    override public void skillLvl2(GameObject [] target){} // to do (needs information from class Combat)

    override public void skillLvl3(GameObject [] target){
        ///<param> target : the targets of the attack (only 1 target) </param> 
        ///<summary> Attacks the target (the amount of dammage is higher than the total of dammage caused by skillLvl1()), and loses HP in the process (for now, 50% of applied attack dammage)</summary>
        
        // checks that only 1 taget is chosen
        if (target.Length != 1){
            Debug.Log("Classe Fighter : Cette compétence ne doit viser qu'un personnage");
            return;
        }
        // increases dammage if the healer's lvl2 skill is used
        int dammageReceived = (int) ((float) baseAtk * dammageMultiplier);

        // increases the dammage of the attack 
        dammageReceived = (int) ((float) dammageReceived * strengthenedMultiplier);

        // applies dammage to the target
        target[0].GetComponent<Character>().receiveDammage(dammageReceived, attackType, false); 

        // loses HP (1/3 of maxHP)
        this.currentHP -= (int) ((float) maxHP * .33f);
        if (this.currentHP < 0){
            this.currentHP = 0;
        }
    }
}

/* 
Niveau 2 - Frappe furieuse :
Charge son poing et attaque au tour suivant avec un coup enflammé infligeant de lourds dégâts. 
*/