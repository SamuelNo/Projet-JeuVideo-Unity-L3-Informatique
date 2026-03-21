using UnityEngine;

abstract public class Enemy : MonoBehavior 
{
    // ---------- Attributes ---------- //

    [SerializeField] protected int maxHP, currentHP, maxMP, currentMP;
    [SerializeField] protected float dodgeProbability;
    [SerializeField] protected AttackType resistance, attackType;
    [SerializeField] protected bool elementalAttack;

    // ---------- Set and Get ---------- //

    public void setMaxHP(int n){ maxHP = n; }
    public void setCurrentHP(int n){ currentHP = n; }
    public void setMaxMP(int n){ maxMP = n; }
    public void setCurrentMP(int n){ currentMP = n; }
    public void setDodgeProbability(float n){ dodgeProbability = n; } 
    public void setResistance(AttackType t){ resistance = t; }
    public void setAttackType(AttackType t){ attackType = t; }
    public void setElementalAttack(bool value){ elementalAttack = value; }

    public int getMaxHP(){ return maxHP; }
    public int getCurrentHP(){ return currentHP; }
    public int getMaxMP(){ return maxMP; }
    public int getCurrentMP(){ return currentMP; }
    public float getDodgeProbability(){ return dodgeProbability; }
    public AttackType getResistance(){ return resistance; }
    public AttackType getAttackType(){ return attackType; }

    public bool isElementalAttack
    { 
        get { return elementalAttack; }
        set { elementalAttack = value; }
    }

    // ---------- Methods ---------- //

    public void receiveDamage(int n){
        ///<param> n : amount of damage to be received by the enemy </param>
        ///<summary> Tries to dodge the attack, then reduces HP by n if dodge fails </summary>

        // tries to dodge the attack 
        if (Random.value < dodgeProbability){
            Debug.Log("Esquivé !");
            return;
        }

        // if dodge fails, damage is taken
        currentHP -= n;
        if (currentHP < 0){
            n += currentHP;
            currentHP = 0;
        }
        Debug.Log("L'adversaire a perdu "+n+"PV");
    }

    public void receiveHeal(int n){
        ///<param> n : amount of HP to be received by the enemy </param> 
        ///<summary> Brings back n amount of HP </summary>

        currentHP += n;
        if (currentHP > maxHP){
            currentHP = maxHP;
        }
    }

    abstract public void onClick(); 
    abstract public void receiveDamage(int attack, AttackType attackType, bool elemental);
    abstract public void targetedAttack(GameObject [] target);
    abstract public void aoeAttack(GameObject [] target);

}