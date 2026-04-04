using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

public class PrincipalTests
{
    // script access
    GameObject scriptsGameObject;
    Principal scriptPrincipal;
    MenuButtons scriptMenuButtons;

    // add visual components here
    GameObject pvpButton, pvmButton, menuButton, nextPlayerButton, startPvPFightButton, startPvMFightButton, leftButton1, rightButton1, leftButton2, rightButton2, stageButton, choseStageButton, fighterSprite1, healerSprite1, mageSprite1, protectorSprite1, fighterSprite2, healerSprite2, mageSprite2, protectorSprite2, stageSprite0, stageSprite1, stageSprite2, stageCharacterSprite;
    GameObject[] characterSpriteList1, characterSpriteList2; 

    [SetUp]
    public void SetUp(){
        // set visual components here
        pvpButton = new GameObject();
        pvmButton = new GameObject();
        menuButton = new GameObject();
        nextPlayerButton = new GameObject();
        startPvPFightButton = new GameObject();
        startPvMFightButton = new GameObject();
        leftButton1 = new GameObject();
        rightButton1 = new GameObject();
        leftButton2 = new GameObject();
        rightButton2 = new GameObject();
        stageButton = new GameObject();
        choseStageButton = new GameObject();
        fighterSprite1 = new GameObject();
        healerSprite1 = new GameObject();
        mageSprite1 = new GameObject();
        protectorSprite1 = new GameObject();
        fighterSprite2 = new GameObject();
        healerSprite2 = new GameObject();
        mageSprite2 = new GameObject();
        protectorSprite2 = new GameObject();
        stageSprite0 = new GameObject();
        stageSprite1 = new GameObject();
        stageSprite2 = new GameObject();
        stageCharacterSprite = new GameObject();

        pvpButton.AddComponent<Button>();
        pvmButton.AddComponent<Button>();
        menuButton.AddComponent<Button>();
        nextPlayerButton.AddComponent<Button>();
        startPvPFightButton.AddComponent<Button>();
        startPvMFightButton.AddComponent<Button>();
        leftButton1.AddComponent<Button>();
        rightButton1.AddComponent<Button>();
        leftButton2.AddComponent<Button>();
        rightButton2.AddComponent<Button>();
        stageButton.AddComponent<Button>();
        choseStageButton.AddComponent<Button>();

        pvpButton.name = "PvPButton";
        pvmButton.name = "PvMButton";
        menuButton.name = "MenuButton";
        nextPlayerButton.name = "NextPlayerButton";
        startPvPFightButton.name = "StartPvPFightButton";
        startPvMFightButton.name = "StartPvMFightButton";
        leftButton1.name = "LeftButton1";
        rightButton1.name = "RightButton1";
        leftButton2.name = "LeftButton2";
        rightButton2.name = "RightButton2";
        stageButton.name = "StageButton";
        choseStageButton.name = "ChoseStageButton";
        fighterSprite1.name = "FighterSpriteLeft";
        healerSprite1.name = "HealerSpriteLeft";
        mageSprite1.name = "MageSpriteLeft";
        protectorSprite1.name = "ProtectorSpriteLeft";
        fighterSprite2.name = "FighterSpriteRight";
        healerSprite2.name = "HealerSpriteRight";
        mageSprite2.name = "MageSpriteRight";
        protectorSprite2.name = "ProtectorSpriteRight";
        stageSprite0.name = "StageSprite0";
        stageSprite1.name = "StageSprite1";
        stageSprite2.name = "StageSprite2";
        stageCharacterSprite.name = "StageCharacterSprite";

        // switchCharacterX() tests
        characterSpriteList1 = new GameObject[] {fighterSprite1, healerSprite1, mageSprite1, protectorSprite1};
        characterSpriteList2 = new GameObject[] {fighterSprite2, healerSprite2, mageSprite2, protectorSprite2};

        // script access
        scriptsGameObject = new GameObject();
        scriptsGameObject.name = "Script";
        scriptPrincipal = scriptsGameObject.AddComponent<Principal>();
        scriptMenuButtons = scriptsGameObject.AddComponent<MenuButtons>();
    }

    [TearDown]
    public void Teardown(){
        // add visual components here
        UnityEngine.Object.DestroyImmediate(pvpButton);
        UnityEngine.Object.DestroyImmediate(pvmButton);
        UnityEngine.Object.DestroyImmediate(menuButton);
        UnityEngine.Object.DestroyImmediate(nextPlayerButton);
        UnityEngine.Object.DestroyImmediate(startPvPFightButton);
        UnityEngine.Object.DestroyImmediate(startPvMFightButton);
        UnityEngine.Object.DestroyImmediate(leftButton1);
        UnityEngine.Object.DestroyImmediate(rightButton1);
        UnityEngine.Object.DestroyImmediate(leftButton2);
        UnityEngine.Object.DestroyImmediate(rightButton2);
        UnityEngine.Object.DestroyImmediate(stageButton);
        UnityEngine.Object.DestroyImmediate(choseStageButton);
        UnityEngine.Object.DestroyImmediate(fighterSprite1);
        UnityEngine.Object.DestroyImmediate(healerSprite1);
        UnityEngine.Object.DestroyImmediate(mageSprite1);
        UnityEngine.Object.DestroyImmediate(protectorSprite1);
        UnityEngine.Object.DestroyImmediate(fighterSprite2);
        UnityEngine.Object.DestroyImmediate(healerSprite2);
        UnityEngine.Object.DestroyImmediate(mageSprite2);
        UnityEngine.Object.DestroyImmediate(protectorSprite2);
        UnityEngine.Object.DestroyImmediate(stageSprite0);
        UnityEngine.Object.DestroyImmediate(stageSprite1);
        UnityEngine.Object.DestroyImmediate(stageSprite2);
        UnityEngine.Object.DestroyImmediate(stageCharacterSprite);

        // script gameobject
        UnityEngine.Object.DestroyImmediate(scriptsGameObject);
    }

    // -------------------------------------------------- Class Principal --------------------------------------------------

    //      Will be tested in Play Mode : 
    // public void leftArrow(InputAction.CallbackContext ctx)
    // public void rightArrow(InputAction.CallbackContext ctx)
    // public IEnumerator moveCharacter()

    [Test]
    public void TestCharacterSelection2(){
        // tests if the method saves the selected character

        scriptPrincipal.setCharacter1(0);
        scriptPrincipal.setCharacter2(1);
        scriptPrincipal.characterSelection2();
        Assert.AreEqual(new int[]{0, 1}, scriptPrincipal.getSelectedCharacters1());

        scriptPrincipal.setCharacter1(2);
        scriptPrincipal.setCharacter2(4);
        scriptPrincipal.characterSelection2();
        Assert.AreEqual(new int[]{2, 4}, scriptPrincipal.getSelectedCharacters1());

        scriptPrincipal.setCharacter1(3);
        scriptPrincipal.setCharacter2(0);
        scriptPrincipal.characterSelection2();
        Assert.AreEqual(new int[]{3, 0}, scriptPrincipal.getSelectedCharacters1());
    }

    [Test]
    public void TestStartPvPFight(){
        // tests if the method saves the selected characters

        scriptPrincipal.setCharacter1(0);
        scriptPrincipal.setCharacter2(1);
        scriptPrincipal.startPvPFight();
        Assert.AreEqual(new int[]{0, 1}, scriptPrincipal.getSelectedCharacters2());

        scriptPrincipal.setCharacter1(2);
        scriptPrincipal.setCharacter2(4);
        scriptPrincipal.startPvPFight();
        Assert.AreEqual(new int[]{2, 4}, scriptPrincipal.getSelectedCharacters2());

        scriptPrincipal.setCharacter1(3);
        scriptPrincipal.setCharacter2(0);
        scriptPrincipal.startPvPFight();
        Assert.AreEqual(new int[]{3, 0}, scriptPrincipal.getSelectedCharacters2());
    }

    [Test]
    public void TestStartPvMFight(){
        // tests if the method saves the selected characters

        scriptPrincipal.setCharacter1(0);
        scriptPrincipal.setCharacter2(1);
        scriptPrincipal.startPvMFight();
        Assert.AreEqual(new int[]{0, 1}, scriptPrincipal.getSelectedCharacters1());

        scriptPrincipal.setCharacter1(2);
        scriptPrincipal.setCharacter2(4);
        scriptPrincipal.startPvMFight();
        Assert.AreEqual(new int[]{2, 4}, scriptPrincipal.getSelectedCharacters1());

        scriptPrincipal.setCharacter1(3);
        scriptPrincipal.setCharacter2(0);
        scriptPrincipal.startPvMFight();
        Assert.AreEqual(new int[]{3, 0}, scriptPrincipal.getSelectedCharacters1());
    }

    [Test]
    public void TestSwitchCharacter1(){
        // tests if the method switches the characters 

        scriptPrincipal.switchCharacter1(0);
        Assert.IsTrue(characterSpriteList1[0].gameObject.activeSelf);

        scriptPrincipal.switchCharacter1(3);
        Assert.IsFalse(characterSpriteList1[0].gameObject.activeSelf);
        Assert.IsTrue(characterSpriteList1[3].gameObject.activeSelf);

        scriptPrincipal.switchCharacter1(1);
        Assert.IsFalse(characterSpriteList1[3].gameObject.activeSelf);
        Assert.IsTrue(characterSpriteList1[1].gameObject.activeSelf);

        scriptPrincipal.switchCharacter1(2);
        Assert.IsFalse(characterSpriteList1[1].gameObject.activeSelf);
        Assert.IsTrue(characterSpriteList1[2].gameObject.activeSelf);
    }

    [Test]
    public void TestSwitchCharacter2(){
        // tests if the method switches the characters 

        scriptPrincipal.switchCharacter2(0);
        Assert.IsTrue(characterSpriteList2[0].gameObject.activeSelf);

        scriptPrincipal.switchCharacter2(3);
        Assert.IsFalse(characterSpriteList2[0].gameObject.activeSelf);
        Assert.IsTrue(characterSpriteList2[3].gameObject.activeSelf);

        scriptPrincipal.switchCharacter2(1);
        Assert.IsFalse(characterSpriteList2[3].gameObject.activeSelf);
        Assert.IsTrue(characterSpriteList2[1].gameObject.activeSelf);

        scriptPrincipal.switchCharacter2(2);
        Assert.IsFalse(characterSpriteList2[1].gameObject.activeSelf);
        Assert.IsTrue(characterSpriteList2[2].gameObject.activeSelf);
    }


    // -------------------------------------------------- Class MenuButtons --------------------------------------------------

    [Test]
    public void TestCharacterButton1(){
        // tests if the correct character is switched

        scriptPrincipal.setCharacter1(0);
        scriptPrincipal.setCharacter2(1);

        // to the right
        scriptMenuButtons.characterButton1(false);
        Assert.AreEqual(2, scriptPrincipal.getCharacter1());

        scriptMenuButtons.characterButton1(false);
        Assert.AreEqual(3, scriptPrincipal.getCharacter1());

        scriptMenuButtons.characterButton1(false);
        Assert.AreEqual(0, scriptPrincipal.getCharacter1());

        scriptMenuButtons.characterButton1(false);
        Assert.AreEqual(2, scriptPrincipal.getCharacter1());

        scriptPrincipal.setCharacter2(0);

        // to the left
        scriptMenuButtons.characterButton1(true);
        Assert.AreEqual(1, scriptPrincipal.getCharacter1());

        scriptMenuButtons.characterButton1(true);
        Assert.AreEqual(3, scriptPrincipal.getCharacter1());

        scriptMenuButtons.characterButton1(true);
        Assert.AreEqual(2, scriptPrincipal.getCharacter1());

        scriptMenuButtons.characterButton1(true);
        Assert.AreEqual(1, scriptPrincipal.getCharacter1());
    }

    [Test]
    public void TestCharacterButton2(){
        // tests if the correct character is switched

        scriptPrincipal.setCharacter1(1);
        scriptPrincipal.setCharacter2(0);

        // to the right
        scriptMenuButtons.characterButton2(false);
        Assert.AreEqual(2, scriptPrincipal.getCharacter2());

        scriptMenuButtons.characterButton2(false);
        Assert.AreEqual(3, scriptPrincipal.getCharacter2());

        scriptMenuButtons.characterButton2(false);
        Assert.AreEqual(0, scriptPrincipal.getCharacter2());

        scriptMenuButtons.characterButton2(false);
        Assert.AreEqual(2, scriptPrincipal.getCharacter2());

        scriptPrincipal.setCharacter1(0);

        // to the left
        scriptMenuButtons.characterButton2(true);
        Assert.AreEqual(1, scriptPrincipal.getCharacter2());

        scriptMenuButtons.characterButton2(true);
        Assert.AreEqual(3, scriptPrincipal.getCharacter2());

        scriptMenuButtons.characterButton2(true);
        Assert.AreEqual(2, scriptPrincipal.getCharacter2());

        scriptMenuButtons.characterButton2(true);
        Assert.AreEqual(1, scriptPrincipal.getCharacter2());
    }
    /*

    public void characterButton1(bool left){
        ///<param> left : weither the button is the left arrow or not </param> 
        ///<summary> switches the character appearing on the left side during the character selection </summary>
        
        character1 = principalScript.getCharacter1();
        character1 = left ? (character1+3)%4 : (character1+1)%4; // chose the next character
        if (character1 == principalScript.getCharacter2()){ // if the character is already taken by the other slot
            character1 = left ? (character1+3)%4 : (character1+1)%4; // chose the next character again
        }
        principalScript.switchCharacter1(character1); // replace the old character with the new one
    }

    public void characterButton2(bool left){
        ///<param> left : weither the button is the left arrow or not </param> 
        ///<summary> switches the character appearing on the right side during the character selection </summary>
        
        character2 = principalScript.getCharacter2();
        character2 = left ? (character2+3)%4 : (character2+1)%4; // chose the next character
        if (character2 == principalScript.getCharacter1()){ // if the character is already taken by the other slot
            character2 = left ? (character2+3)%4 : (character2+1)%4; // chose the next character again
        }
        principalScript.switchCharacter2(character2); // replace the old character with the new one
    }
    */
}
