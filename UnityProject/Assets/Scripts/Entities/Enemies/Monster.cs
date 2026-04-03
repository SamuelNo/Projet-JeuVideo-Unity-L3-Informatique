using UnityEngine;
public class Monster : Enemy 
{
    // ---------- Methods ---------- //
    override public void OnClick()
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

    override public void ReceiveDamage(int attack, AttackType attackType, bool elemental){
        ///<param> attack : the amount of damage taken, atkType : the type of the attack, elemental : weither the attack is elemental </param>
        ///<sumary> Calculates damage taken and calls the ReceiveDamage method to deal damage </sumary>
        
        // takes original amount of attack damage
        int damageReceived = attack;

        // if the Monster is resistant to the attack, it will take 40% less damage  
        if (this.resistance == attackType){
            damageReceived = (int) (attack * 0.6f);
        } 

        ReceiveDamage(damageReceived);
    }


    //Attacks
    //The attacks depend on the monster's HP

    override public void TargetedAttack(GameObject target){
    ///<param> target : the target of the attack </param> 
    ///<summary> Attacks the target (depends on the HP of the monster) </summary>
    
        int damage = (int)(MaxHP * 0.1f); //10% of the monster's HP 
        target.GetComponent<Character>().receiveDamage(damage, this.AttackTypeUsed, this.ElementalAttack);
        
    }

    override public void AoeAttack(GameObject [] target){
    ///<param> target : the targets of the attack </param> 
    ///<summary> Attacks the targets </summary>
    
        int damage = (int)(MaxHP * 0.1f); //10% of its health

        //will be dealt to multiple target
        for (int i=0; i<target.Length; i++){
            target[i].GetComponent<Character>().receiveDamage(damage, AttackTypeUsed, ElementalAttack);
        }
    }

}