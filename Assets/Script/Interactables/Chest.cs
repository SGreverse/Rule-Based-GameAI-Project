using System;
using Assets.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public class StringEvent : UnityEvent<string> { }
public class Chest : MonoBehaviour
{
    public string ChestID => this.name;

    [Header("UI")]
    [SerializeField] private GameObject promptUI;

    [Header("Items")]
    [Tooltip("The item inside this chest")]
    public ItemData LootInside; //optional gear
    public int Potions;         //potions
    public int Projectiles;     //projectiles

    [Header("Visuals")]
    public SpriteRenderer chestSpriteRenderer;
    public Sprite closedSprite;
    public Sprite openedSprite;

    [Header("OpenEvent")]
    public StringEvent OnChestOpened;

    private bool _isOpened = false;

    private bool _isPlayerInRange = false;

    private PlayerManager _player;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.OpenedChestIDs.Contains(ChestID))
        {
            SetChestOpenedState();
        }
        else
        {
            chestSpriteRenderer.sprite = closedSprite;
        }
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }
    private void Update()
    {

        if (_isPlayerInRange && _player != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenChest();
            HidePrompt();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isOpened && collision.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            if (promptUI != null)
            {
                _player= collision.GetComponent<PlayerManager>();
                promptUI.SetActive(true); // Show the interaction prompt
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the Player left the trigger area
        if (!_isOpened && other.CompareTag("Player"))
        {
            HidePrompt();
        }
    }

    private void HidePrompt()
    {
        _isPlayerInRange = false;
        if (promptUI != null)
        {
            promptUI.SetActive(false); // Hide the prompt
        }
    }
    private void OpenChest()
    {
        string itemsText = "";

        if (LootInside != null)
        {
            itemsText += $"+{this.LootInside.ItemName}\n";
            _player.GetBrain().Equipment.EquipBetterItem(this.LootInside);
        }

        if (Potions > 0)
        {
            itemsText += $"+{Potions} Potions\n";
            _player.GetBrain().PotionAmount += this.Potions;
        }

        if (Projectiles > 0)
        {
            itemsText += $"+{Projectiles} Projectiles\n";
            _player.GetBrain().ProjectileAmount += this.Projectiles; // Assuming you have this!
        }
        if (itemsText == "")
        {
            itemsText = "Chest Was Empty";
        }
        GameManager.Instance.RegisterOpenedChest(ChestID);

        // This will now perfectly pass the itemsText string to the UI
        OnChestOpened?.Invoke(itemsText);

        SetChestOpenedState();
    }

    private void SetChestOpenedState()
    {
        _isOpened = true;
        chestSpriteRenderer.sprite = openedSprite;
        GetComponent<Collider2D>().enabled = false;
    }
}
