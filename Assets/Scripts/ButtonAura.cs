using UnityEngine;

public class ButtonAura : MonoBehaviour
{
    [SerializeField] private MenuButton menuButton;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Entered Aura");
            menuButton.OnAuraEnter();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Exited Aura");
            menuButton.OnAuraExit();
        }
    }
}
