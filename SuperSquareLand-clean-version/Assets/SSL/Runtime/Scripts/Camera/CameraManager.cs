﻿using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance {get; private set;}

    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Profile System")]
    [SerializeField] private CameraProfile _defaultCameraProfile;
    private CameraProfile _currentCameraProfile;

    //Transition

    private float _profileTransitionTimer = 0f;
    private float _profileTransitionDuration = 0f;
    private Vector3 _profileTransitionStartPosition;
    private float _profileTransiaitonStartSize;

    //Follow
    private Vector3 _profileLastFollowDestination;

    //Damping
    private Vector3 _dampedPosition;

    private void Awake()
    {
        Instance = this;
    }

    private void _SetCameraPosition(Vector3 position)
    {
        Vector3 newCameraPosition = _camera.transform.position;
        newCameraPosition.x = position.x;
        newCameraPosition.y = position.y;
        _camera.transform.position = newCameraPosition;
    }

    private void _SetCameraSize (float size)
    {
        _camera.orthographicSize = size;
    }

    private void _SetCameraDampedPosition(Vector3 position)
    {
        _dampedPosition.x = position.x;
        _dampedPosition.y = position.y;
    }

    private void _InitToDefaultProfile()
    {
        _currentCameraProfile = _defaultCameraProfile;
        _SetCameraPosition(_currentCameraProfile.CameraPosition);
        _SetCameraSize(_currentCameraProfile.CameraSize);
        _SetCameraDampedPosition(_FindCameranextPosition());
    }

    private void Start()
    {
        _InitToDefaultProfile();
    }

    private void Update()
    {
        Vector3 nextPosition = _FindCameranextPosition();
        nextPosition = _ApplyDamping(nextPosition);
        if(_IsPlayingProfileTransition())
        {
            _profileTransitionTimer += Time.deltaTime;
            Vector3 transitionPosition = _CalculateProfileTransitionPosition(nextPosition);
            _SetCameraPosition(transitionPosition);
            float transitionSize = _CalculateProfileTransitionCameraSize(_currentCameraProfile.CameraSize);
            _SetCameraSize(transitionSize);
        } else {
            _SetCameraPosition(nextPosition);
            _SetCameraSize(_currentCameraProfile.CameraSize);
        }
    }

    public void EnterProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null)
    {
        _currentCameraProfile = cameraProfile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
        _SetCameraDampedPosition(_FindCameranextPosition());
    }

    public void ExitProfile(CameraProfile cameraProfile, CameraProfileTransition transition = null)
    {
        if (_currentCameraProfile != cameraProfile) return;
        _currentCameraProfile = _defaultCameraProfile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
        _SetCameraDampedPosition(_FindCameranextPosition());

    }

    private void _PlayProfileTransition(CameraProfileTransition transition)
    {
        _profileTransitionStartPosition = _camera.transform.position;
        _profileTransiaitonStartSize = _camera.orthographicSize;
        _profileTransitionTimer = 0f;   
        _profileTransitionDuration = transition.duration;
    }

    private bool _IsPlayingProfileTransition()
    {
        return _profileTransitionTimer < _profileTransitionDuration;
    }

    private float _CalculateProfileTransitionCameraSize(float endSize)
    {
        float percent = _profileTransitionTimer / _profileTransitionDuration;
        percent = Mathf.Clamp01(percent);

        float startSize = _profileTransiaitonStartSize;
        return startSize + (endSize - startSize) * percent;
    }

    private Vector3 _CalculateProfileTransitionPosition(Vector3 destination)
    {
        float percent = _profileTransitionTimer / _profileTransitionDuration;
        percent = Mathf.Clamp01(percent);

        Vector3 origin = _profileTransitionStartPosition;
        return origin + (destination - origin) * percent;

    }

    private Vector3 _FindCameranextPosition()
    {
        if(_currentCameraProfile.ProfileType == CameraProfileType.FollowTarget)
        {
            if (_currentCameraProfile.TargetToFollow != null) {
                CameraFollowable targetToFollow = _currentCameraProfile.TargetToFollow;
                _profileLastFollowDestination.x = targetToFollow.FollowPositionX;
                _profileLastFollowDestination.y = targetToFollow.FollowPositionY;
                return _profileLastFollowDestination;
            }
        }
        return _currentCameraProfile.CameraPosition;
    }

    private Vector3 _ApplyDamping(Vector3 position)
    {
        if (_currentCameraProfile.UseDampingHorizontally) {
            _dampedPosition.x = Mathf.Lerp(
                _dampedPosition.x,
                position.x,
                _currentCameraProfile.HorizontalDampingFactor * Time.deltaTime
            );
        } else {
            _dampedPosition.x = position.x;
        }

        if (_currentCameraProfile.UseDampingVertically) {
            _dampedPosition.y = Mathf.Lerp(
                _dampedPosition.y,
                position.y,
                _currentCameraProfile.VerticalDampingFactor * Time.deltaTime
            );
        } else {
            _dampedPosition.y = position.y;
        }

        return _dampedPosition;
    }
}