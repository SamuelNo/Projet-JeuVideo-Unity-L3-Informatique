using UnityEngine;

public class Boss : Enemy 
{
    // ---------- Initialisation ---------- //
    //to do
    
    // ---------- Methods ---------- //
    override public void onClick(){}

    override receiveDamage(int attack, AttackType attackType, bool elemental){
        ///<sumary>Calculates damage taken and calls the receiveDamage method to deal damage</sumary>
        // takes original amount of attack damage
        int damageReceived = attack;

        if (string.Equals(this.resistance.toString(), attackType.toString())){
            damageReceived = (int) (attack * 0.4);
        } //0.4 can be changed

        receiveDamage(damageReceived);
    }

    override public void targetedAttack(GameObject target){
    ///<param> target : the target of the attack </param> 
    ///<summary> Attacks the target (depends on the HP of the boss)</summary>
        damage = (int) this.maxHP * 0.1; 
        target.getComponent<Character>().receiveDamage(damage, this.attackType, this.elementalAttack);
        
    }

    override public void aoeAttack(GameObject [] target){
    ///<param> target : the targets of the attack </param> 
    ///<summary> Attacks the targets </summary>
    
        damage = (int) this.maxHP * 0.1; 
        for (int i=0; i<target.Length; i++){
            target[i].getComponent<Character>().receiveDamage(damage, this.attackType, this.elementalAttack);
        }
    }

    override public void specialAttack(GameObject [] target){
    ///<param> target : the targets of the attack </param> 
    ///<summary> Attacks the targets </summary>
    
        damage = (int) this.maxHP * 0.3; 
        target.getComponent<Character>().receiveDamage(damage, this.attackType, this.elementalAttack);
    }
}