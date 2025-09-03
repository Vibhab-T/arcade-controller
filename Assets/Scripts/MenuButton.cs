using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform button;
    [SerializeField] private Renderer buttonRenderer;

    [Header("Visual Settings")]
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField] private float growScale = 1.2f;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private TextMeshPro labelText;
    [SerializeField] private string label;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Scene")]
    [SerializeField] private string sceneToLoad;

    private Vector3 originalScale;
    private bool isHovered = false;
    private Material buttonMaterial;

    private void Start()
    {
        originalScale = button.localScale;

        //material
        buttonMaterial = buttonRenderer.material;
        SetBaseColor();

        if (labelText != null)
        {
            labelText.text = label;
            labelText.gameObject.SetActive(false);
        }
        if(targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    { // Scale transition
        Vector3 targetScale = isHovered ? originalScale * growScale : originalScale;
        button.localScale = Vector3.Lerp(button.localScale, targetScale, Time.deltaTime * transitionSpeed);

        // Color and Emission transition
        Color targetBaseColor = isHovered ? glowColor : baseColor;
        buttonMaterial.color = Color.Lerp(buttonMaterial.color, targetBaseColor, Time.deltaTime * transitionSpeed);

        Color currentEmission = buttonMaterial.GetColor("_EmissionColor");
        Color targetEmission = isHovered ? glowColor * 5f : Color.black; // 5x intensity for bloom
        buttonMaterial.SetColor("_EmissionColor", Color.Lerp(currentEmission, targetEmission, Time.deltaTime * transitionSpeed));

        if (labelText != null && labelText.gameObject.activeSelf && targetCamera != null)
        {
            Vector3 camPosition = targetCamera.transform.position;
            Vector3 direction = labelText.transform.position - camPosition;
            direction.y = 0; // Keep text upright

            if (direction != Vector3.zero)
                labelText.transform.rotation = Quaternion.LookRotation(direction);
        }

        if(isHovered && Input.GetKeyDown(KeyCode.Return))
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("No Scene Assigned");
            }
        }
    }

    private void SetBaseColor()
    {
        buttonMaterial.color = baseColor;
        buttonMaterial.SetColor("_EmissionColor", Color.black);
    }

    public void OnAuraEnter()
    {
        isHovered = true;

        if (labelText != null)
        {
            labelText.gameObject.SetActive(true);
        }
    }

    public void OnAuraExit()
    {
        isHovered = false;

        if (labelText != null)
        {
            labelText.gameObject.SetActive(false);
        }
    }
}
