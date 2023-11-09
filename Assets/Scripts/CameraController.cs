using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float AnimationDuration;
    [HideInInspector] public bool inAnimation {get; private set;} = false;

    public static CameraController Instance {get; private set;}

    private void Awake() {
        Instance = this;
    }
    
    public void TransitionToPlayerPerspective(Player player) {
        Debug.Log($"Transitioning to Perspective of {player.name}");
        StartCoroutine(Transition(player));
    }

    private IEnumerator Transition(Player player) {
        while(inAnimation) {    // Wait for any ongoing animation to finish
            yield return new WaitForSeconds(0.5f);
        }

        float timeElapsed = 0f;
        inAnimation = true;

        Transform target = player.CamTransform;
        Transform start = transform;

        while (timeElapsed < AnimationDuration) {
            
            transform.position = Vector3.Lerp(start.position, target.position, timeElapsed/AnimationDuration);
            transform.rotation = Quaternion.Slerp(start.transform.rotation, target.rotation, timeElapsed/AnimationDuration);

            Physics.SyncTransforms();
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;

        inAnimation = false;
    }
}
