using UnityEngine;
public class Monster : Enemy 
{
    // ---------- Initialisation ---------- //
    protected override void Awake()
    {
        base.Awake();

        this.costAOE = 5;
        this.costSpecial = 0;
        this.costHeal = 5;
        this.costBoost = 7;
        this.costProtection = 5;
    }

    // ---------- Methods ---------- //

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

    override public void TargetedAttack(GameObject target)
    {
        ///<param> target : the target of the attack </param> 
        ///<summary> Attacks the target</summary>

        Debug.Log(gameObject.name + " a utilisé Targeted Attack");

        int damage = (int)(basePower * GetRankMultiplier() * BuffAttack);
        target.GetComponent<Character>().receiveDamage(damage, this.AttackTypeUsed, this.ElementalAttack);

    }

    override public void AoeAttack(GameObject[] target)
    {
        ///<param> target : the targets of the attack </param> 
        ///<summary> Attacks the targets </summary>

        Debug.Log(gameObject.name + " a utilisé AOE Attack");

        UseMp(GetMPCost(costAOE));

        int damage = (int)(basePower * GetRankMultiplier() * 0.6f * BuffAttack); //0.6f is aoeMultiplier

        //will be dealt to multiple target
        for (int i = 0; i < target.Length; i++)
        {
            target[i].GetComponent<Character>().receiveDamage(damage, AttackTypeUsed, ElementalAttack);
        }

    }

}