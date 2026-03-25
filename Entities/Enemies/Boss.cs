using UnityEngine;

public class Boss : Enemy 
{
    // ---------- Methods ---------- //
    override public void OnClick(){}

    override public void ReceiveDamage(int attack, AttackType attackType, bool elemental){
        ///<param> attack : the amount of damage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param>
        ///<sumary> Calculates damage taken and calls the ReceiveDamage method to deal damage </sumary>
        
        // takes original amount of attack damage
        int damageReceived = attack;

        // if the Boss is resistant to the attack, it will take 60% less damage 
        if (this.resistance == attackType){
            damageReceived = (int) (attack * 0.4f);
        }

        ReceiveDamage(damageReceived);
    }


    //Attacks
    //The attacks depend on the boss's HP

    override public void TargetedAttack(GameObject target){
    ///<param> target : the target of the attack </param> 
    ///<summary> Attacks the target (depends on the HP of the boss) </summary>
    
        int damage = (int)(MaxHP * 0.15); //15% of the boss's HP 
        target.GetComponent<Character>().ReceiveDamage(damage, this.attackType, this.elementalAttack);
        
    }

    override public void AoeAttack(GameObject [] target){
    ///<param> target : the targets of the attack </param> 
    ///<summary> Attacks the targets </summary>
    
        int damage = (int)(MaxHP * 0.15f); //15% of its health

        //will be dealt to multiple target
        for (int i=0; i<target.Length; i++){
            target[i].GetComponent<Character>().ReceiveDamage(damage, this.attackType, this.elementalAttack);
        }
    }

    public void specialAttack(GameObject [] target){
    ///<param> target : the targets of the attack </param> 
    ///<summary> Attacks the targets </summary>
    
        int damage = (int)(MaxHP * 0.3f); //30% of its HP
        for (int i=0; i<target.Length; i++){
            target[i].GetComponent<Character>().ReceiveDamage(damage, this.attackType, this.elementalAttack);
        }
    }
}