using UnityEngine;

namespace USITools
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    public class USI_AddonServiceManager : MonoBehaviour
    {
        public static USI_AddonServiceManager Instance { get; private set; }

        public ServiceCollection ServiceCollection { get; private set; }
        public ServiceManager ServiceManager { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Setup dependency injection for USI services
            ServiceCollection = new ServiceCollection();
            ServiceCollection
                .AddSingletonService<PartThumbnailService>()
                .AddSingletonService<PrefabManager>()
                .AddSingletonService<ShipThumbnailService>()
                .AddSingletonService<TextureService>()
                .AddSingletonService<WindowManager>();

            ServiceManager = new ServiceManager(ServiceCollection);
        }
    }
}
