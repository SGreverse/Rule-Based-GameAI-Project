using System.Runtime.CompilerServices;
using Assets.Algorithm.BlackBoard;
using Assets.Algorithm.Data;
using Assets.Algorithm.Deque;
using UnityEngine;

public class BlackBoardScript : MonoBehaviour
{
    private const float TIME_UNTIL_INFO_RESET = 15f;
    private const float TIME_UNTIL_PLAYER_ENVIRONMENTAL_AWERNESS_RESET= 3f;
    private const float TIME_UNTIL_PLAYER_ATTACK_COUNT_RESET= 5f;

    private GameBlackboard _blackbaordInstance;

    public CombatSettingsSO EnemiesCombatSettings;
    void Start()
    {
        _blackbaordInstance = GameBlackboard.Instance;
        _blackbaordInstance.CombatSettings = this.EnemiesCombatSettings;
    }
    // the update loop ensures outdated info is discarded
    void Update()
    {
        float InfoTimeStamp = _blackbaordInstance.ReadTimeStamp(EnvironmentKey.PlayerDetected);
        if (Time.time - InfoTimeStamp >= TIME_UNTIL_INFO_RESET)
        {
            _blackbaordInstance.PlayerIsUnkown();
        }
        
        BlackboardData Event=_blackbaordInstance.ReadFirstRecordedEvent(EnvironmentKey.PlayerDirectionChange);

        // must always have atleast 1 direction
        while (Event != null &&
            _blackbaordInstance.ReadData<GameEventDeque<BlackboardData>>(EnvironmentKey.PlayerDirectionChange).Count>1 && 
            Time.time-Event.Timestamp>= TIME_UNTIL_PLAYER_ENVIRONMENTAL_AWERNESS_RESET )
        {
            _blackbaordInstance.RemoveEvent(EnvironmentKey.PlayerDirectionChange);
            Event = _blackbaordInstance.ReadFirstRecordedEvent(EnvironmentKey.PlayerDirectionChange);
        }

        Event = _blackbaordInstance.ReadFirstRecordedEvent(EnvironmentKey.PlayerAmountOfAttacks);

        while (Event != null && Time.time - Event.Timestamp >= TIME_UNTIL_PLAYER_ATTACK_COUNT_RESET)
        {
            _blackbaordInstance.RemoveEvent(EnvironmentKey.PlayerAmountOfAttacks);
            Event = _blackbaordInstance.ReadFirstRecordedEvent(EnvironmentKey.PlayerAmountOfAttacks);
        }

        //clean expired node reservations.
        if (GameBlackboard.Instance != null && GameBlackboard.Instance.GlobalPathFinder.ReservationTable != null)
        {
            // Constantly delete reservations that are in the past
            GameBlackboard.Instance.GlobalPathFinder.ReservationTable.CleanupExpiredReservations(Time.time);
        }
    }

}
