using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace alpoLib.Res
{
    public static class AddressableLoader
    {
        public static async Awaitable<Scene> LoadSceneAsync(string sceneName)
        {
            var handle = Addressables.LoadSceneAsync(sceneName);
            await handle.Wait();
            var si = handle.Result;
            await si.ActivateAsync();
            return handle.Result.Scene;
        }
    }
}