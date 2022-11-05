using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Stemuli
{
    public class AddressablesManager : MonoBehaviour
    {
        public bool clearCache = false;
        public int numDownloaded;
        public int numAssetBundlesToDownload;

        private async void Start()
        {
            if (clearCache)
            {
                Caching.ClearCache();
            }

            await Addressables.InitializeAsync();

            List<string> possibleUpdates = await Addressables.CheckForCatalogUpdates();

            if(possibleUpdates.Count > 0)
            {
                Debug.Log("Update avaliable");
                UpdateAndDownload();
            }
            else
            {
                Debug.Log("No update avaliable");
            }

            SceneManager.LoadScene("Login");
        }

        private float BytesToKiloBytes(long bytes)
        {
            return bytes / 1024f;
        }

        private async void UpdateAndDownload()
        {
            List<IResourceLocator> updatedResourceLocators = await Addressables.UpdateCatalogs(true);

            if (updatedResourceLocators != null)
            {
                var allKeys = updatedResourceLocators[0].Keys;

                for (int i = 1; i < updatedResourceLocators.Count; i++)
                {
                    allKeys.Append(updatedResourceLocators[i].Keys);
                }

                numAssetBundlesToDownload = allKeys.ToList().Count;
                numDownloaded = 0;

                foreach (var key in allKeys)
                {
                    numDownloaded++;

                    //if (ui != null)
                    //{
                    //    ui.UpdateDownloadsText($"downloading ... {numDownloaded}/{numAssetBundlesToDownload}");
                    //}


                    var keyDownloadSizeKB = BytesToKiloBytes(await Addressables.GetDownloadSizeAsync(key).Task);
                    if (keyDownloadSizeKB <= 0)
                    {
                        continue;
                    }

                    var keyDownloadOperation = Addressables.DownloadDependenciesAsync(key);
                    while (!keyDownloadOperation.IsDone)
                    {
                        await Task.Yield();
                        var status = keyDownloadOperation.GetDownloadStatus();
                        var percent = status.Percent * 100.0f;

                        //if (ui != null)
                        //{
                        //    ui.UpdateProgressText(percent);
                        //}
                    }
                }
            }
        }
    }
}
