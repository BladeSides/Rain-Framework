using UnityEngine;

namespace RainFramework.Utilities
{
    public abstract class SingletonPersistent<T> : MonoBehaviour where T : Component
    {
        // Class for having singletons that persist across scenes.
        // Written for RainFramework by BladeSides

        #region Fields

        /// <summary>
        /// The instance.
        /// </summary>
        protected static T instance;

        private static bool applicationQuitting;

        private static object _lock;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (applicationQuitting)
                    return null;

                if (instance == null)
                {
                    // Lock it to a single thread so nothing else can access object
                    // Makes sure the code is safe and can't be interfered with.
                    _lock = new();
                    lock (_lock)
                    {
                        instance = FindObjectOfType<T>();
                        if (instance == null)
                        {
                            GameObject obj = new GameObject(typeof(T).Name);
                            instance = obj.AddComponent<T>();
                        }
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        protected virtual void Awake()
        {
            applicationQuitting = false;
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            applicationQuitting = true;
        }

        #endregion

    }
}