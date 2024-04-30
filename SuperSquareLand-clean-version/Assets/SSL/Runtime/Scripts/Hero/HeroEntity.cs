using UnityEngine;

public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [SerializeField] private HeroHorizontalMovementsSettings _movementsSettings;
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

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    private float timer = 0f;
    public bool isDashing = false;

    #region Functions Move Dir
    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }
    #endregion
    private void FixedUpdate()
    {
        if (_AreOrientAndMovementOpposite()){
            _TurnBack();
        } else {
            _UpdateHorizontalSpeed();
            _ChangeOrientFromHorizontalMovement();
        }
        _ApplyFallGravity();

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
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");

        GUILayout.EndVertical();
    }

    private void _ChangeOrientFromHorizontalMovement(){
        if (_moveDirX == 0f) return ;
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _Accelerate(){
        _horizontalSpeed += _movementsSettings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > _movementsSettings.speedMax){
            _horizontalSpeed = _movementsSettings.speedMax;
        }
    }

    public void _Decelerate(){
        _horizontalSpeed -= _movementsSettings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f) {
            _horizontalSpeed = 0f;
        }
    }

    private void _UpdateHorizontalSpeed(){
        if (_moveDirX != 0f){
            _Accelerate();
        } else {
            _Decelerate();
        }
    }

    private void _TurnBack(){
        _horizontalSpeed -= _movementsSettings.turnBackFrictions * Time.fixedDeltaTime;
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

    private void _ApplyFallGravity(){
        _verticalSpeed -= _fallSettings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -_fallSettings.fallSpeedMax){
            _verticalSpeed = -_fallSettings.fallSpeedMax;
        }
    }

    private void _ApplyVerticalSpeed(){
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }
}