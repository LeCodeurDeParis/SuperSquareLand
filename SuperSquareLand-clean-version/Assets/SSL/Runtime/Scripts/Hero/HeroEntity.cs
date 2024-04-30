using UnityEngine;
using UnityEngine.Serialization;
public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [FormerlySerializedAs("_movementsSettings")]
    [SerializeField] private HeroHorizontalMovementsSettings _groundHorizontalMovementsSettings;
    [SerializeField] private HeroHorizontalMovementsSettings _airHorizontalMovementsSettings;

    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;

    [Header("Dash")]
    [SerializeField] private HeroDashSettings _dashSettings;

    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;

    [Header("Vertical Movements")]
    private float _verticalSpeed = 0f;

    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround {get; private set;}

    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private float timer = 0f;
    public bool isDashing = false;

    enum JumpState {
        NotJumping,
        JumpImpulsion,
        Falling
    }

    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;

    #region Functions Move Dir
    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }
    #endregion
    private void FixedUpdate()
    {
        _ApplyGroundDetection();

        HeroHorizontalMovementsSettings horizontalMovementsSettings = _GetCurrentHorizontalMovementsSettings();
        if (_AreOrientAndMovementOpposite()){
            _TurnBack(horizontalMovementsSettings);
        } else {
            _UpdateHorizontalSpeed(horizontalMovementsSettings);
            _ChangeOrientFromHorizontalMovement();
        }

        if (IsJumping){
            _UpdateJump();
        } else {
            if (!IsTouchingGround){
                _ApplyFallGravity(_fallSettings);
            }
            else {
                _ResetVerticalSpeed();
            }
        }

        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();
    }

    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        _rigidbody.velocity = velocity;
    }
    
    private void Update()
    {
        _UpdateOrientVisual();
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label($"OrientX = {_orientX}");
        if (IsTouchingGround){
            GUILayout.Label("OnGround");
        } else {
            GUILayout.Label("InAir");
        }
        GUILayout.Label($"JumpState = {_jumpState}");
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");

        GUILayout.EndVertical();
    }

    private void _ChangeOrientFromHorizontalMovement(){
        if (_moveDirX == 0f) return ;
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _Accelerate(HeroHorizontalMovementsSettings settings){
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > settings.speedMax){
            _horizontalSpeed = settings.speedMax;
        }
    }

    public void _Decelerate(HeroHorizontalMovementsSettings settings){
        _horizontalSpeed -= settings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f) {
            _horizontalSpeed = 0f;
        }
    }

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementsSettings settings){
        if (_moveDirX != 0f){
            _Accelerate(settings);
        } else {
            _Decelerate(settings);
        }
    }

    private void _TurnBack(HeroHorizontalMovementsSettings settings){
        _horizontalSpeed -= settings.turnBackFrictions * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f){
            _horizontalSpeed = 0f;
            _ChangeOrientFromHorizontalMovement();
        }
    }

    private bool _AreOrientAndMovementOpposite(){
        return _moveDirX * _orientX < 0f;
    }

    public float _Dash(){
        if (timer < _dashSettings.Duration && !isDashing){
            timer += Time.fixedDeltaTime;
            isDashing = true;
            return _horizontalSpeed += _dashSettings.Speed * Time.fixedDeltaTime;
        } else {
            timer = 0f;
            isDashing = false;
            return 0f;
        }
    }

    private void _ApplyFallGravity(HeroFallSettings settings){
        _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.fallSpeedMax){
            _verticalSpeed = -settings.fallSpeedMax;
        }
    }

    private void _ApplyVerticalSpeed(){
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }

    private void _ApplyGroundDetection(){
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }
    
    private void _ResetVerticalSpeed(){
        _verticalSpeed = 0f;
    }

    public void JumpStart(){
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }

    public bool IsJumping => _jumpState != JumpState.NotJumping;

    private void _UpdateJumpStateImpulsion(){
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings.jumpMaxDuration){
            _verticalSpeed = _jumpSettings.jumpSpeed;
        } else {
            _jumpState = JumpState.Falling;
        }
    }

    private void _UpdateJumpStateFalling(){
        if (!IsTouchingGround) {
            _ApplyFallGravity(_jumpFallSettings);
        } else {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
        }
    }

    private void _UpdateJump(){
        switch (_jumpState){
            case JumpState.JumpImpulsion:
                _UpdateJumpStateImpulsion();
                break;
            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
        }
    }

    public void StopJumpImpulsion(){
        _jumpState = JumpState.Falling;
    }

    public bool IsJumpImpulsing => _jumpState == JumpState.JumpImpulsion;
    public bool isJumpMinDurationReached => _jumpTimer >= _jumpSettings.jumpMinDuration;

    private HeroHorizontalMovementsSettings _GetCurrentHorizontalMovementsSettings(){
        if (IsTouchingGround){
            return _groundHorizontalMovementsSettings;
        } else {
            return _airHorizontalMovementsSettings;
        }
    }


}