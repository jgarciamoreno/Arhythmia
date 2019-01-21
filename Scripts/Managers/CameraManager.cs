using UnityEngine;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private List<Camera> playerCameras = new List<Camera>();

    public void SetCameras(bool resetting)
    {
        if (resetting)
            return;

        foreach (Camera c in FindObjectsOfType<Camera>())
            if (c.tag == "PlayerCam")
                playerCameras.Add(c);

        playerCameras.Reverse();

        AdjustCameraViewPort();
    }

    private void AdjustCameraViewPort()
    {
        if (!GameManager.INSTANCE.multiMonitor)
        {
            switch (playerCameras.Count)
            {
                case 1:
                    playerCameras[0].rect = new Rect(0, 0, 1, 1);
                    break;
                case 2:
                    playerCameras[0].rect = new Rect(-0.5f, 0, 1, 1);
                    playerCameras[1].rect = new Rect(0.5f, 0, 1, 1);
                    break;
                case 3:
                    playerCameras[0].rect = new Rect(-0.5f, 0, 1, 1);
                    playerCameras[1].rect = new Rect(0.5f, 0.5f, 1, 1);
                    playerCameras[2].rect = new Rect(0.5f, -0.5f, 1, 1);
                    break;
                case 4:
                    playerCameras[0].rect = new Rect(-0.5f, 0.5f, 1, 1);
                    playerCameras[1].rect = new Rect(0.5f, 0.5f, 1, 1);
                    playerCameras[2].rect = new Rect(-0.5f, -0.5f, 1, 1);
                    playerCameras[3].rect = new Rect(0.5f, -0.5f, 1, 1);
                    break;
            }

            foreach (Camera c in playerCameras)
                c.targetDisplay = 0;
        }
        else
        {
            if (Display.displays.Length > 1)
                Display.displays[1].Activate();

            playerCameras[0].targetDisplay = 0;
            playerCameras[1].targetDisplay = 1;
        }
    }

    public void ToMainMenu()
    {
        foreach (Camera cam in playerCameras)
            Destroy(cam.transform.parent.parent.gameObject);

        playerCameras.Clear();
    }
}