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
    private float _previousHorizontalSpeed = 0f;

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

    [Header("Walls")]
    [SerializeField] private WallsDetector _wallsDetector;
    public bool IsTouchingWallRight {get; private set;}
    public bool IsTouchingWallLeft {get; private set;}

    [Header("Jump")]
    [SerializeField] private HeroJumpSettings[] _jumpSettings;
    private int _indexJump = 0;

    public bool nbrJump => _indexJump < _jumpSettings.Length;
    public bool canJump => (nbrJump && _jumpState != JumpState.JumpImpulsion);
    [SerializeField] private HeroFallSettings _jumpFallSettings;
    [SerializeField] private HeroJumpHorizontalMovementsSettings _jumpHorizontalMovementsSettings;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    //Camera Follow
    private CameraFollowable _cameraFollowable;

    public bool isDashing = false;

    enum JumpState {
        NotJumping,
        JumpImpulsion,
        Falling
    }

    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;

    private void Awake()
    {
        _cameraFollowable = GetComponent<CameraFollowable>();
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        _cameraFollowable.FollowPositionY = _rigidbody.position.y;
    }

    #region Functions Move Dir
    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }
    #endregion
    private void FixedUpdate()
    {
        _ApplyGroundDetection();
        _ApplyWallRightDetection();
        _ApplyWallLeftDetection();
        _UpdateCameraFollowPosition();

        HeroHorizontalMovementsSettings horizontalMovementsSettings = _GetCurrentHorizontalMovementsSettings();
        if (_AreOrientAndMovementOpposite()){
            _TurnBack(horizontalMovementsSettings);
        } else {
            if (!_dashSettings.isDashing){
                _UpdateHorizontalSpeed(horizontalMovementsSettings);
            }
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


        HeroJumpHorizontalMovementsSettings jumpHorizontalMovementsSettings = _GetCurrentJumpHorizontalMovementsSettings();
        if (!IsTouchingGround){
            if (_jumpState == JumpState.NotJumping){
                _UpdateJumpHorizontalSpeed(jumpHorizontalMovementsSettings);
                _ApplyFallGravity(_fallSettings);
            }
            else{
                _ApplyFallGravity(_fallSettings);
            }
        }
        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();
        _Dash();
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
        GUILayout.Label($"IsDashing  = {_dashSettings.isDashing}");
        if (IsTouchingWallRight){
            GUILayout.Label("TouchingWallRight");
        }
        else{
            GUILayout.Label("NotTouchingWallRight");
        }
        if (IsTouchingWallLeft){
            GUILayout.Label("Touching Wall Left");
        }
        else{
            GUILayout.Label("NotTouchingWallLeft");
        }
        if(canJump){
            GUILayout.Label("Peut Sauter");
        }
        else{
            GUILayout.Label("Ne peut pas sauter");
        }
        

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

    private void _Accelerate(HeroJumpHorizontalMovementsSettings settings){
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > settings.speedMax){
            _horizontalSpeed = settings.speedMax;
        }
    }

    public void _Decelerate(HeroJumpHorizontalMovementsSettings settings){
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

    private void _UpdateJumpHorizontalSpeed(HeroJumpHorizontalMovementsSettings settings){
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

    public void _DashOn()
    {
        if (_dashSettings.timer < _dashSettings.Duration){
            if(!IsTouchingGround){
                _previousHorizontalSpeed = _horizontalSpeed;
                _horizontalSpeed = _dashSettings.Speed;
                _dashSettings.isDashing = true;
            }
            else{
                _previousHorizontalSpeed = _horizontalSpeed;
                _horizontalSpeed = _dashSettings.Speed;
                _dashSettings.isDashing = true;
            }
        }
    }

    public void _Dash()
    {
        if (_dashSettings.isDashing)
        {
            if (_dashSettings.timer <= _dashSettings.Duration)
            {
                _dashSettings.timer += Time.deltaTime;
            } else {
                _dashSettings.isDashing = false;
                if (_previousHorizontalSpeed > 0){
                    _horizontalSpeed =  _previousHorizontalSpeed;
                }
                else{
                    _horizontalSpeed = 0;   
                }
                _dashSettings.timer = 0f;
            }
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
        _indexJump = 0;
    }
    
    private void _ResetVerticalSpeed(){
        _verticalSpeed = 0f;
    }

    public void JumpStart(){
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
        if (_indexJump < _jumpSettings.Length -1){
            _indexJump +=1;
            Debug.Log(_indexJump);
        }
    }

    public bool IsJumping => _jumpState != JumpState.NotJumping;

    private void _UpdateJumpStateImpulsion(int index){
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings[index].jumpMaxDuration){
            _verticalSpeed = _jumpSettings[index].jumpSpeed;

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
                _UpdateJumpStateImpulsion(_indexJump);
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
    public bool isJumpMinDurationReached => _jumpTimer >= _jumpSettings[_indexJump].jumpMinDuration;

    private HeroHorizontalMovementsSettings _GetCurrentHorizontalMovementsSettings(){
        if (IsTouchingGround){
            return _groundHorizontalMovementsSettings;
        } else {
            return _airHorizontalMovementsSettings;
        }
    }

    private HeroJumpHorizontalMovementsSettings _GetCurrentJumpHorizontalMovementsSettings(){
        return _jumpHorizontalMovementsSettings;
    }

    private void _UpdateCameraFollowPosition()
    {
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        if (IsTouchingGround && !IsJumping)
        {
            _cameraFollowable.FollowPositionY = _rigidbody.position.y;
        }
    }

    private void _ApplyWallRightDetection(){
        IsTouchingWallRight = _wallsDetector.DetectWallRightNearBy();
    }

    private void _ApplyWallLeftDetection(){
        IsTouchingWallLeft = _wallsDetector.DetectWallLeftNearBy();
    }

    private void NoDashOnWall(){
        if (IsTouchingWallLeft || IsTouchingWallRight){
            _dashSettings.isDashing = false;
        }
    }
}