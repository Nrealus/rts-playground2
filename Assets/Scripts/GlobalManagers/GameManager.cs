using UnityEngine;

namespace GlobalManagers
{
    //this attribute is optional but recommended. It will allow you to create the singleton via the asset menu.
    //you need to put the created singleton asset on the Resources folder.
    [CreateAssetMenu(fileName = "GameManager", menuName = "Managers/Game Manager Data", order = 0)]
    public class GameManager : SingletonScriptableObject<GameManager>
    {

        [Header("Keys Map")]
        #region Keys Map

        public KeyCode zkey;
        public KeyCode skey;
        public KeyCode dkey;
        public KeyCode qkey;
        public KeyCode akey;
        public KeyCode ekey;
        public KeyCode fkey;
        public KeyCode rkey;
        public KeyCode lshiftkey;
        public KeyCode laltkey;
        public KeyCode lctrlkey;
        public KeyCode alpha0key;
        public KeyCode alpha1key;
        public KeyCode alpha2key;
        public KeyCode alpha3key;
        public KeyCode alpha4key;
        public KeyCode alpha5key;
        public KeyCode alpha6key;
        public KeyCode alpha7key;
        public KeyCode alpha8key;
        public KeyCode alpha9key;
        
        #endregion

        [Header("Mouse Settings")]
        #region Mouse Settings

        public int lmb;
        public int mmb;
        public int rmb;

        [Tooltip("Double key press time window (in seconds)")]
        public float doublePressMaxTime;
        [Tooltip("Double mouse click time window (in seconds)")]
        public float doubleClickMaxTime;
        
        #endregion

        [HideInInspector] public Camera currentMainCamera;
        [HideInInspector] public Canvas currentMainCanvas;
        //[HideInInspector] public InputManager IM;
        [HideInInspector] public Core.Handlers.MainHandler currentMainHandler;

        //optional (but recommended)
        //this method will run before the first scene is loaded. Initializing the singleton here
        //will allow it to be ready before any other GameObjects on every scene and will
        //will prevent the "initialization on first usage". 
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void BeforeSceneLoad() { BuildSingletonInstance(); }

        //optional,
        //will run when the Singleton Scriptable Object is first created on the assets. 
        //Usually this happens on edit mode, not runtime. (the override keyword is mandatory for this to work)
        public override void ScriptableObjectAwake()
        {
            Debug.Log(GetType().Name + " created.");
        }

        //optional,
        //will run when the associated MonoBehavioir awakes. (the override keyword is mandatory for this to work)
        public override void MonoBehaviourAwake()
        {
            Debug.Log(GetType().Name + " behaviour awake.");

            //IM = Behaviour.gameObject.AddComponent<InputManager>();
            //IM.Init();

            currentMainCamera = FindObjectOfType<Camera>();
            currentMainCanvas = FindObjectOfType<Canvas>();
            currentMainHandler = FindObjectOfType<Core.Handlers.MainHandler>();

            currentMainHandler.Init();

            //A coroutine example:
            //Singleton Objects do not have coroutines.
            //if you need to use coroutines use the atached MonoBehaviour
            //Behaviour.StartCoroutine(SimpleCoroutine());
        }

        //any methods as usual
        /*private IEnumerator SimpleCoroutine()
        {
            while (true)
            {
                Debug.Log(GetType().Name + " coroutine step.");
                yield return new WaitForSeconds(3);
            }
        }*/

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        private void OnLevelFinishedLoading(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            //Debug.Log("Level Loaded");
            //Debug.Log(scene.name);
            //Debug.Log(mode);

            /*
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "AssetBundles/uibundle"));
            if (myLoadedAssetBundle == null) {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            */

        }

        //optional,
        //Classic runtime Update method (the override keyword is mandatory for this to work).
        public override void Update()
        {

        }

        //optional,
        //Classic runtime FixedUpdate method (the override keyword is mandatory for this to work).
        public override void FixedUpdate()
        {

        }
    }
}
/*
*  Notes:
*  - Remember that you have to create the singleton asset on edit mode before using it. You have to put it on the Resources folder and of course it should be only one. 
*  - Like other Unity Singleton this one is accessible anywhere in your code using the "Instance" property i.e: GameManager.Instance
*/
