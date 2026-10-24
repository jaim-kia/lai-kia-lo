using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State;
    public static event Action<GameState> OnGameStateChanged;

    [Header("Rooms")]
    [SerializeField] private string startRoomScene = "Room_01";
    [SerializeField] private string startEntranceId = "default";

    [Header("Transition")]
    [SerializeField] private ScreenFader fader;

    public string CurrentRoom { get; private set; }
    private bool isTransitioning;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // if you pressed Play with a room open next to this scene in the editor, use that room
        string preloaded = FindAlreadyLoadedRoom();
        StartCoroutine(TransitionRoutine(preloaded ?? startRoomScene, startEntranceId, null, 0f,
                                         isBoot: true, alreadyLoaded: preloaded != null));
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.Overworld:
                break;
            case GameState.Dialogue:
                break;
            case GameState.Transition:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }

    // transition to an entrance in a diff scene
    public void TransitionToEntrance(string sceneName, string entranceId, float verticalOffset = 0f)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName, entranceId, null, verticalOffset));
    }

    // respawn benches
    public void TransitionToPosition(string sceneName, Vector3 position)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName, null, position, 0f));
    }


    private IEnumerator TransitionRoutine(string sceneName, string entranceId, Vector3? explicitPos,
                                          float verticalOffset, bool isBoot = false, bool alreadyLoaded = false)
    {
        isTransitioning = true;
        UpdateGameState(GameState.Transition);
        PlayerController.Instance.SetFrozen(true);

        if (isBoot) fader.SetBlack();
        else yield return StartCoroutine(fader.FadeOut());

        CameraManager.instance.ResetForRoomChange();

        if (!alreadyLoaded)
        {
            // unload everything
            if (!string.IsNullOrEmpty(CurrentRoom))
                yield return SceneManager.UnloadSceneAsync(CurrentRoom);

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        Scene room = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(room); // Instantiate() (e.g. the bench) now lands in this room
        CurrentRoom = sceneName;

        yield return null;

        Vector3 spawn = explicitPos ?? ResolveEntrance(room, entranceId, verticalOffset);
        PlayerController.Instance.Teleport(spawn);

        if (!PlayerStats.Instance.HasRespawnPoint)
            PlayerStats.Instance.SetRespawnPoint(CurrentRoom, spawn);

        yield return null; // give the camera a couple of frames to snap onto the player
        yield return null;

        PlayerController.Instance.SetFrozen(false);
        yield return StartCoroutine(fader.FadeIn());

        isTransitioning = false;
        UpdateGameState(GameState.Overworld);
    }

    private Vector3 ResolveEntrance(Scene room, string entranceId, float verticalOffset)
    {
        foreach (var e in FindObjectsByType<RoomEntrance>(FindObjectsSortMode.None))
        {
            if (e.gameObject.scene == room && e.entranceId == entranceId)
                return e.GetSpawn(verticalOffset);
        }

        Debug.LogError($"No RoomEntrance '{entranceId}' found in scene '{room.name}'");
        return PlayerController.Instance.transform.position;
    }

    private string FindAlreadyLoadedRoom()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s != gameObject.scene) return s.name;
        }
        return null;
    }
}

public enum GameState
{
    Overworld,
    Dialogue,
    Transition
}