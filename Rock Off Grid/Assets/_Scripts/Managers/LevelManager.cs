using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [SerializeField] private GameObject _loaderCanvas;
    [SerializeField] private Slider _progreesBar;

    private bool loading = false;
    private float _target;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    public async void LoadScene(string sceneName)
    {
        loading = true;
        _target = 0;
        _progreesBar.value = 0;

        MusicManager.instance.StopMusic();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            GameManager.instance.ChangeGameState(GameState.Playing);
        }
        else
        {
            var scene = SceneManager.LoadSceneAsync(sceneName);
            scene.allowSceneActivation = false;
            _loaderCanvas.SetActive(true);

            do
            {
                await Task.Delay(1000);
                _target = scene.progress;

            } while (scene.progress < 0.9f);

            loading = false;
            scene.allowSceneActivation = true;
            _loaderCanvas.SetActive(false);

            GameManager.instance.ChangeGameState(GameState.Playing);
        }
    }

    private void Update()
    {
        if (!loading) return;
        _progreesBar.value = Mathf.MoveTowards(_progreesBar.value, _target, 3 * Time.deltaTime);
    }
}
