using UnityEngine;
using System;

abstract public class Character : MonoBehaviour
{
    // ---------- Attributes ---------- //

    [SerializeField] protected int lvl, maxHP, currentHP, maxMP, currentMP, baseAtk, teamID;
    [SerializeField] protected float dodgeProbability, damageMultiplier, weakenedMultiplier, strengthenedMultiplier;
    [SerializeField] protected AttackType attackType;
    [SerializeField] protected Weakness weakness;
    
    
    // ---------- Set and Get ---------- //

    public void setLvl(int n){ lvl = n; }
    public void setMaxHP(int n){ maxHP = n; }
    public void setCurrentHP(int n){ currentHP = n; }
    public void setMaxMP(int n){ maxMP = n; }
    public void setCurrentMP(int n){ currentMP = n; }
    public void setBaseAtk(int n){ baseAtk = n; }
    public void setDodgeProbability(float n){ dodgeProbability = n; } 
    public void setDamageMultiplier(float n){ damageMultiplier = n; } 
    public void setWeakenedMultiplier(float n){ weakenedMultiplier = n; } 
    public void setStrengthenedMultiplier(float n){ strengthenedMultiplier = n; } 
    public void setAttackType(AttackType t){ attackType = t; }
    public void setWeakness(Weakness w){ weakness = w; }

    public void setTeamID(int n){ teamID = n; }

    public int getLvl(){ return lvl; }
    public int getMaxHP(){ return maxHP; }
    public int getCurrentHP(){ return currentHP; }
    public int getMaxMP(){ return maxMP; }
    public int getCurrentMP(){ return currentMP; }
    public int getBaseAtk(){ return baseAtk; }
    
    public int getTeamID(){ return teamID; }
    public float getDodgeProbability(){ return dodgeProbability; }
    public float getDamageMultiplier(){ return damageMultiplier; }
    public float getWeakenedMultiplier(){ return weakenedMultiplier; }
    public float getStrengthenedMultiplier(){ return strengthenedMultiplier; }
    public AttackType getAttackType(){ return attackType; }
    public Weakness getWeakness(){ return weakness; }


    // ---------- Methods ---------- //

    public void receiveDamage(int n){
        ///<param> n : amount of damage to be received by the character </param>
        ///<summary> Tries to dodge the attack, then reduces HP by n if dodge fails </summary>

        // tries to dodge the attack 
        if (UnityEngine.Random.value < dodgeProbability){
            Debug.Log("esquivé !");
            return;
        }

        // if dodge fails, damage is taken
        currentHP -= n;
        if (currentHP < 0){
            n += currentHP;
            currentHP = 0;
        }
        //Debug.Log("Le personnage a perdu "+n+"PV");
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

        if (currentMP >= n){
            currentMP -= n;
        } else {
            Debug.LogException(new Exception("[Classe Character] There aren't enough magic points, this method should not be used")); 
        }
    }

    public void baseAttack(GameObject target){
        ///<param> target : the target of the attack </param> 
        ///<summary> Calculate the amount of damage and attacks the target </summary>

        // calculates damage (damage is increased if the healer's lvl2 skill is used)
        int damageReceived = (int) ((float) baseAtk * damageMultiplier); 

        // uses target's receiveDamage() to apply damage
        target.GetComponent<Character>().receiveDamage(damageReceived, attackType, false); 
    }

    abstract public void onClick(); 
    abstract public void receiveDamage(int attack, AttackType attackType, bool elemental);
    abstract public void skillLvl1(GameObject target);
    abstract public void skillLvl2(GameObject [] target);
    abstract public void skillLvl3(GameObject [] target);

}
