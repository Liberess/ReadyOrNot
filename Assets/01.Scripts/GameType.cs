using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameType : MonoBehaviour
{
    [SerializeField] private GameTypes type;
    public GameTypes Type { get => type; }
}