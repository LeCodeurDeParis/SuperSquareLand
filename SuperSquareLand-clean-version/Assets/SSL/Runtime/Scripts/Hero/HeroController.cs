using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;

    [Header("Dash Settings")]
    [SerializeField] private HeroDashSettings _dashSettings;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.EndVertical();
    }

    private void Update()
    {
        _entity.SetMoveDirX(GetInputMoveX());
    }

    private float GetInputMoveX()
    {
        float inputMoveX = 0f;
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q)){
            inputMoveX = -1f;
        }
        
        if(Input.GetKey(KeyCode.D)){
            inputMoveX = 1f;
        }

        if (Input.GetKey(KeyCode.E)){
            inputMoveX =_entity._Dash();
        }

        return inputMoveX;
    }
}