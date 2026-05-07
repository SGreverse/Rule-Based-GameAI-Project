using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerPosScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _debugText;

    [Tooltip("Drag the Player GameObject here")]
    [SerializeField] private PlayerManager _player;


    void Update()
    {

        // 1. Get the physical world position
        Vector3 playerPos = _player.transform.position;

        // 2. Get the room index from your Manager
        Vector2Int roomIndex = GameManager.Instance.currentPlayerRoomIndex;

        if (_debugText != null)
        {
            _debugText.text = $"<b>Player Pos:</b> ({playerPos.x:F2}, {playerPos.y:F2}) <b>Room Index:</b> ({roomIndex.x}, {roomIndex.y})";
        }

    }
}


