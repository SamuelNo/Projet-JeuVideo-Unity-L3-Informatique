using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestScript
{
    GameObject fighterGameObject, healerGameObject, mageGameObject, protectorGameObject;
    Character fighter, healer, mage, protector;

    [SetUp]
    public void SetUp(){
        fighterGameObject = new GameObject();
        healerGameObject = new GameObject();
        mageGameObject = new GameObject();
        protectorGameObject = new GameObject();

        fighter = fighterGameObject.AddComponent(typeof(Fighter)) as Fighter;
        healer = healerGameObject.AddComponent(typeof(Healer)) as Healer;
        mage = mageGameObject.AddComponent(typeof(Mage)) as Mage;
        protector = protectorGameObject.AddComponent(typeof(Protector)) as Protector;
    }

    [TearDown]
    public void Teardown(){
        Object.DestroyImmediate(fighterGameObject);
        Object.DestroyImmediate(healerGameObject);
        Object.DestroyImmediate(mageGameObject);
        Object.DestroyImmediate(protectorGameObject);
    }

    // ---------- Class Character ----------
    [Test]
    public void TestReceiveDamage(){
        fighter.setDodgeProbability(0); // makes sure the character won't avoid the damage
        fighter.setCurrentHP(10);

        fighter.receiveDamage(2);
        Assert.AreEqual(8,fighter.getCurrentHP()); // the correct amount of HP has to be removed

        fighter.receiveDamage(100);
        Assert.IsFalse(0<fighter.getCurrentHP()); // HP should not go lower than 0
    }

    [Test]
    public void TestDodge(){
        fighter.setDodgeProbability(100); // makes sure the character will dodge
        fighter.setCurrentHP(10);

        fighter.receiveDamage(10);
        Assert.AreEqual(10,fighter.getCurrentHP()); // no dammage should be taken
    }

    [Test]
    public void TestReceiveHeal(){
        fighter.setCurrentHP(10);
        fighter.setMaxHP(20);
        
        fighter.receiveHeal(5);
        Assert.AreEqual(15,fighter.getCurrentHP()); // the correct amount of HP should be added
        
        fighter.receiveHeal(100);
        Assert.IsFalse(30<fighter.getCurrentHP()); // HP should not go over the maxHP
    }

    [Test]
    public void TestUseMp(){
        fighter.setCurrentMP(10);

        fighter.useMP(2);
        Assert.AreEqual(8,fighter.getCurrentMP()); // the correct amount of MP should be removed

        fighter.useMP(100);
        Assert.AreEqual(8,fighter.getCurrentMP()); // if there isn't enough MP, nothing should be taken (might change to exception?)
    }

    [Test]
    public void TestBaseAttack(){
        GameObject targetGameObject = new GameObject(); // creates target for the attack
        Character target = targetGameObject.AddComponent(typeof(Fighter)) as Fighter; // the class might be changed once enemies are implemented
        target.setCurrentHP(10);
        target.setWeakenedMultiplier(3);
        target.setDodgeProbability(0);
        fighter.setBaseAtk(2);
        fighter.setDamageMultiplier(1);


        target.setWeakness(Weakness.Elemental);
        fighter.baseAttack(targetGameObject);
        Assert.AreEqual(8, target.getCurrentHP()); // if correct attack type (melee) is used, only 2HP should be removed

        target.setWeakness(Weakness.Melee);
        fighter.baseAttack(targetGameObject);
        Assert.AreEqual(2, target.getCurrentHP()); // if correct attack type (melee) is used, 3*2HP should be removed

        fighter.setDamageMultiplier(2);
        fighter.baseAttack(targetGameObject);
        Assert.AreEqual(2, target.getCurrentHP()); // damageMultiplier is increased, 2*3*2HP should be removed


        Object.DestroyImmediate(targetGameObject); // destroys target
    }

    // ---------- Class Fighter ----------
    [Test]
    public void TestReceiveDamageFighter(){
        fighter.setDodgeProbability(0);
        fighter.setCurrentHP(10);
        fighter.setWeakenedMultiplier(3);

        fighter.receiveDamage(2, AttackType.Neutral, true);
        Assert.AreEqual(8,fighter.getCurrentHP()); // Fighter isn't weak to neutral attacks, only 2HP should be removed

        fighter.receiveDamage(2, AttackType.Melee, true);
        Assert.AreEqual(6,fighter.getCurrentHP()); // Fighter isn't weak to melee attacks, only 2HP should be removed

        fighter.receiveDamage(2, AttackType.Ranged, true);
        Assert.AreEqual(0,fighter.getCurrentHP()); // Fighter is weak to ranged attacks, 3*2HP should be removed
    }

    [Test]
    public void TestSkillLvl1Fighter(){
        
    }
/*
    [Test]
    public void TestSkillLvl2Fighter(){}

    [Test]
    public void TestSkillLvl3Fighter(){}
*/
    // ---------- Class Healer ----------
    [Test]
    public void TestReceiveDamageHealer(){
        healer.setDodgeProbability(0);
        healer.setCurrentHP(22);
        healer.setWeakenedMultiplier(3);

        healer.receiveDamage(2, AttackType.Neutral, false);
        Assert.AreEqual(20,healer.getCurrentHP()); // Healer isn't weak to neutral non-elemental attacks, only 2HP should be removed

        healer.receiveDamage(2, AttackType.Neutral, true);
        Assert.AreEqual(14,healer.getCurrentHP()); // Healer is weak to neutral elemental attacks, 3*2HP should be removed

        healer.receiveDamage(2, AttackType.Ranged, false);
        Assert.AreEqual(8,healer.getCurrentHP()); // Healer is weak to ranged non-elemental attacks, 3*2HP should be removed

        healer.receiveDamage(2, AttackType.Melee, true);
        Assert.IsTrue(2>healer.getCurrentHP()); // Healer is extreamly weak to non-neutral elemental attacks, >3*2HP should be removed
        }
/*
    [Test]
    public void TestSkillLvl1Healer(){}

    [Test]
    public void TestSkillLvl2Healer(){}

    [Test]
    public void TestSkillLvl3Healer(){}
*/

    // ---------- Class Mage ----------
    [Test]
    public void TestReceiveDamageMage(){
        mage.setDodgeProbability(0);
        mage.setCurrentHP(10);
        mage.setWeakenedMultiplier(3);

        mage.receiveDamage(2, AttackType.Neutral, true);
        Assert.AreEqual(8,mage.getCurrentHP()); // Mage isn't weak to neutral attacks, only 2HP should be removed

        mage.receiveDamage(2, AttackType.Ranged, true);
        Assert.AreEqual(6,mage.getCurrentHP()); // Mage isn't weak to ranged attacks, only 2HP should be removed

        mage.receiveDamage(2, AttackType.Melee, true);
        Assert.AreEqual(0,mage.getCurrentHP()); // Mage is weak to melee attacks, 3*2HP should be removed
    }
/*
    [Test]
    public void TestSkillLvl1Mage(){}

    [Test]
    public void TestSkillLvl2Mage(){}

    [Test]
    public void TestSkillLvl3Mage(){}
*/

    // ---------- Class Protector ----------
    [Test]
    public void TestReceiveDamageProtector(){
        protector.setDodgeProbability(0);
        protector.setCurrentHP(10);
        protector.setWeakenedMultiplier(3);

        protector.receiveDamage(2, AttackType.Neutral, false);
        Assert.AreEqual(8,protector.getCurrentHP()); // Protector isn't weak to non-elemental attacks, only 2HP should be removed

        protector.receiveDamage(2, AttackType.Neutral, true);
        Assert.AreEqual(2,protector.getCurrentHP()); // Protector is weak to elemental attacks, 3*2HP should be removed
    }
/*
    [Test]
    public void TestSkillLvl1Protector(){}

    [Test]
    public void TestSkillLvl2Protector(){}

    [Test]
    public void TestSkillLvl3Protector(){}*/
}
