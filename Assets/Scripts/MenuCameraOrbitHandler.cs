using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuCameraOrbitHandler : MonoBehaviour
{
    [SerializeField] float FadeDuration = 0.5f;
    [SerializeField] float OrbitDuration = 5.0f;
    [SerializeField] int RotationDegrees = 200;
    [SerializeField] Image PanelBackground;
    [HideInInspector] Camera Camera;
    void Start()
    {
        Camera = gameObject.GetComponent<Camera>();
        GameHandler.Instance.PlacePiecesRandomly();
        StartCoroutine(CinematicCycle());
    }

    private IEnumerator CinematicCycle()  {
        float timeElapsed = 0;
        RandomizeCameraSettingsAndPosition();

        while (true) {
            //yield return new WaitForSeconds(OrbitDuration);
            timeElapsed = 0;
            while (timeElapsed < OrbitDuration) {
                transform.RotateAround(Vector3.zero, Vector3.up, RotationDegrees/OrbitDuration * Time.deltaTime);
                transform.LookAt(Vector3.zero);
                timeElapsed += Time.deltaTime;
                yield return null;
            }            

            StartCoroutine(ScreenFadeMagic());
        }
    }

    private IEnumerator ScreenFadeMagic() {
        float timeElapsed = 0;
        while (timeElapsed < FadeDuration) {
            PanelBackground.color = new Color(r: 0, g: 0, b: 0, a: timeElapsed/FadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        GameHandler.Instance.PlacePiecesRandomly();
        RandomizeCameraSettingsAndPosition();
        yield return new WaitForSeconds(0.2f);

        while (timeElapsed > 0) {
            PanelBackground.color = new Color(r: 0, g: 0, b: 0, a: timeElapsed/FadeDuration);
            timeElapsed -= Time.deltaTime;
            yield return null;
        }
    }

    private void RandomizeCameraSettingsAndPosition() {
        Camera.fieldOfView = new System.Random().Next(30,90);
        System.Random random = new();
        Camera.transform.position = new Vector3(random.Next(3,10), random.Next(3,10), random.Next(3,10));
        Camera.transform.LookAt(Vector3.zero);
        Physics.SyncTransforms();
    }
}
