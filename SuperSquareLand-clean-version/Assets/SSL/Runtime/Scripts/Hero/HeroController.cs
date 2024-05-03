using UnityEngine;

public class HeroController : MonoBehaviour
{
    [Header("Entity")]
    [SerializeField] private HeroEntity _entity;
    private bool _entityWasTouchingGround = false;

    [Header("Dash Settings")]
    [SerializeField] private HeroDashSettings _dashSettings;

    [Header("Jump Buffer")]
    [SerializeField] private float _jumpBufferDuration = 0.2f;
    private float _jumpBufferTimer = 0f;

    [Header("Coyote Time")]
    [SerializeField] private float _coyoteTimeDuration = 0.2f;
    private float _coyoteTimeCountdown = -1f;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"Jump Buffer Timer = {_jumpBufferTimer}");
        GUILayout.Label($"CoyoteTime Countdown = {_coyoteTimeCountdown}");
        GUILayout.EndVertical();
    }

    private void Start()
    {
        _CancelJumpBuffer();
    }

    private bool _GetInputJump()
        {
            return Input.GetKey(KeyCode.Space);
        }

    private bool _GetInputDash()
    {
        return Input.GetKeyDown(KeyCode.E);
    }
    private void Update()
    {
        _UpdateJumpBuffer();

        _entity.SetMoveDirX(GetInputMoveX());

        if (_EnntityHasExitGround())
        {
            _ResetCoyoteTime();
        } else {
            _UpdateCoyoteTime();
        }

        if (_GetInputDownJump())
        {
            if ((_entity.IsTouchingGround || _IsCoyoteTimeActive()) && !_entity.IsJumping)
            {
                _entity.JumpStart();
            } else {
                _ResetJumpBuffer();
            }
        }

        if (_GetInputDash())
        {
            _entity._Dash();
        }

        if (IsJumpBufferActive())
        {
            if ((_entity.IsTouchingGround || _IsCoyoteTimeActive()) && !_entity.IsJumping)
            {
                _entity.JumpStart();
            }
        }

        if(_entity.IsJumpImpulsing){
            if(!_GetInputJump() && _entity.isJumpMinDurationReached){
                _entity.StopJumpImpulsion();
            }
        }
        _entityWasTouchingGround = _entity.IsTouchingGround;
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

        return inputMoveX;
    }

    private bool _GetInputDownJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    private void _ResetJumpBuffer(){
        _jumpBufferTimer = 0f;
    }

    private bool IsJumpBufferActive(){
        return _jumpBufferTimer < _jumpBufferDuration;
    }

    private void _UpdateJumpBuffer(){
        if (!IsJumpBufferActive()) return;
        _jumpBufferTimer += Time.deltaTime;
    }

    private void _CancelJumpBuffer(){
        _jumpBufferTimer = _jumpBufferDuration;
    }

    private void _UpdateCoyoteTime(){
        if (!_IsCoyoteTimeActive()) return;
        _coyoteTimeCountdown -= Time.deltaTime; 
    }

    private void _ResetCoyoteTime(){
        _coyoteTimeCountdown = _coyoteTimeDuration;
    }

    private bool _IsCoyoteTimeActive(){
        return _coyoteTimeCountdown > 0f;
    }

    private bool _EnntityHasExitGround(){
        return _entityWasTouchingGround && !_entity.IsTouchingGround;
    }
}