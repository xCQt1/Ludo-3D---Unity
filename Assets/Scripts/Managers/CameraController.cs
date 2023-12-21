using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float _animationDuration;
    [HideInInspector] public bool InAnimation {get; private set;} = false;

    public static CameraController Instance {get; private set;}

    private void Awake() {
        Instance = this;
    }
    
    public void TransitionToPlayerPerspective(Player player) {      // moves the camera to the players perspective
        Debug.Log($"Transitioning to Perspective of {player.name}");
        StartCoroutine(Transition(player));
    }

    private IEnumerator Transition(Player player) {     // actual animation
        while(InAnimation) {    // Wait for any ongoing animation to finish
            yield return new WaitForSeconds(0.5f);
        }

        float timeElapsed = 0f;
        InAnimation = true;

        Transform target = player.CamTransform;
        Transform start = transform;

        while (timeElapsed < _animationDuration) {
            
            transform.position = Vector3.Lerp(start.position, target.position, timeElapsed/_animationDuration);
            transform.rotation = Quaternion.Slerp(start.transform.rotation, target.rotation, timeElapsed/_animationDuration);

            Physics.SyncTransforms();
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;

        InAnimation = false;
    }
    
}
