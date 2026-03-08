using UnityEngine;

abstract public class Character : MonoBehaviour
{
    // ---------- Attributes ---------- //

    [SerializeField] protected int lvl, maxHP, currentHP, maxMP, currentMP, baseAtk;
    [SerializeField] protected float dodgePrbability, dammageMultiplier, weakenedMultiplier, strengthenedMultiplier;
    [SerializeField] protected AttackType attackType;
    [SerializeField] protected Weakness weakness;
    // weakenedMultiplier multiplies the dammage received : the attack's type is the character's weakness
    // strengthenedMultiplier multiplies the dammage sent : a skill is used instead of a basic attack

    
    // ---------- Set and Get ---------- //

    public void setLvl(int n){ lvl = n; }
    public void setMaxHP(int n){ maxHP = n; }
    public void setCurrentHP(int n){ currentHP = n; }
    public void setMaxMP(int n){ maxMP = n; }
    public void setCurrentMP(int n){ currentMP = n; }
    public void setBaseAtk(int n){ baseAtk = n; }
    public void setDodgePrbability(float n){ dodgePrbability = n; } 
    public void setDamageMultiplier(float n){ dammageMultiplier = n; } 
    public void setWeakenedMultiplier(float n){ weakenedMultiplier = n; } 
    public void setStrengthenedMultiplier(float n){ strengthenedMultiplier = n; } 
    public void setAttackType(AttackType t){ attackType = t; }
    public void setWeakness(Weakness w){ weakness = w; }

    public int getLvl(){ return lvl; }
    public int getMaxHP(){ return maxHP; }
    public int getCurrentHP(){ return currentHP; }
    public int getMaxMP(){ return maxMP; }
    public int getCurrentMP(){ return currentMP; }
    public int getBaseAtk(){ return baseAtk; }
    public float getDodgePrbability(){ return dodgePrbability; }
    public float getDamageMultiplier(){ return dammageMultiplier; }
    public float getWeakenedMultiplier(){ return weakenedMultiplier; }
    public float getStrengthenedMultiplier(){ return strengthenedMultiplier; }
    public AttackType getAttackType(){ return attackType; }
    public Weakness getWeakness(){ return weakness; }


    // ---------- Methods ---------- //

    public void receiveDammage(int n){
        ///<param> n : amount of dammage to be received by the character </param>
        ///<summary> Tries to dodge the attack, then reduces HP by n if dodge fails </summary>

        // tries to dodge the attack 
        if (Random.value < dodgePrbability){
            Debug.Log("esquivé !");
            return;
        }

        // if dodge fails, damage is taken
        currentHP -= n;
        if (currentHP < 0){
            currentHP = 0;
        }
    }
    

    public void receiveHeal(int n){
        ///<param> n : amount of HP to be received by the character </param> 
        ///<summary> Brings back n amount of HP </summary>

        currentHP += n;
        if (currentHP > maxHP){
            currentHP = maxHP;
        }
    }

    public void useMP (int n){
        ///<param>  amount of MP used by the character </param> 
        ///<summary> Reduces n amount of MP </summary>

        if (currentMP > n){
            currentMP -= n;
        } else {
            currentMP = 0;
        }
    }

    public void baseAttack(GameObject target){
        ///<param> target : the target of the attack </param> 
        ///<summary> Calculate the amount of dammage and attacks the target </summary>

        // calculates dammage (dammage is increased if the healer's lvl2 skill is used)
        int dammageReceived = (int) ((float) baseAtk * dammageMultiplier); 

        // uses target's receiveDammage() to apply dammage
        target.GetComponent<Character>().receiveDammage(dammageReceived, attackType, false); 
    }

    abstract public void onClick(); 
    abstract public void receiveDammage(int attack, AttackType attackType, bool elemental);
    abstract public void skillLvl1(GameObject target);
    abstract public void skillLvl2(GameObject [] target);
    abstract public void skillLvl3(GameObject [] target);

}
