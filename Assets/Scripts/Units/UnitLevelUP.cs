using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLevelUP
{


    public int GetLevelUpExp(UnitDTO unitDTO)
    {
        int x = 420;

        return Mathf.RoundToInt(x * ((1.5f * unitDTO.level) + (2 * unitDTO.rank)));
    }

    public UnitDTO LevelUP(UnitDTO unitDTO,float expReward)
    {
        UnitDTO unitDTO1 = new UnitDTO();
        unitDTO1.level = unitDTO.level;
        unitDTO1.currentExpPrecent = unitDTO.currentExpPrecent;
        if (unitDTO.level <= 99)
        {
            float CurrentExp = GetLevelUpExp(unitDTO) * unitDTO.currentExpPrecent;
            
            if (expReward + CurrentExp > GetLevelUpExp(unitDTO))
            {
                unitDTO1.level++;
                float afterLevelUpExp = (expReward + CurrentExp) - GetLevelUpExp(unitDTO);
                if (afterLevelUpExp > GetLevelUpExp(unitDTO1))
                {
                    unitDTO1 = LevelUP(unitDTO1, afterLevelUpExp);
                }
            }
            else
            {
                unitDTO1.currentExpPrecent = (expReward + CurrentExp) / GetLevelUpExp(unitDTO);
            }
        }

        return unitDTO1;
    }
}
