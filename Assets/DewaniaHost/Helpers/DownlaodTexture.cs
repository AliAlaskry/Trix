
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalStore
{
    // get and store images
    public static class DownlaodTexture
    {
        // store max redirect requests count
        public const int MaxRedirectRequestCount = 3;
        // store max requests timeOut
        public const int MaxRequestTimeOut = 3;

        // download image from external source 
        public static IEnumerator Download(string URL, Action<Texture2D> onSuccess,
            Action onFail)
        {
            Uri uri = new(URL);

            using (UnityWebRequest request = UnityWebRequest.Get(URL))
            {
                request.disposeCertificateHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                request.disposeUploadHandlerOnDispose = true;

                request.redirectLimit = MaxRedirectRequestCount;

                request.timeout = MaxRequestTimeOut;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success && request.error == null)
                {
                    if (request.downloadHandler.IsNullData())
                    {
                        // notify
                    }
                    else
                    {
                        byte[] data = request.downloadHandler.data;

                        Texture2D texture = new(120, 120);
                        texture.LoadImage(data);
                        onSuccess?.Invoke(texture);
                    }
                }
                else
                {
                    onFail?.Invoke();
                    Debugging.Print($"failed to download image without extension with result {request.result} due to {request.error}");
                }
            }
        }

        static bool IsNullData(this DownloadHandler downloadHandler)
        {
            if (downloadHandler == null) return true;
            if (downloadHandler.data == null) return true;
            return false;
        }

        public static Texture2D ToTexture2D(this Texture texture)
        {
            if (texture == null) return null;

            return Texture2D.CreateExternalTexture(texture.width, texture.height, TextureFormat.RGB24,
                false, false, texture.GetNativeTexturePtr());
        }
    }
}