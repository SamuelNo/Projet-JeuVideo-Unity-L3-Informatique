using UnityEngine;

abstract public class Enemy : MonoBehaviour 
{
    // ---------- Attributes ---------- //

    [SerializeField] protected int maxHP, currentHP, maxMP, currentMP;
    [SerializeField] protected float dodgeProbability;
    [SerializeField] protected AttackType resistance, attackTypeUsed;
    [SerializeField] protected bool elementalAttack;

    // ---------- Set and Get ---------- //

    public int MaxHP { get => maxHP; protected set => maxHP = value; }
    public int CurrentHP { get => currentHP; protected set => currentHP = value; }
    public int MaxMP { get => maxMP; protected set => maxMP = value; }
    public int CurrentMP { get => currentMP; protected set => currentMP = value; }
    public float DodgeProbability { get => dodgeProbability; protected set => dodgeProbability = value; } 
    public AttackType Resistance{ get => resistance; protected set => resistance = value; }
    public AttackType AttackTypeUsed { get => attackTypeUsed; protected set => attackTypeUsed = value; }
    public bool ElementalAttack { get => elementalAttack; protected set => elementalAttack = value; }



    // ---------- Methods ---------- //

    public void ReceiveDamage(int n){
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

    public void ReceiveHeal(int n){
        ///<param> n : amount of HP to be received by the enemy </param> 
        ///<summary> Brings back n amount of HP </summary>

        currentHP += n;
        if (currentHP > maxHP){
            currentHP = maxHP;
        }
    }

    abstract public void OnClick(); 
    abstract public void ReceiveDamage(int attack, AttackType attackType, bool elemental);
    abstract public void TargetedAttack(GameObject target);
    abstract public void AoeAttack(GameObject [] target);

}