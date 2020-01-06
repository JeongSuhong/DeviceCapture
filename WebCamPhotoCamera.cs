using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class WebCamPhotoCamera : MonoBehaviour
{
    public RawImage rawImage;

private	WebCamTexture webCamTexture;
    private Quaternion baseRotation;

    private void Start()
	{
        WebCamDevice[] camDevices = WebCamTexture.devices;
        webCamTexture = new WebCamTexture(camDevices[0].name);
        rawImage.texture = webCamTexture;
        rawImage.material.mainTexture = webCamTexture;

        baseRotation = rawImage.transform.rotation;

        webCamTexture.Play();
    }

    void Update()
    {
        rawImage.transform.rotation = baseRotation * Quaternion.AngleAxis(webCamTexture.videoRotationAngle, Vector3.back);
    }

    private IEnumerator CoTakePhoto()
	{
        yield return new WaitForEndOfFrame();

        Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.ARGB32, false);
        Texture2D rotateTex;

        texture.SetPixels(webCamTexture.GetPixels());
       
        texture.Apply();

        if (webCamTexture.videoRotationAngle == 180)
            rotateTex = rotateTexture(texture, false, true);
        else if (webCamTexture.videoRotationAngle == 90)
            rotateTex = rotateTexture(texture, true);
        else
            rotateTex = texture;

        byte[] bytes = rotateTex.EncodeToPNG();

#if UNITY_EDITOR
        File.WriteAllBytes(Application.dataPath + "/photo.png", bytes);
#elif UNITY_ANDROID
        File.WriteAllBytes(Application.persistentDataPath + "/photo.png", bytes);
#endif
    }

    private Texture2D rotateTexture(Texture2D originalTexture, bool clockwise, bool isFlip = false)
    {
        Color32[] originalPixel = originalTexture.GetPixels32();
        Color32[] rotatedPixel = new Color32[originalPixel.Length];
        int width = originalTexture.width;
        int height = originalTexture.height;
        Texture2D rotatedTexture;

        if (isFlip)
        {
            for (int i = 0; i < originalPixel.Length; i++)
                rotatedPixel[originalPixel.Length - 1 - i] = originalPixel[i];

            rotatedTexture = new Texture2D(width, height);
        }
        else
        {
            int rotated, original;

            for (int j = 0; j < height; ++j)
            {
                for (int i = 0; i < width; ++i)
                {

                    rotated = (i + 1) * height - j - 1;
                    original = clockwise ? originalPixel.Length - 1 - (j * width + i) : j * width + i;      // 시계방향 or 역방향으로 90도 회전
                    rotatedPixel[rotated] = originalPixel[original];
                }
            }

            rotatedTexture = new Texture2D(height, width);
        }

        rotatedTexture.SetPixels32(rotatedPixel);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    public void OnCaptureForWebCam()
    {
        StartCoroutine(CoTakePhoto());
    }


}
