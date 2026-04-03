using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CharacterTest
{
    GameObject fighterGameObject, healerGameObject, mageGameObject, protectorGameObject, enemyGameObject;
    Character fighter, healer, mage, protector;
    Enemy enemy;

    [SetUp]
    public void SetUp(){
        // characters
        fighterGameObject = new GameObject();
        healerGameObject = new GameObject();
        mageGameObject = new GameObject();
        protectorGameObject = new GameObject();

        fighter = fighterGameObject.AddComponent<Fighter>();
        healer = healerGameObject.AddComponent<Healer>();
        mage = mageGameObject.AddComponent<Mage>();
        protector = protectorGameObject.AddComponent<Protector>();

        // enemy
        enemyGameObject = new GameObject();
        enemy = enemyGameObject.AddComponent<Monster>();
    }

    [TearDown]
    public void Teardown(){
        UnityEngine.Object.DestroyImmediate(fighterGameObject);
        UnityEngine.Object.DestroyImmediate(healerGameObject);
        UnityEngine.Object.DestroyImmediate(mageGameObject);
        UnityEngine.Object.DestroyImmediate(protectorGameObject);
        UnityEngine.Object.DestroyImmediate(enemyGameObject);
    }

    // -------------------------------------------------- Class Character --------------------------------------------------
    /*[Test]
    public void TestOnClick(){
        GameObject tmpGameObject = new GameObject();
        BattleUIController controller = tmpGameObject.AddComponent<BattleUIController>(); // creation of a BattleUIController to find

        fighter.onClick();
        // test smth idk

        UnityEngine.Object.DestroyImmediate(tmpGameObject); // suppression of the BattleUIController
        fighter.onClick();
        LogAssert.Expect(LogType.Exception, "Exception: [Classe Character] BattleUIController not found in the scene.");
    }*/

    [Test]
    public void TestReceiveDamage(){
        fighter.setDodgeProbability(0); // makes sure the character won't avoid the damage
        fighter.setCurrentHP(10);

        // the correct amount of HP has to be removed
        fighter.receiveDamage(2);
        Assert.AreEqual(8,fighter.getCurrentHP());

        // HP should not go lower than 0
        fighter.receiveDamage(100);
        Assert.IsFalse(0<fighter.getCurrentHP());
    }

    [Test]
    public void TestDodge(){
        fighter.setDodgeProbability(100); // makes sure the character will dodge
        fighter.setCurrentHP(10);

        // no damage should be taken
        fighter.receiveDamage(10);
        Assert.AreEqual(10,fighter.getCurrentHP());
    }

    [Test]
    public void TestReceiveHeal(){
        fighter.setCurrentHP(10);
        fighter.setMaxHP(20);
        
        // the correct amount of HP should be added
        fighter.receiveHeal(5);
        Assert.AreEqual(15,fighter.getCurrentHP());
        
        // HP should not go over the maxHP
        fighter.receiveHeal(100);
        Assert.IsFalse(30<fighter.getCurrentHP()); 
    }

    [Test]
    public void TestUseMp(){
        fighter.setCurrentMP(10);

        // the correct amount of MP should be removed
        fighter.useMP(2);
        Assert.AreEqual(8,fighter.getCurrentMP());

        // if there isn't enough MP, nothing should be taken and an exception should appear
        fighter.useMP(100);
        Assert.AreEqual(8,fighter.getCurrentMP());
        LogAssert.Expect(LogType.Exception, "Exception: [Classe Character] There aren't enough magic points, this method should not be used");
    }

    [Test]
    public void TestBaseAttack(){
        GameObject targetGameObject = new GameObject(); // creates target for the attack
        Character target = targetGameObject.AddComponent<Fighter>();
        // character
        target.setCurrentHP(22);
        target.setWeakenedMultiplier(3);
        target.setDodgeProbability(0);
        fighter.setBaseAtk(2);
        fighter.setDamageMultiplier(1);
        // enemy
        enemy.CurrentHP = 22;
        enemy.Resistance = AttackType.Ranged;
        enemy.DodgeProbability = 0;

        // if the correct attack type (melee) is used, only 2HP should be removed
        target.setWeakness(Weakness.Elemental);
        fighter.baseAttack(targetGameObject); // character
        Assert.AreEqual(20, target.getCurrentHP());
        fighter.baseAttack(enemyGameObject); // enemy
        Assert.AreEqual(20, enemy.CurrentHP);

        // if the correct attack type (melee) is used, 3*2HP should be removed
        target.setWeakness(Weakness.Melee);
        fighter.baseAttack(targetGameObject);
        Assert.AreEqual(14, target.getCurrentHP());

        // damageMultiplier is increased, 2*3*2HP should be removed
        fighter.setDamageMultiplier(2);
        fighter.baseAttack(targetGameObject);
        Assert.AreEqual(2, target.getCurrentHP());

        UnityEngine.Object.DestroyImmediate(targetGameObject); // destroys target
    }

    // -------------------------------------------------- Class Fighter --------------------------------------------------
    [Test]
    public void TestReceiveDamageFighter(){
        fighter.setDodgeProbability(0);
        fighter.setCurrentHP(10);
        fighter.setWeakenedMultiplier(3);

        // Fighter isn't weak to neutral attacks, only 2HP should be removed
        fighter.receiveDamage(2, AttackType.Neutral, true);
        Assert.AreEqual(8,fighter.getCurrentHP());

        // Fighter isn't weak to melee attacks, only 2HP should be removed
        fighter.receiveDamage(2, AttackType.Melee, true);
        Assert.AreEqual(6,fighter.getCurrentHP());

        // Fighter is weak to ranged attacks, 3*2HP should be removed
        fighter.receiveDamage(2, AttackType.Ranged, true);
        Assert.AreEqual(0,fighter.getCurrentHP());
    }

    [Test]
    public void TestSkillLvl1Fighter(){
        GameObject targetGameObject = new GameObject(); // creates target for the attack
        Character target = targetGameObject.AddComponent(typeof(Fighter)) as Fighter; // the class might be changed once enemies are implemented
        target.setCurrentHP(10);
        target.setWeakness(Weakness.Elemental);
        fighter.setCurrentMP(100);
        fighter.setBaseAtk(1);
        fighter.setDamageMultiplier(1);

        // enemy
        enemy.CurrentHP = 10;
        enemy.Resistance = AttackType.Ranged;
        fighter.skillLvl1(enemyGameObject);
        Assert.AreEqual(90,fighter.getCurrentMP()); // 10MP should be used by the skill
        Assert.IsTrue(8 >= enemy.CurrentHP); // the target is attacked at least 2 times (-2HP)
        Assert.IsTrue(6 <= enemy.CurrentHP); // the target is attacked at most 4 times (-4HP)

        // character
        fighter.skillLvl1(targetGameObject);
        Assert.AreEqual(80,fighter.getCurrentMP()); // 10MP should be used by the skill
        Assert.IsTrue(8 >= target.getCurrentHP()); // the target is attacked at least 2 times (-2HP)
        Assert.IsTrue(6 <= target.getCurrentHP()); // the target is attacked at most 4 times (-4HP)

        target.setCurrentHP(10);
        target.setWeakenedMultiplier(2);
        target.setWeakness(Weakness.Melee);
        fighter.skillLvl1(targetGameObject);
        Assert.IsTrue(6 >= target.getCurrentHP()); // if the correct attack type (melee) is used, at least 2*2*1=4HP should be removed
        Assert.IsTrue(2 <= target.getCurrentHP()); // if the correct attack type (melee) is used, at most 4*2*1=8HP should be removed

        target.setCurrentHP(10);
        target.setWeakness(Weakness.Elemental);
        fighter.setDamageMultiplier(2);
        fighter.skillLvl1(targetGameObject);
        Assert.IsTrue(6 >= target.getCurrentHP()); // damageMultiplier is increased, at least 2*2*1=4HP should be removed
        Assert.IsTrue(2 <= target.getCurrentHP()); // damageMultiplier is increased, at most 4*2*1=8HP should be removed


        UnityEngine.Object.DestroyImmediate(targetGameObject); // destroys target
    }

    [Test]
    public void TestSkillLvl2Fighter(){}

    [Test]
    public void TestSkillLvl3Fighter(){
        GameObject targetGameObject = new GameObject(); // creates target for the attack
        Character target = targetGameObject.AddComponent<Fighter>(); // the class might be changed once enemies are implemented
        target.setCurrentHP(100);
        target.setWeakenedMultiplier(2);
        target.setWeakness(Weakness.Elemental);
        fighter.setCurrentMP(130);
        fighter.setDamageMultiplier(1);
        fighter.setBaseAtk(1);
        fighter.setStrengthenedMultiplier(5);
        fighter.setCurrentHP(99);
        fighter.setMaxHP(99);

        // if more than 1 target is in list, nothing should happen and an exception should appear
        GameObject targetGameObject2 = new GameObject(); // creates a temporary 2nd target
        Character target2 = targetGameObject2.AddComponent<Fighter>();
        target2.setCurrentHP(100);
        target.setWeakness(Weakness.Elemental);
        fighter.skillLvl3(new GameObject[] {targetGameObject, targetGameObject2});
        Assert.AreEqual(100, target.getCurrentHP());
        Assert.AreEqual(100, target2.getCurrentHP());
        Assert.AreEqual(130, fighter.getCurrentMP());
        LogAssert.Expect(LogType.Exception, "Exception: [Classe Fighter] This skill has to target only 1 character");
        UnityEngine.Object.DestroyImmediate(targetGameObject2); // destroys 2nd target

        // enemy
        enemy.CurrentHP = 100;
        enemy.Resistance = AttackType.Ranged;
        fighter.skillLvl3(new GameObject[] {enemyGameObject});
        Assert.AreEqual(100, fighter.getCurrentMP()); // 30MP should be used
        Assert.AreEqual(95, enemy.CurrentHP); // attack is multiplied by strengtenedMultiplier, 5*1HP should be taken 
        Assert.AreEqual(66, fighter.getCurrentHP()); // 1/3 of maxHP should be lost

        // character
        fighter.skillLvl3(new GameObject[] {targetGameObject});
        Assert.AreEqual(70, fighter.getCurrentMP()); // 30MP should be used
        Assert.AreEqual(95, target.getCurrentHP()); // attack is multiplied by strengtenedMultiplier, 5*1HP should be taken 
        Assert.AreEqual(33, fighter.getCurrentHP()); // 1/3 of maxHP should be lost

        // if the correct attack type (melee) is used, 2*5*1=10HP should be removed
        target.setWeakness(Weakness.Melee);
        fighter.skillLvl3(new GameObject[] {targetGameObject});
        Assert.AreEqual(85, target.getCurrentHP());

        // damageMultiplier is increased, 1*1*5*1=20HP should be removed
        fighter.setDamageMultiplier(2);
        fighter.skillLvl3(new GameObject[] {targetGameObject});
        Assert.AreEqual(65, target.getCurrentHP()); 


        UnityEngine.Object.DestroyImmediate(targetGameObject); // destroys target
    }

    // -------------------------------------------------- Class Healer --------------------------------------------------
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

    [Test]
    public void TestSkillLvl1Healer(){
        GameObject targetGameObject = new GameObject(); // creates target for the skill
        Character target = targetGameObject.AddComponent(typeof(Fighter)) as Fighter; // the class might be changed once enemies are implemented
        target.setMaxHP(100);
        target.setCurrentHP(0);
        healer.setCurrentMP(25);
        healer.setMaxHP(100);
        healer.setCurrentHP(0);

        healer.skillLvl1(targetGameObject);
        Assert.AreEqual(15, healer.getCurrentMP()); // 10MP should be used
        Assert.AreEqual(10, target.getCurrentHP()); // 10% of target's maxMP should be restored

        healer.skillLvl1(healerGameObject);
        Assert.AreEqual(5, healer.getCurrentMP()); // 10MP should be used
        Assert.AreEqual(10, healer.getCurrentHP()); // 10% of healer's maxMP should be restored 

        UnityEngine.Object.DestroyImmediate(targetGameObject); // destroys target
    }

    [Test]
    public void TestSkillLvl2Healer(){}

    [Test]
    public void TestSkillLvl3Healer(){
        GameObject targetGameObject = new GameObject(); // creates target for the skill
        Character target = targetGameObject.AddComponent(typeof(Fighter)) as Fighter; // the class might be changed once enemies are implemented
        target.setMaxHP(100);
        target.setCurrentHP(10);
        healer.setCurrentMP(80);
        healer.setMaxHP(100);
        healer.setCurrentHP(10);

        GameObject targetGameObject2 = new GameObject(); // creates target for the skill
        Character target2 = targetGameObject2.AddComponent(typeof(Fighter)) as Fighter; // the class might be changed once enemies are implemented
        target2.setMaxHP(100);
        target2.setCurrentHP(10);
        healer.skillLvl3(new GameObject[] {targetGameObject, targetGameObject2});
        Assert.AreEqual(10, target.getCurrentHP()); // if more than 1 target is in list, nothing should happen and an exception should appear
        Assert.AreEqual(10, target2.getCurrentHP());
        Assert.AreEqual(10, healer.getCurrentHP());
        LogAssert.Expect(LogType.Exception, "Exception: [Classe Healer] Only the ally should be entered as a parameter");
        UnityEngine.Object.DestroyImmediate(targetGameObject2); // destroys target

        healer.skillLvl3(new GameObject[] {targetGameObject});
        Assert.AreEqual(50, healer.getCurrentMP()); // 30MP should be used
        Assert.AreEqual(60, healer.getCurrentHP()); // 50% of healer's maxMP should be restored
        Assert.AreEqual(60, target.getCurrentHP()); // 50% of target's maxMP should be restored

        healer.skillLvl3(new GameObject[] {targetGameObject});
        Assert.IsFalse(100 < healer.getCurrentHP()); // HP should not go over maxHP
        Assert.IsFalse(100 < target.getCurrentHP()); // HP should not go over maxHP


        UnityEngine.Object.DestroyImmediate(targetGameObject); // destroys target
    }


    // -------------------------------------------------- Class Mage --------------------------------------------------
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

    [Test]
    public void TestSkillLvl1Mage(){
        GameObject targetGameObject = new GameObject(); // creates target for the attack
        Character target = targetGameObject.AddComponent(typeof(Fighter)) as Fighter; // the class might be changed once enemies are implemented
        mage.setCurrentMP(100);
        mage.setBaseAtk(1);
        mage.setStrengthenedMultiplier(3);
        mage.setDamageMultiplier(1);
        target.setCurrentHP(100);
        target.setWeakness(Weakness.Melee);
        target.setWeakenedMultiplier(2);
        enemy.CurrentHP = 100;
        enemy.Resistance = AttackType.Melee;
        
        // enemy
        mage.skillLvl1(enemyGameObject);
        Assert.AreEqual(90, mage.getCurrentMP()); // 10MP should be used
        Assert.AreEqual(97, enemy.CurrentHP); // target should lose baseAtk*strengtenedMultiplier = 1*3 = 3HP

        // character
        mage.skillLvl1(targetGameObject);
        Assert.AreEqual(80, mage.getCurrentMP()); // 10MP should be used
        Assert.AreEqual(97, target.getCurrentHP()); // target should lose baseAtk*strengtenedMultiplier = 1*3 = 3HP

        target.setWeakness(Weakness.Ranged);
        mage.skillLvl1(targetGameObject);
        Assert.AreEqual(91, target.getCurrentHP()); // if the attack is of type ranged, target should lose 1*3*2 = 6HP

        mage.setDamageMultiplier(2);
        mage.skillLvl1(targetGameObject);
        Assert.AreEqual(79, target.getCurrentHP()); // damageMultiplier is increased, target should lose 1*3*2*2 = 12HP

        UnityEngine.Object.DestroyImmediate(targetGameObject); // destroys target
    }

    [Test]
    public void TestSkillLvl2Mage(){}

    [Test]
    public void TestSkillLvl3Mage(){
        GameObject targetGameObject1 = new GameObject(), targetGameObject2 = new GameObject(); // creates targets for the attack
        Character target1 = targetGameObject1.AddComponent(typeof(Fighter)) as Fighter, 
                  target2 = targetGameObject2.AddComponent(typeof(Fighter)) as Fighter; // the class might be changed once enemies are implemented
        target1.setCurrentHP(106);
        target1.setWeakness(Weakness.Melee);
        target1.setWeakenedMultiplier(2);
        target2.setCurrentHP(100);
        target2.setWeakness(Weakness.Melee);
        mage.setCurrentMP(160);
        mage.setBaseAtk(1);
        mage.setDamageMultiplier(1);
        mage.setStrengthenedMultiplier(2);
        enemy.CurrentHP = 106;
        enemy.Resistance = AttackType.Melee;
        
        // enemy
        mage.skillLvl3(new GameObject[] {enemyGameObject});
        Assert.AreEqual(130, mage.getCurrentMP()); // 30MP should be used
        Assert.AreEqual(100, enemy.CurrentHP); // damage should be worth to 3 attacks (3*1*2=6)

        // character
        mage.skillLvl3(new GameObject[] {targetGameObject1});
        Assert.AreEqual(100, mage.getCurrentMP()); // 30MP should be used
        Assert.AreEqual(100, target1.getCurrentHP()); // damage should be worth to 3 attacks (3*1*2=6)

        mage.skillLvl3(new GameObject[] {targetGameObject1, targetGameObject2});
        Assert.AreEqual(97, target1.getCurrentHP()); // total damage should be worth to 3 attacks (3*1*2=6HP) (3HP per target)
        Assert.AreEqual(97, target2.getCurrentHP());
        Assert.AreEqual(target1.getCurrentHP(), target2.getCurrentHP()); // each enemy should recieve the same amount of damage

        target1.setWeakness(Weakness.Ranged);
        mage.skillLvl3(new GameObject[] {targetGameObject1});
        Assert.AreEqual(85, target1.getCurrentHP()); // if the attack is of type ranged, target should lose 2*3*1*2 = 12HP

        mage.setDamageMultiplier(2);
        mage.skillLvl3(new GameObject[] {targetGameObject1});
        Assert.AreEqual(61, target1.getCurrentHP()); // damageMultiplier is increased, target should lose 2*2*3*1*2 = 24HP


        UnityEngine.Object.DestroyImmediate(targetGameObject1); // destroys target1
        UnityEngine.Object.DestroyImmediate(targetGameObject2); // destroys target2
    }


    // -------------------------------------------------- Class Protector --------------------------------------------------
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

    [Test]
    public void TestSkillLvl1Protector(){}

    [Test]
    public void TestSkillLvl2Protector(){
        GameObject targetGameObject1 = new GameObject(), targetGameObject2 = new GameObject(); // creates targets for the attack
        Character target1 = targetGameObject1.AddComponent(typeof(Fighter)) as Fighter, 
                  target2 = targetGameObject2.AddComponent(typeof(Fighter)) as Fighter; // the class might be changed once enemies are implemented
        target1.setCurrentHP(10);
        target1.setWeakness(Weakness.Melee);
        target1.setWeakenedMultiplier(2);
        target2.setCurrentHP(10);
        target2.setWeakness(Weakness.Melee);
        protector.setCurrentMP(100);
        protector.setBaseAtk(1);
        protector.setDamageMultiplier(1);
        protector.setStrengthenedMultiplier(3);
        enemy.CurrentHP = 10;
        enemy.Resistance = AttackType.Melee;

        // enemy
        protector.skillLvl2(new GameObject[] {enemyGameObject});
        Assert.AreEqual(80, protector.getCurrentMP()); // 20MP should be used
        Assert.AreEqual(9, enemy.CurrentHP); // damage should be worth to 1 attacks (1HP)

        // character
        protector.skillLvl2(new GameObject[] {targetGameObject1});
        Assert.AreEqual(60, protector.getCurrentMP()); // 20MP should be used
        Assert.AreEqual(9, target1.getCurrentHP()); // damage should be worth to 1 attacks (1HP)

        protector.skillLvl2(new GameObject[] {targetGameObject1, targetGameObject2});
        Assert.AreEqual(8, target1.getCurrentHP()); // each enemy should recieve the same amount of damage (1HP)
        Assert.AreEqual(9, target2.getCurrentHP()); 

        protector.setDamageMultiplier(2);
        protector.skillLvl2(new GameObject[] {targetGameObject1});
        Assert.AreEqual(6, target1.getCurrentHP()); // damageMultiplier is increased, target should lose 2*1 = 2HP


        UnityEngine.Object.DestroyImmediate(targetGameObject1); // destroys target1
        UnityEngine.Object.DestroyImmediate(targetGameObject2); // destroys target2
    }

    [Test]
    public void TestSkillLvl3Protector(){}    
}
