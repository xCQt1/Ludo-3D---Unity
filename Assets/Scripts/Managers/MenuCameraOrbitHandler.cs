using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class MenuCameraOrbitHandler : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float FadeDuration = 0.5f;
    [SerializeField] private float OrbitDuration = 5.0f;
    [SerializeField] private int RotationDegrees = 200;

    [Header("References")]
    [SerializeField] private Image _panelBackground;
    [HideInInspector] private Camera _camera;
    void Start()
    {
        _camera = gameObject.GetComponent<Camera>();
        StartCoroutine(CinematicCycle());
    }

    private IEnumerator CinematicCycle()  {
        RandomizeCameraSettingsAndPosition();

        while (true) {
            StartCoroutine(CameraOrbit());
            yield return new WaitForSeconds(OrbitDuration);
            StartCoroutine(ScreenFadeMagic());
        }
    }

    private IEnumerator ScreenFadeMagic() {
        float timeElapsed = 0;
        while (timeElapsed < FadeDuration) {
            _panelBackground.color = new Color(r: 0, g: 0, b: 0, a: timeElapsed/FadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        RandomizeCameraSettingsAndPosition();
        yield return new WaitForSeconds(0.2f);

        while (timeElapsed > 0) {
            _panelBackground.color = new Color(r: 0, g: 0, b: 0, a: timeElapsed/FadeDuration);
            timeElapsed -= Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator CameraOrbit() {
        float timeElapsed = 0;
        while (timeElapsed < OrbitDuration) {
            transform.RotateAround(Vector3.zero, Vector3.up, RotationDegrees/OrbitDuration * Time.deltaTime);
            transform.LookAt(Vector3.zero);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void RandomizeCameraSettingsAndPosition() {
        _camera.fieldOfView = new System.Random().Next(30,90);
        System.Random random = new();
        _camera.transform.position = new Vector3(random.Next(3,10), random.Next(3,10), random.Next(3,10));
        _camera.transform.LookAt(Vector3.zero);
        Physics.SyncTransforms();
    }
}
