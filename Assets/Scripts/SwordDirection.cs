using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class SwordDirection : MonoBehaviour
{
    public enum Dir { None = -1, Up = 0, Down = 1, Left = 2, Right = 3 }

    [Header("Arrow Images")]
    public Image arrowUp;
    public Image arrowDown;
    public Image arrowLeft;
    public Image arrowRight;
    public bool CanSwitch => !_switchLocked;
    [SerializeField] private bool _switchLocked = false;

    public void LockSwitch() => _switchLocked = false;
    public void UnlockSwitch() => _switchLocked = false;
    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);

    [Header("Reference Center (Optional)")]
    public RectTransform centerRect; // 保留但相对模式不再依赖它

    [Header("Tuning - Relative Aim")]
    [Tooltip("Mouse X/Y 放大系数，越大越灵敏")]
    public float aimSensitivity = 10f;

    [Tooltip("虚拟摇杆最大半径（越大越不容易进死区）")]
    public float aimMaxRadius = 180f;

    [Tooltip("当鼠标不动时，虚拟摇杆回中速度（越大回中越快）")]
    public float aimRecenterSpeed = 6f;

    [Tooltip("死区（虚拟摇杆长度小于该值时不切换；也不会清空方向）")]
    public float deadzonePixels = 12f;

    [Tooltip("进入切换判定的最小半径（略大于死区，避免贴边抖动）")]
    public float minRadiusForSwitch = 20f;

    [Tooltip("允许反向直穿的角度容差(度)，建议 35~60")]
    [Range(0f, 90f)] public float oppositeLeapAngle = 55f;

    [Tooltip("反向直穿要求的更大半径（避免小抖动触发直穿）")]
    public float oppositeLeapRadius = 60f;

    [Tooltip("扇区角度偏移（度），0=标准四象限分区")]
    public float sectorAngleOffset = 0f;

    [SerializeField] public Dir CurrentDir = Dir.Right;
    public ThirdPersonController player;

    [SerializeField] private Dir _lastDir = Dir.None;

    // 相对模式的“虚拟摇杆”
    [SerializeField] private Vector2 _aim = Vector2.zero;

    // 12 个切换索引表：from(行) -> to(列)
    private static readonly int[,] SwitchIndexMap =
    {
        // to:    Up   Down  Left  Right
        /*Up*/   { -1,   5,    4,    6 },
        /*Down*/ {  11,  -1,   12,   10 },
        /*Left*/ {  9,   7,   -1,    8 },
        /*Right*/{  1,   3,    2,   -1 },
    };
    private void OnEnable()
    {
        CurrentDir = Dir.Right;
        _lastDir = Dir.Right;
        SetHighlight(Dir.Right);
    }
    private void Start()
    {
        CurrentDir = Dir.Right;
        _lastDir = Dir.Right;
        SetHighlight(Dir.Right);
    }

    private void Update()
    {
        if (!CanSwitch)
        {
            // 锁定期间你可以选择只显示当前方向（或显示鼠标方向但不触发动画）
            SetHighlight(_lastDir == Dir.None ? CurrentDir : _lastDir);
            return;
        }
        // 1) 读取鼠标相对输入（不锁鼠标也能用）
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");
        Vector2 input = new Vector2(mx, my) * aimSensitivity;

        // 2) 累积到虚拟摇杆
        _aim += input;

        // 3) clamp 半径
        _aim = Vector2.ClampMagnitude(_aim, aimMaxRadius);

        // 4) 只有在“鼠标基本不动”时才回中（关键：别把输入冲掉）
        if (input.sqrMagnitude < 0.0001f)
        {
            float dt = Time.deltaTime;
            float t = 1f - Mathf.Exp(-aimRecenterSpeed * dt);
            _aim = Vector2.Lerp(_aim, Vector2.zero, t);
        }

        float r = _aim.magnitude;

        // 5) 死区：不切换，但也不把方向清成 None（保持当前）
        if (r < deadzonePixels)
        {
            SetHighlight(_lastDir == Dir.None ? CurrentDir : _lastDir);
            return;
        }

        // 6) 判定方向（根据虚拟摇杆角度）
        Dir dir = DirFromAngle(_aim, sectorAngleOffset);

        // 7) 太靠近中心：只更新 UI，不触发切换动画
        if (r < minRadiusForSwitch)
        {
            SetHighlight(dir);
            return;
        }

        // 8) 允许“反向直穿”（Down->Up / Left->Right）
        Dir leapTo = TryOppositeLeap(_lastDir, _aim, r);
        if (leapTo != Dir.None)
            dir = leapTo;

        // 9) 真正发生切换：只触发一次 SwitchSwordPos
        if (dir != Dir.None && _lastDir != Dir.None && dir != _lastDir)
        {
            int idx = GetSwitchIndex(_lastDir, dir);
            if (dir == Dir.Left)
            {
                player.BlendSwordLeft();
            }
            else if (dir == Dir.Down)
            {
                player.BlendSwordDown();
            }

            else if (dir == Dir.Up) { 


                player.BlendSwordUp();
            }

            else if (dir == Dir.Right)
            {
                player.BlendSwordRight();
            }


            if (idx >= 0 && CanSwitch)
            {
                LockSwitch();                 // ✅ 触发瞬间立刻锁
                player.SwitchSwordPos(idx);   // 触发动画
                _lastDir = dir;


            }
            _lastDir = dir;
        }
        else if (_lastDir == Dir.None)
        {
            _lastDir = dir;
        }

        SetHighlight(dir);
    }

    private Dir TryOppositeLeap(Dir last, Vector2 delta, float r)
    {
        if (last == Dir.None) return Dir.None;
        if (r < oppositeLeapRadius) return Dir.None;

        float ang = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        ang = Normalize360(ang);

        float target;
        Dir targetDir;

        switch (last)
        {
            case Dir.Up: target = 270f; targetDir = Dir.Down; break;
            case Dir.Down: target = 90f; targetDir = Dir.Up; break;
            case Dir.Left: target = 0f; targetDir = Dir.Right; break;
            case Dir.Right: target = 180f; targetDir = Dir.Left; break;
            default: return Dir.None;
        }

        float d = AngleDistance(ang, target);
        return (d <= oppositeLeapAngle) ? targetDir : Dir.None;
    }

    private static Dir DirFromAngle(Vector2 delta, float offsetDeg)
    {
        float ang = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        ang = Normalize360(ang + offsetDeg);

        // Right(315~45), Up(45~135), Left(135~225), Down(225~315)
        if (ang >= 315f || ang < 45f) return Dir.Right;
        if (ang >= 45f && ang < 135f) return Dir.Up;
        if (ang >= 135f && ang < 225f) return Dir.Left;
        return Dir.Down;
    }

    private static float Normalize360(float ang)
    {
        ang %= 360f;
        if (ang < 0f) ang += 360f;
        return ang;
    }

    private static float AngleDistance(float a, float b)
    {
        float d = Mathf.Abs(a - b) % 360f;
        return d > 180f ? 360f - d : d;
    }

    // 保留（目前相对模式不再用中心点）
    private Vector2 GetCenterScreenPos()
    {
        if (centerRect == null)
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        return RectTransformUtility.WorldToScreenPoint(null, centerRect.position);
    }

    // 保留
    private int GetSwitchIndex(Dir from, Dir to)
    {
        if (from == Dir.None || to == Dir.None) return -1;
        if (from == to) return -1;

        int fi = (int)from;
        int ti = (int)to;
        return SwitchIndexMap[fi, ti];
    }

    
    // 保留（你的 attackID + UI 高亮逻辑不动）
    public void SetHighlight(Dir dir)
    {
        CurrentDir = dir;

        if (arrowUp) arrowUp.color = normalColor;
        if (arrowDown) arrowDown.color = normalColor;
        if (arrowLeft) arrowLeft.color = normalColor;
        if (arrowRight) arrowRight.color = normalColor;

        switch (dir)
        {
            case Dir.Up:
                if (arrowUp)
                {
                    arrowUp.color = highlightColor;
                    player.attackID = (player.playerState.stanceValue >= 0) ? 1 : 5;
                }
                break;

            case Dir.Down:
                if (arrowDown)
                {
                    arrowDown.color = highlightColor;
                    player.attackID = (player.playerState.stanceValue >= 0) ? 2 : 6;
                }
                break;

            case Dir.Left:
                if (arrowLeft)
                {
                    arrowLeft.color = highlightColor;
                    player.attackID = (player.playerState.stanceValue >= 0) ? 3 : 7;
                }
                break;

            case Dir.Right:
                if (arrowRight)
                {
                    arrowRight.color = highlightColor;
                    player.attackID = (player.playerState.stanceValue >= 0) ? 4 : 8;
                }
                break;

            case Dir.None:
            default:
                break;
        }
    }
}
