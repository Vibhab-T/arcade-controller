using System.Runtime.CompilerServices;
using UnityEngine;

public class Quit : MonoBehaviour
{
    private void Awake()
    {
        QuitGame();
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
