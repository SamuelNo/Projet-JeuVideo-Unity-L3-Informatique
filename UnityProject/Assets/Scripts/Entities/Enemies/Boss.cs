using UnityEngine;

public class Boss : Enemy 
{
    // ---------- Initialisation ---------- //
    protected override void Awake()
    {
        base.Awake();
        this.Rank = Rank.S;

        this.costAOE = 10;
        this.costSpecial = 15;
        this.costHeal = 10;
        this.costBoost = 12;
        this.costProtection = 8;
    }

    // ---------- Methods ---------- //

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

    override public void TargetedAttack(GameObject target)
    {
        ///<param> target : the target of the attack </param> 
        ///<summary> Attacks the target </summary>

        Debug.Log(gameObject.name + " a utilisé Targeted Attack");

        int damage = (int)(basePower * GetRankMultiplier() * BuffAttack);
        target.GetComponent<Character>().receiveDamage(damage, AttackTypeUsed, ElementalAttack);

    }

    override public void AoeAttack(GameObject[] target)
    {
        ///<param> target : the targets of the attack </param> 
        ///<summary> Attacks the targets </summary>

        Debug.Log(gameObject.name + " a utilisé AOE Attack");

        UseMp(GetMPCost(costAOE));

        int damage = (int)(basePower * GetRankMultiplier() * 0.7f * BuffAttack);

        //will be dealt to multiple target
        for (int i = 0; i < target.Length; i++)
        {
            target[i].GetComponent<Character>().receiveDamage(damage, AttackTypeUsed, ElementalAttack);
        }
    }


    public void SpecialAttack(GameObject[] target)
    {
        ///<param> target : the targets of the attack </param> 
        ///<summary> Attacks the targets </summary>
        ///
        Debug.Log(gameObject.name + " a utilisé Special Attack");

        UseMp(GetMPCost(costSpecial));

        int damage = (int)(basePower * GetRankMultiplier() * 1.2f);
        for (int i = 0; i < target.Length; i++)
        {
            target[i].GetComponent<Character>().receiveDamage(damage, AttackTypeUsed, ElementalAttack);
        }
    }
}