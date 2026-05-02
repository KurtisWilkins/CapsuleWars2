using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInventory : MonoBehaviour
{
    [SerializeField] public SpriteRenderer shadow;
    [SerializeField] public SpriteRenderer r_Sheild;
    [SerializeField] public SpriteRenderer backSprite;
    [SerializeField] public SpriteRenderer tailSprite;
    [SerializeField] public SpriteRenderer r_Shoulder;
    [SerializeField] public SpriteRenderer r_Arm;
    [SerializeField] public SpriteRenderer r_Weapon;
    [SerializeField] public SpriteRenderer r_ClothArm;
    [SerializeField] public SpriteRenderer r_Foot;
    [SerializeField] public SpriteRenderer l_Foot;
    [SerializeField] public SpriteRenderer r_ClothFoot;
    [SerializeField] public SpriteRenderer l_ClothFoot;
    [SerializeField] public SpriteRenderer body;
    [SerializeField] public SpriteRenderer clothBody;
    [SerializeField] public SpriteRenderer bodyArmor;
    [SerializeField] public SpriteRenderer head;
    [SerializeField] public SpriteRenderer hair;
    [SerializeField] public SpriteRenderer l_EyeBack;
    [SerializeField] public SpriteRenderer l_EyeFront;
    [SerializeField] public SpriteRenderer r_EyeBack;
    [SerializeField] public SpriteRenderer r_EyeFront;
    [SerializeField] public SpriteRenderer faceHair;
    [SerializeField] public SpriteRenderer l_Weapon;
    [SerializeField] public SpriteRenderer l_Arm;
    [SerializeField] public SpriteRenderer l_ClothArm;
    [SerializeField] public SpriteRenderer l_Shoulder;
    [SerializeField] public SpriteRenderer l_Helmet1;
    [SerializeField] public SpriteRenderer l_Helmet2;
    [SerializeField] public SpriteRenderer l_Sheild;

    Equipment helmet = new Equipment();
    Equipment shoulder = new Equipment();
    Equipment chest = new Equipment();
    Equipment back = new Equipment();
    Equipment arms = new Equipment();
    Equipment legs = new Equipment();
    Equipment rightHand = new Equipment();
    Equipment leftHand = new Equipment();

    public void LoadUnitInventory(UnitDTO unitDTO, Database database)
    {   
        UnitBody_SO unitBodySO = database.unitBodys[unitDTO.bodyID];

        r_Arm.sprite = unitBodySO.arm_R[unitDTO.evolution];
        r_Arm.color = unitBodySO.color;

        l_Arm.sprite = unitBodySO.arm_L[unitDTO.evolution];
        l_Arm.color = unitBodySO.color;

        r_Foot.sprite = unitBodySO.foot_R[unitDTO.evolution];
        r_Foot.color = unitBodySO.color;

        l_Foot.sprite = unitBodySO.foot_L[unitDTO.evolution];
        l_Foot.color = unitBodySO.color;

        body.sprite = unitBodySO.body[unitDTO.evolution];
        body.color = unitBodySO.color;

        head.sprite = unitBodySO.head[unitDTO.evolution];
        head.color = unitBodySO.color;

        tailSprite.sprite = unitBodySO.tail[unitDTO.evolution];
        tailSprite.color = unitBodySO.color;

        UnitEye_SO unitEye_SO = database.unitEyes[unitDTO.eyesID];

        l_EyeBack.sprite = unitEye_SO.l_BackEye;
        l_EyeBack.color = unitEye_SO.l_BackEyeColor;

        l_EyeFront.sprite = unitEye_SO.l_FrontEye;
        l_EyeFront.color = unitEye_SO.l_FrontEyeColor;

        r_EyeBack.sprite = unitEye_SO.r_BackEye;
        r_EyeBack.color = unitEye_SO.r_BackEyeColor;

        r_EyeFront.sprite = unitEye_SO.r_FrontEye;
        r_EyeFront.color = unitEye_SO.r_FrontEyeColor;

        UnitHair_SO unitHair_SO = database.unitHair[unitDTO.hairID];

        hair.sprite = unitHair_SO.hair;
        hair.color = unitHair_SO.hairColor;

        if (unitDTO.helmet.equiptmentSOID != 0)
        {
            helmet = new Equipment(unitDTO.helmet,database);
            UnitEquipment_SO helment_SO = database.unitEquipment[unitDTO.helmet.equiptmentSOID];
            if (helment_SO.hasSprite)
            {
                l_Helmet1.sprite = helment_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                l_Helmet1.color = helment_SO.equipmentColor;

                if (helment_SO.coveredBody.coverHair)
                {
                    hair.color = new Color(255, 255, 255, 0);
                }
                if (helment_SO.coveredBody.coverFace)
                {
                    head.color = new Color(255, 255, 255, 0);
                }
            }
            else
            {
                l_Helmet1.color = new Color(255, 255, 255, 0);
            }
        }
        else
        {
            l_Helmet1.color = new Color(255, 255, 255, 0);
        }

        if (unitDTO.shoulders.equiptmentSOID != 0)
        {
            shoulder = new Equipment(unitDTO.shoulders, database);
            UnitEquipment_SO shoulder_SO = database.unitEquipment[unitDTO.shoulders.equiptmentSOID];
            if (shoulder_SO.hasSprite)
            {
                r_Shoulder.sprite = shoulder_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                r_Shoulder.color = shoulder_SO.equipmentColor;
                l_Shoulder.sprite = shoulder_SO.equipmentSpriteL[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                l_Shoulder.color = shoulder_SO.equipmentColor;
            }
            else
            {
                r_Shoulder.color = new Color(255, 255, 255, 0);
                l_Shoulder.color = new Color(255, 255, 255, 0);
            }
        }
        else
        {
            r_Shoulder.color = new Color(255, 255, 255, 0);
            l_Shoulder.color = new Color(255, 255, 255, 0);
        }

        if (unitDTO.chest.equiptmentSOID != 0)
        {
            chest = new Equipment(unitDTO.chest, database);
            UnitEquipment_SO chest_SO = database.unitEquipment[unitDTO.chest.equiptmentSOID];
            if (chest_SO.hasSprite)
            {
                clothBody.sprite = chest_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                clothBody.color = chest_SO.equipmentColor;
            }
            else
            {
                clothBody.color = new Color(255, 255, 255, 0);
            }
        }
        else
        {
            clothBody.color = new Color(255, 255, 255, 0);
        }

        if (unitDTO.back.equiptmentSOID != 0)
        {
            back = new Equipment(unitDTO.back, database);
            UnitEquipment_SO back_SO = database.unitEquipment[unitDTO.back.equiptmentSOID];
            if (back_SO.hasSprite)
            {
                backSprite.sprite = back_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                backSprite.color = back_SO.equipmentColor;
            }
            else
            {
                backSprite.color = new Color(255, 255, 255, 0);
            }
        }
        else
        {
            backSprite.color = new Color(255, 255, 255, 0);
        }

        if (unitDTO.arms.equiptmentSOID != 0)
        {
            arms = new Equipment(unitDTO.arms, database);
            UnitEquipment_SO arms_SO = database.unitEquipment[unitDTO.arms.equiptmentSOID];
            if (arms_SO.hasSprite)
            {
                r_ClothArm.sprite = arms_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                r_ClothArm.color = arms_SO.equipmentColor;
                l_ClothArm.sprite = arms_SO.equipmentSpriteL[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                l_ClothArm.color = arms_SO.equipmentColor;
            }
            else
            {
                r_ClothArm.color = new Color(255, 255, 255, 0);
                l_ClothArm.color = new Color(255, 255, 255, 0);
            }
        }
        else
        {
            r_ClothArm.color = new Color(255, 255, 255, 0);
            l_ClothArm.color = new Color(255, 255, 255, 0);
        }

        if (unitDTO.legs.equiptmentSOID != 0)
        {
            legs = new Equipment(unitDTO.legs, database);
            UnitEquipment_SO legs_SO = database.unitEquipment[unitDTO.legs.equiptmentSOID];
            if (legs_SO.hasSprite)
            {
                r_ClothFoot.sprite = legs_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                r_ClothFoot.color = legs_SO.equipmentColor;
                l_ClothFoot.sprite = legs_SO.equipmentSpriteL[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                l_ClothFoot.color = legs_SO.equipmentColor;
            }
            else
            {
                r_ClothFoot.color = new Color(255, 255, 255, 0);
                l_ClothFoot.color = new Color(255, 255, 255, 0);
            }
        }
        else
        {
            r_ClothFoot.color = new Color(255, 255, 255, 0);
            l_ClothFoot.color = new Color(255, 255, 255, 0);
        }

        if (unitDTO.rightHand.equiptmentSOID != 0)
        {
            rightHand = new Equipment(unitDTO.rightHand, database);
            UnitEquipment_SO rightHand_SO = database.unitEquipment[unitDTO.rightHand.equiptmentSOID];
            if (rightHand_SO.hasSprite)
            {
                if (rightHand_SO.itemType == ItemType.Sheild)
                {
                    r_Sheild.sprite = rightHand_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                    r_Sheild.color = rightHand_SO.equipmentColor;
                }
                else
                {
                    r_Weapon.sprite = rightHand_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                    r_Weapon.color = rightHand_SO.equipmentColor;
                }
            }
            else
            {
                r_Weapon.color = new Color(255, 255, 255, 0);
                r_Sheild.color = new Color(255, 255, 255, 0);
            }
        }
        else
        {
            r_Weapon.color = new Color(255, 255, 255, 0);
            r_Sheild.color = new Color(255, 255, 255, 0);
        }

        if (unitDTO.leftHand.equiptmentSOID != 0)
        {
            leftHand = new Equipment(unitDTO.leftHand, database);
            UnitEquipment_SO leftHand_SO = database.unitEquipment[unitDTO.leftHand.equiptmentSOID];
            if (leftHand_SO.hasSprite)
            {
                if (leftHand_SO.itemType == ItemType.Sheild)
                {
                    l_Sheild.sprite = leftHand_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                    l_Sheild.color = leftHand_SO.equipmentColor;
                }
                else
                {
                    l_Weapon.sprite = leftHand_SO.equipmentSpriteR[unitDTO.evolution].equipmentSprites[unitBodySO.raceID];
                    l_Weapon.color = leftHand_SO.equipmentColor;
                }
            }
            else
            {
                l_Weapon.color = new Color(255, 255, 255, 0);
                l_Sheild.color = new Color(255, 255, 255, 0);
            }
        }
        else
        {
            l_Weapon.color = new Color(255, 255, 255, 0);
            l_Sheild.color = new Color(255, 255, 255, 0);
        }




    }


    public int GetEquipmentAttack()
    {
        return helmet.attack + shoulder.attack + chest.attack + back.attack + arms.attack + legs.attack + rightHand.attack + leftHand.attack; 
    }

    public int GetEquipmentAttackElement()
    {
        return helmet.attackElement + shoulder.attackElement + chest.attackElement + back.attackElement + arms.attackElement + legs.attackElement + rightHand.attackElement + leftHand.attackElement;
    }

    public int GetEquipmentHealth()
    {
        return helmet.health + shoulder.health + chest.health + back.health + arms.health + legs.health + rightHand.health + leftHand.health;
    }

    public int GetEquipmentSpeed()
    {
        return helmet.speed + shoulder.speed + chest.speed + back.speed + arms.speed + legs.speed + rightHand.speed + leftHand.speed;
    }

    public int GetEquipmentDefense()
    {
        return helmet.defense + shoulder.defense + chest.defense + back.defense + arms.defense + legs.defense + rightHand.defense + leftHand.defense;
    }

    public int GetEquipmentDefenseElement()
    {
        return helmet.defenseElement + shoulder.defenseElement + chest.defenseElement + back.defenseElement + arms.defenseElement + legs.defenseElement + rightHand.defenseElement + leftHand.defenseElement;
    }

    public int GetEquipmentAccuracy()
    {
        return helmet.accuracy + shoulder.accuracy + chest.accuracy + back.accuracy + arms.accuracy + legs.accuracy + rightHand.accuracy + leftHand.accuracy;
    }

    public int GetEquipmentResistence()
    {
        return helmet.resistence + shoulder.resistence + chest.resistence + back.resistence + arms.resistence + legs.resistence + rightHand.resistence + leftHand.resistence;
    }

    public int GetEquipmentCritRate()
    {
        return helmet.critrate + shoulder.critrate + chest.critrate + back.critrate + arms.critrate + legs.critrate + rightHand.critrate + leftHand.critrate;
    }

    public int GetEquipmentCritDamage()
    {
        return helmet.critDamage + shoulder.critDamage + chest.critDamage + back.critDamage + arms.critDamage + legs.critDamage + rightHand.critDamage + leftHand.critDamage;
    }

    public int GetEquipmentmass()
    {
        return helmet.mass + shoulder.mass + chest.mass + back.mass + arms.mass + legs.mass + rightHand.mass + leftHand.mass;
    }
}
