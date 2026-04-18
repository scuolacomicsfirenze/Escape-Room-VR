using DesignPatterns.Generics;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Tooltip("Espresso in secondi")]
    ///<summary>
    ///Espresso in secondi
    ///</summary>
    [SerializeField] float gameTimer = 1200;
    [SerializeField] SimpleTimerWithEvent timerWithEvent;

    private void Start()
    {
        timerWithEvent.StartTimer(gameTimer);
    }

}
