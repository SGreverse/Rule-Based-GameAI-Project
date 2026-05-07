using System.Collections;
using TMPro;
using UnityEngine;

public class ItemObtainScript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupContainer;
    public TextMeshProUGUI itemText;

    // NEW: The CanvasGroup handles fading the UI panel and text together
    private CanvasGroup _canvasGroup;

    [Header("Settings")]
    public float displayDuration = 2.5f; // How long it stays fully visible
    public float fadeDuration = 1.0f;    // How long the actual fade-out takes

    void Awake()
    {
        // Automatically grab (or add) the CanvasGroup to your popup panel
        if (popupContainer != null)
        {
            _canvasGroup = popupContainer.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = popupContainer.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        popupContainer.SetActive(false);
    }

    //called by an event
    public void ShowItemPopup(string items)
    {
        StopAllCoroutines();

        itemText.text = items;

        // Reset transparency to 100% visible and turn it on
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        popupContainer.SetActive(true);

        StartCoroutine(HidePopupRoutine());
    }

    private IEnumerator HidePopupRoutine()
    {
        yield return new WaitForSeconds(displayDuration);

        if (_canvasGroup != null)
        {
            float timeElapsed = 0f;

            while (timeElapsed < fadeDuration)
            {
                timeElapsed += Time.deltaTime;

                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);

                yield return null;
            }

            _canvasGroup.alpha = 0f;
        }

        popupContainer.SetActive(false);
    }
}
