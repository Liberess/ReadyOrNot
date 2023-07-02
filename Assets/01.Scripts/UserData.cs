using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Health = 0,
    Speed,
    Stamina
}

[System.Serializable]
public class UserData
{
    // 유저 정보
    public string userName;
    public float mouseSensitivity;
    public int crosshairNum;

    // 총 관련 정보 (구매, 업글)
    public bool[] haveGuns = new bool[9];
    public int[] gunLevels = new int[9];
    public GunData[] gunDatas = new GunData[9];
    public GunData equipGunData;
    public int equipGunNum;

    // 스텟 관련 정보 (업글)
    public int[] statUpLevels = new int[3];

    // Animation
    public RuntimeAnimatorController[] animCtrls =
        new RuntimeAnimatorController[9];
}