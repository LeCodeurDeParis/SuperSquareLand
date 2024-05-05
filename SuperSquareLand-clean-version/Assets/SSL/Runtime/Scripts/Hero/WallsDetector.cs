using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallsDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform[] _detectionPointsLeft;
    [SerializeField] private Transform[] _detectionPointsRight;

    [SerializeField] private float _detectionLength = 0.1f;
    [SerializeField] private LayerMask _wallLayerMask;

    public bool DetectWallRightNearBy(){
        foreach (Transform detectionPoint in _detectionPointsRight){
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.right,
                _detectionLength,
                _wallLayerMask
            );
            if (hitResult.collider != null){
                return true;
            }
        }
        return false;
    }

    public bool DetectWallLeftNearBy(){
        foreach (Transform detectionPoint in _detectionPointsLeft){
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.left,
                _detectionLength,
                _wallLayerMask
            );
            if (hitResult.collider != null){
                return true;
            }
        }
        return false;
    }
    
}
