using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class UnitExpPanel : MonoBehaviour
{
    [SerializeField] RawImage unitIcon;
    [SerializeField] TextMeshProUGUI unitName;
    [SerializeField] TextMeshProUGUI levelNumber;
    [SerializeField] TextMeshProUGUI Dead;
    [SerializeField] Image health;
    [SerializeField] Image exp;
    [SerializeField] UnitDTO unitDTO;
    [SerializeField] Color deadcolor = new Color(255f, 100f, 100f, 255f);
    bool dead = false;

    int excessExp = 0;

    List<Tweener> t =  new List<Tweener>();
    TweenCallback tweenCallback;

    public void LoadUnitExpPanel(UnitDTO _unitDTO,Texture texture) 
    { 
        unitIcon.texture = texture; 
        unitName.text = _unitDTO.name;
        levelNumber.text = _unitDTO.level.ToString();

        if(_unitDTO.currentHealthPrecent == 0)
        {
            Dead.gameObject.SetActive(true);
            dead = true;

            unitIcon.color = deadcolor;

            t.Add(Dead.transform.DOPunchPosition(Vector3.up, 1f, 5, .5f, false).SetLoops(-1, LoopType.Restart));
        }
        health.fillAmount = _unitDTO.currentHealthPrecent;
        exp.fillAmount = _unitDTO.currentExpPrecent;
        unitDTO = _unitDTO;

        //BattleEndController.onExpReward += IncreaseExp;

    }

    private void OnDestroy()
    {
        //BattleEndController.onExpReward -= IncreaseExp;
        foreach(var l in t)
        {
            l.Kill();
        }
    }

    //public void IncreaseExp(int e)
    //{
    //    int x = e;
    //    if(unitDTO.currentHealthPrecent == 0)
    //    {
    //        x = Mathf.RoundToInt( e / 2);
    //    }
        
    //    if(unitDTO.level == 100)
    //    {
    //        //dosomething
    //    }
    //    else
    //    {
    //        int l = GetLevelUpExp();

    //        float t = (unitDTO.currentExpPrecent * l);

    //        //Debug.Log("Exp increase " + x + " current exp " + t);

    //        if (x + unitDTO.currentExpPrecent * l > l)
    //        {
    //            LevelUp(Mathf.RoundToInt( x + (unitDTO.currentExpPrecent * l) - l));
    //        }
    //        else
    //        {
    //            increaseEXPCall(x);
    //        }
    //    }
    //}

    //public int GetLevelUpExp()
    //{
    //    int x = 420;

    //    return Mathf.RoundToInt(x * ((1.5f * unitDTO.level) + (2 * unitDTO.rank)));
    //}

    //void LevelUp(int x)
    //{
    //    Tweener a = exp.DOFillAmount(1, .5f);
    //    tweenCallback = a.onComplete += LevelUPVisuals;
    //    t.Add(a);
    //    excessExp = x;
    //}

    //void LevelUPVisuals()
    //{
    //    unitDTO.level++;
    //    levelNumber.text = unitDTO.level.ToString();    

    //    float e = GetLevelUpExp();

    //    float f = (excessExp / e);

    //    //Debug.Log("Exp Growth " + excessExp + " exp to level up " + e + " fill amount " + f);

    //    t.Add(levelNumber.transform.DOPunchPosition(Vector3.up, .2f, 5, .5f, false).SetLoops(-1, LoopType.Restart));

    //    t.Add(exp.DOFillAmount(unitDTO.currentExpPrecent + f, .5f));

    //    unitDTO.currentExpPrecent += f;
    //    tweenCallback -= LevelUPVisuals;
    //}

    //void increaseEXPCall(int x)
    //{   
    //    float e = GetLevelUpExp();

    //    float f = (x / e);

    //    //Debug.Log("x is " + x + " exp to level up " + e + " fill amount " + f);

    //    t.Add(exp.DOFillAmount(unitDTO.currentExpPrecent + f, .5f));

    //    unitDTO.currentExpPrecent += f;
    //}
}
