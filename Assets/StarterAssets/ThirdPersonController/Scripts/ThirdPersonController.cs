 using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : Singleton<ThirdPersonController>
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        public bool isAttack;
        public int attack_num;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        public bool canMove;
        private bool roll_accelerate;
        private bool flip_accelerate;
        public bool isFliping;
        public bool canFlipToMove;
        Vector3 _rollDir;
        private AnimatorStateInfo stateInfo;
        public bool canChainNext;//是否进入连招区间
        public bool bufferAttack;
        public bool canRollAttack;
        public bool isRolling;
        public bool canImmediatelyAttack;
        
        public PlayerStateUI playerState;


        private bool isInCombo;
        public bool canRotateDuringAttack;

        

        public GameObject defenseEffect;
        //public Transform sword;
        //public Transform defenceEffectPoint;

        [Header("Spell Settings")]
        public FireBall fireballPrefab;
        public Transform firePoint;        // 手/武器前端的挂点
        public float baseTravelTime = 0.7f; // 基础飞行时间
        public float maxExtraTime = 0.3f;   // 远距离时额外增加一点时间

        [Header("Lock-On")]
        //public EnemyBase currentLockTarget;
        public Transform cameraTransform; //Main camera引用
        public float lockOnPitch = 15f;//与玩家保持的相对高度
        public float lockOnRadius = 15f; //能够锁定敌人的半径
        private bool isTargeting; //是否已经锁定
        public Transform lockTarget; //锁定的目标，通常是敌人
        //public Transform faceTarget;
        public float lockOnCameraLerpSpeed = 2f; //相机过渡速度
        public GameObject lockIcon; //相机锁定的图标

        [Header("damage-check")]
        public bool canTakeDamage;

        [Header("Auto Attack Face")]
        [SerializeField] private float autoFaceRadius = 6f;       // 自由视角下自动转向的半径
        [SerializeField] private float autoFaceTurnSpeed = 720f;  // 自动转向最大角速度(度/秒)

        [Header("SwordWave")]
        public SwordWave swordWavePrefab;
        public SwordWave currSwordWave;
        public Transform swordWavePoint;

        [Header("Counter")]
        public bool isCounter;

        [Header("Weapon")]
        public Sword playerWeapon;

        [Header("Execution")]
        [SerializeField] private float executePitch = 8f;          // 稍稍俯视一点
        [SerializeField] private float executeCameraLerpSpeed = 10f;
        public bool isExecutingView;
        public bool canExecute;

        [Header("SwitchSword")]
        public Material darkSwordMaterial;
        public Material glowSwordMaterial;
        public GameObject darkSwordEffect;
        public GameObject glowSwordEffect;
        public bool isDark;
        public bool triggerCounterTutorial;
        public float switchCoolDown = 0f;

        public bool isCounterReward;
        public bool isRollReward;

        [Header("GetHit")]
        public bool isHitState;


        [Header("HeavyAttack")]
        public bool isHeavyAttack;

        [Header("Die and Recover")]
        public bool isDead;
        public Transform bornPoint;

        //private bool isExecuting;
        [Header("AxeReward")]
        public GameObject axeEquipped;

        public SwordDirection swordDirUI;
        public int attackID;
        public bool isdodging;

        public GameObject leftSword;
        public GameObject rightSword;
        public int switchIndex;
        public SwordDirection swordDir;
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private static readonly List<string> comboStateNames = new List<string>
        {
            "combo_01_1",
            "combo_01_2",
            "combo_01_3",
            "combo_04_1",
            "combo_04_2",
            "combo_04_3",
            "combo_04_4",
            "combo_04_5",
        // 之后有新的连招，直接在这里加就行
        
        };


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            playerState = GetComponent<PlayerStateUI>();
            //Time.timeScale = 0.5f;
            Tutorial.Instance.ShowPerspectiveTip();
        }

        private void Update()
        {

            _hasAnimator = TryGetComponent(out _animator);
            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
           

            if (_input.attack)
            {
                _input.attack = false;
                Debug.Log("攻击");
                _animator.SetTrigger("Attack");
                _animator.SetInteger("attackID", attackID);
                ResetLayerWeight();
                BlendSwordRight();

                if (attackID == 4)
                {
                    leftSword.SetActive(true);
                    rightSword.SetActive(false);
                }
                else
                {
                    leftSword.SetActive(false);
                    rightSword.SetActive(true);
                }

                
            }


            if (lockTarget != null&&lockIcon!=null)
            {
                lockIcon.transform.position = Camera.main.WorldToScreenPoint(lockTarget.GetChild(0).position);
            }

            if (!isFliping&&!isDead&&!LevelManager.Instance.isPause)
            {

                JumpAndGravity();
                GroundedCheck();
            
                //Attack();
                HeavyAttack();
                UpdateAttackFacing();
                Counter();
                Dodge();
                //Roll();
                //RollRollAccelerate();
            
                Stab();
                Toss();
                ChangeToSneak();
                ChangeCombo();
                ChangeMovement();
                //ChangeExecutionView();
                TakeExecution();
            }

            switchCoolDown-=Time.deltaTime;

            FlipAccelerate();

            
            isTargeting = _animator.GetFloat("LockOn") == 1;

            if(EnemyManager.Instance.GetNearestEnemy(transform.position, lockOnRadius) != null)
            {
                if (Tutorial.Instance.isFinishAttackTip &&!Tutorial.Instance.isFinishLockTip)
                {
                    Tutorial.Instance.ShowLockTip();
                }
            }
            if (lockTarget != null && lockTarget.GetComponent<EnemyBase>().state == EnemyBase.EnemyState.ChangeYinYang)
            {
                if (!Tutorial.Instance.isFinishTossTip)
                {
                    Tutorial.Instance.ShowTossTip();
                }
            }


            isInCombo = comboStateNames.Any(name => stateInfo.IsName(name));

            if (isInCombo)
            {
                if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f)
                {
                    _animator.applyRootMotion = true;
                }
                else
                {
                    

                    _animator.applyRootMotion = false;
                    //attack_num = 0;
                    //_animator.SetInteger("Attack_num", attack_num);
                    
                }
                //_animator.applyRootMotion = true;
                // 当前正在播放名为“你的动画状态名称”的动画
                //Debug.Log("当前动画是：你的动画状态名称");
            }
            else if (stateInfo.IsName("heavyAttack"))
            {
                _animator.applyRootMotion = true;
            }
            else if (stateInfo.IsName("Dodge"))
            {
                _animator.applyRootMotion = true;
            }
            else if (stateInfo.IsName("Dodge Roll"))
            {
                if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.3f)
                {
                    _animator.applyRootMotion = false;
                    canRollAttack = false;


                    //canMove = false;
                }
                else if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.3f && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.87f)
                {
                    canMove = false;
                    roll_accelerate = false;
                    _animator.applyRootMotion = true;
                }
                else
                {
                    if (_animator.GetFloat("LockOn") != 1)
                    {
                        _animator.applyRootMotion = false;
                        canMove = true;
                    }


                }
            }

            else if (_animator.GetBool("isSneak"))
            {
                _animator.applyRootMotion = true;
            }
            else
            {
                _animator.applyRootMotion = false;

            }

            if (isdodging) return;

            if (_animator.GetBool("isSneak") || isTargeting)
            {
                if(!isDead && !LevelManager.Instance.isPause)
                    EightDirectionMove();                    
            }
            else
            {
                if(canMove&&!isDead&& !LevelManager.Instance.isPause)
                    Move();
            }
            

            

            //Debug.Log(_input.attack);
        }

        private void LateUpdate()
        {
            if (isExecutingView)
            {
                ExecuteCameraRotation();
            }
            else
            {

                if (isTargeting)
                {
                    if (!LevelManager.Instance.isPause)
                        LockOnCameraRotation();
                }
                else
                {
                    if(!LevelManager.Instance.isPause)
                        CameraRotation();
                }
            }
            
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
                if (!Tutorial.Instance.isFinishPerspectiveTip)
                {
                    Tutorial.Instance.isFinishPerspectiveTip= true;
                    Tutorial.Instance.HideTip();
                    Tutorial.Instance.ShowMoveTip();
                }
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }


        private void LockOnCameraRotation()
        {
            if (lockTarget == null)
            {
                // 没有锁定目标时退回自由视角逻辑
                CameraRotation();
                return;
            }

            // ★ 1. 用“玩家指向敌人”的方向来算 yaw，而不是用玩家自身的欧拉角
            Vector3 toTarget = lockTarget.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
                return;

            // 世界空间转成角度：z 轴为前
            float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;

            // ★ 2. 平滑插值到目标 yaw（无论是否在翻滚，都对着敌人）
            _cinemachineTargetYaw = Mathf.LerpAngle(
                _cinemachineTargetYaw,
                targetYaw,
                Time.deltaTime * lockOnCameraLerpSpeed   // 比如 10f
            );

            float targetPitch = lockOnPitch;
            _cinemachineTargetPitch = Mathf.Lerp(
                _cinemachineTargetPitch,
                targetPitch,
                Time.deltaTime * lockOnCameraLerpSpeed
            );

            // pitch 继续用你现有的逻辑（可以保持原高度，或稍微固定一个角度）
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // ★ 3. 把旋转应用到 Cinemachine 的跟随目标
            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                                 _cinemachineTargetYaw,
                                 0f);
        }

        private void ExecuteCameraRotation()
        {
            var camMode = CameraModeController.Instance;
            if (camMode == null || camMode.currentEnemy == null)
            {
                // 没有处决目标就退回普通逻辑
                CameraRotation();
                return;
            }

            Transform execTarget = camMode.currentEnemy;

            // 1. 计算“玩家 → 处决目标”的水平向量
            Vector3 toTarget = execTarget.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
                return;

            // 2. 世界空间转成 yaw 角度（Z 轴朝前）
            float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;

            // 3. 平滑插值到目标 yaw（不读鼠标输入，完全跟随处决目标）
            _cinemachineTargetYaw = Mathf.LerpAngle(
                _cinemachineTargetYaw,
                targetYaw,
                Time.deltaTime * executeCameraLerpSpeed
            );

            // 4. pitch 固定在一个常量（只狼处决那种微俯视）
            float targetPitch = executePitch;
            _cinemachineTargetPitch = Mathf.Lerp(
                _cinemachineTargetPitch,
                targetPitch,
                Time.deltaTime * executeCameraLerpSpeed
            );

            // 如果你还有俯仰限制就照旧夹一下
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // 5. 把旋转应用到 Cinemachine 的跟随目标
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0f
            );
        }


        private void Attack()
        {
            if (stateInfo.IsName("Backflip") || CameraModeController.Instance.vcamExecute.Priority>0||canExecute)
                return;

            bool canStartFirstAttackNow =(attack_num == 0) && (!isRolling || canRollAttack); 
            if ((canChainNext || canStartFirstAttackNow) && GameObject.Find("Dialogue Panel") == null)
            {
                if (bufferAttack)
                {
                    if (playerState.energyBar.fillAmount > playerState.energy_per_attack) { 

                        ExecuteAttack();
                        Debug.Log("触发预输入");
                        bufferAttack = false;
                    }
                }
                else
                {
                    canImmediatelyAttack = true;
                    

                }
            }

            if (_input.attack )
            {
                _input.attack = false;
                
                if (GameObject.Find("Dialogue Panel") == null)
                {
                    if (attack_num == 0)
                    {
                        if (isRolling && !canRollAttack)
                        {
                            bufferAttack = true;
                            return;
                        }
                        canChainNext = false;
                        canImmediatelyAttack = true;
                        //ExecuteAttack();
                    }
                    

                    if (canImmediatelyAttack)
                    {
                        if (playerState.energyBar.fillAmount > playerState.energy_per_attack)
                        {

                            ExecuteAttack();
                            //Debug.Log("进行连招");
                            canImmediatelyAttack = false;
                        }
                    }
                    else
                    {
                        bufferAttack = true;  // 预输入
                    }

                    
                }
            }
        }


        private void ExecuteAttack()
        {

            if (isTargeting && lockTarget != null)
            {
                Vector3 toTarget = lockTarget.position - transform.position;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    Quaternion currentRot = transform.rotation;
                    Quaternion targetRot = Quaternion.LookRotation(toTarget);

                    // 计算当前与目标的夹角
                    float angle = Quaternion.Angle(currentRot, targetRot);
                    if (angle > 0.01f)
                    {
                        // 这一帧最多能转多少度
                        float maxStep = 720f * Time.deltaTime;
                        // 本帧插值比例 t（0~1）
                        float t = Mathf.Clamp01(maxStep / angle);

                        // ★ 用 Slerp 进行“部分旋转”，而不是一次旋完
                        transform.rotation = Quaternion.Slerp(currentRot, targetRot, t);
                        // 等价也可以写成：
                        // transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, maxStep);
                    }
                }

                
            }

           

            Debug.Log("执行一次");
            _animator.applyRootMotion = true;
            if(!_animator.GetBool("changeCombo"))
            {

                attack_num = (attack_num % 3) + 1;
            }
            else
            {
                attack_num = attack_num + 1;
            }
            _animator.SetInteger("Attack_num", attack_num);
            _animator.SetTrigger("Attack");
            //_input.attack = false;
            _animator.SetBool("isSneak", false);
            playerState.ConsumeEnergy();
            playerState.recoverEnergy = false;
            canImmediatelyAttack = false;            
            canChainNext = false;
            bufferAttack = false;

            if (attack_num == 2)
            {
                if (Tutorial.Instance.isFinishJumpTip && !Tutorial.Instance.isFinishAttackTip)
                {
                    Tutorial.Instance.isFinishAttackTip = true;
                    Tutorial.Instance.HideTip();
                    Tutorial.Instance.ShowHeavyAttackTip();
                }
            }
            

        }

        private void UpdateAttackFacing()
        {
            
            if (!isTargeting || lockTarget == null) return;
            if (!canRotateDuringAttack) return;

            Vector3 toTarget = lockTarget.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Quaternion currentRot = transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(toTarget);

            float maxStep = 720f * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, maxStep);
        }

        private void TakeExecution()
        {
            if (Input.GetKeyDown(KeyCode.F)&&canExecute)
            {
                ChangeExecutionView();
                canExecute = false;
                Debug.Log("执行处决");
                _animator.SetTrigger("Execution");
                playerWeapon.SetStabTransform();
                lockTarget.GetComponent<EnemyBase>().HideExecutionMarker();
                Tutorial.Instance.HideTip();
                AudioManager.Instance.PlayBeginExecution();
                
            }
        }

        public void ApplyExecutionEffect()
        {
            if(isTargeting)
                lockTarget.GetComponent<EnemyBase>().PlayExecutionAnim();
        }
        public void HeavyAttack() {


            if (Input.GetKeyDown(KeyCode.R))
            {
                _animator.SetTrigger("isHeavyAttack");
                if (Tutorial.Instance.isFinishAttackTip && !Tutorial.Instance.isFinishHeavyAttackTip)
                {
                    Tutorial.Instance.isFinishHeavyAttackTip = true;
                    Tutorial.Instance.HideTip();
                    
                }
            }
        }

        private void Counter()
        {
            if (_input.defense)
            {
                _input.defense=false;
                _animator.SetTrigger("Defense");
                //isCounter = true;
                if (Time.timeScale != 1f && !Tutorial.Instance.isFinishCounterTip)
                {
                    Time.timeScale=1f;
                    Tutorial.Instance.isFinishCounterTip = true;
                    isCounterReward = true;
                    Tutorial.Instance.HideTip();
                }
                
            }
        }

        public void ShowCounterEffect()
        {
            defenseEffect.SetActive(false);
            defenseEffect.SetActive(true);
            isCounter = false;
        }



        private void Stab()
        {
            if (_input.stab)
            {
                _input.stab=false;
                _animator.SetTrigger("Stab");
            }
        }

        private void Toss()
        {
            if (_input.toss)
            {
                _input.toss = false;
                _animator.SetTrigger("Toss");
                //CastFireball();
                print("投掷");
                if (!Tutorial.Instance.isFinishTossTip&&Tutorial.Instance.currentTip==Tutorial.Instance.tossTip)
                {
                    Tutorial.Instance.HideTip() ;
                    Tutorial.Instance.isFinishTossTip = true;
                }
                playerState.ConsumeMagic(0.2f);
            }
        }

        void CastFireball()
        {
            if (fireballPrefab == null || firePoint == null) return;
            AudioManager.Instance.PlayThrowFireBall();
            // 1. 生成火球
            FireBall proj = Instantiate(
                fireballPrefab,
                firePoint.position,
                firePoint.rotation
            );

            // 2. 计算目标位置
            Vector3 targetPos;

            if (lockTarget != null)
            {
                // 锁定时瞄准敌人锁定点，稍微往上抬一点
                targetPos = lockTarget.position + Vector3.up * 0.2f;
            }
            else
            {
                // 无锁定：沿摄像机前方打一段距离
                Vector3 forward = cameraTransform != null
                    ? cameraTransform.forward
                    : transform.forward;

                forward.y = Mathf.Clamp(forward.y, -0.1f, 0.5f); // 避免打太高/太低
                forward.Normalize();

                float distance = 15f; // 无锁定时默认射程
                targetPos = firePoint.position + forward * distance;
            }

            // 3. 根据距离调整飞行时间，让远处多飞一会儿，近处快一点
            Vector3 displacement = targetPos - firePoint.position;
            float horizontalDist = new Vector3(displacement.x, 0, displacement.z).magnitude;

            // 简单：水平距离越远，时间稍微长一点（上限 maxExtraTime）
            float t = baseTravelTime + Mathf.Clamp01(horizontalDist / 20f) * maxExtraTime;

            // 4. 计算初始速度：v = (Δp - 0.5 * g * t^2) / t
            Vector3 g = Physics.gravity;
            Vector3 velocity = (displacement - 0.5f * g * t * t) / t;

            // 5. 发射
            proj.Launch(velocity);
        }


        public void AttackRotateOn()
        {
            canRotateDuringAttack = true;
        }

        // 在挥刀中后段锁死方向
        public void AttackRotateOff()
        {
            canRotateDuringAttack = false;
        }

        public void OpenComboWindow()
        {
            canChainNext = true;
            //Debug.Log("可以切换连招");
        }

        public void CloseComboWindow()
        {
            canChainNext = false;
        }

        public void OpenDamageWindow()
        {
            canTakeDamage = true;
        }

        

        public void CloseDamageWindow()
        {
            canTakeDamage = false;
        }

        public void OpenHeavyAttackWindow()
        {
            canTakeDamage = true;
            isHeavyAttack=true;
        }

        public void CloseHeavyAttackWindow()
        {
            canTakeDamage = false;
            isHeavyAttack = false;
        }
        public void OpenRollAttackWindow()
        {
            canRollAttack = true;
            isRolling = false;
            playerState.recoverEnergy = true;
        }

        public void OpenCounterWindow()
        {
            isCounter = true;
        }

        public void CloseCounterWindow()
        {
            isCounter = false;
        }

        public void ResetCombo()
        {
            attack_num = 0;
            _animator.SetInteger("Attack_num", attack_num);
            if(!isRolling)
                playerState.recoverEnergy = true;
            canFlipToMove = true;
            
        }

        public void ResetInput()
        {
            _input.attack = false;
            
            _input.roll = false;
            _input.stab = false;
            _input.toss = false;
            Input.ResetInputAxes();
        }
        private void RollAccelerate()
        {
            if (roll_accelerate)
            {
                _controller.Move(transform.forward * 5f * Time.deltaTime);
            }
        }
        private void FlipAccelerate()
        {
            if (flip_accelerate)
            {
                _controller.Move(transform.forward * (-8f) * Time.deltaTime);
            }
        }

        public void OpenFilpAccelerateWindow()
        {
            canFlipToMove = false;
            flip_accelerate = true;
        }

        public void CloseFilpAccelerateWindow()
        {
            //canMove = true;
            flip_accelerate = false;
            isFliping = false;
        }

        public void OpenFlipToMoveWindow()
        {
            canFlipToMove = true;
        }
        public void EnableMove()
        {
            canMove=true;
        }

        public void Dodge()
        {
            if (_input.roll)
            {
                isdodging = true;
                Vector2 raw = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                cameraTransform = Camera.main.transform;
                // 2) 把“镜头朝向”投影到地面（只取 XZ），作为本地坐标系的 +Z
                Vector3 camF = cameraTransform ? cameraTransform.forward : Vector3.forward;
                Vector3 camR = cameraTransform ? cameraTransform.right : Vector3.right;

                Vector3 f = camF;          // camF 是你投影到水平面的 camera forward
                if (f.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(f, Vector3.up);
                }

                // 投影到水平面，避免镜头俯仰影响
                camF.y = 0f;
                camR.y = 0f;
                camF.Normalize();
                camR.Normalize();

                // 3) 用镜头坐标系把输入转换成世界方向：dirWorld = camR * x + camF * y
                Vector3 dirWorld3 = camR * raw.x + camF * raw.y;

                // 4) 没有输入就默认“向镜头前方”闪避
                Vector2 dirWorld2;
                if (dirWorld3.magnitude < 0.2f)
                {
                    dirWorld2 = Vector2.up; // 注意：这里的 Vector2.up 表示“镜头前方”
                }
                else
                {
                    // 转成“以镜头为坐标系”的二维向量：x=右，y=前
                    // 这一步其实 raw 已经是镜头坐标系输入了，但我们用 dirWorld3 更稳（兼容手柄/自定义输入）
                    // 把世界向量再投回镜头轴上得到 camera-space 2D
                    float x = Vector3.Dot(dirWorld3.normalized, camR);
                    float y = Vector3.Dot(dirWorld3.normalized, camF);
                    dirWorld2 = new Vector2(x, y);
                }

                // 5) 在“镜头坐标系”下量化到 8 方向（结果为 -1/0/1 组合）
                Vector2 dir = QuantizeTo8Dir(dirWorld2, 0.0001f); // 这里死区已在上面处理过，给个极小值即可

                // 写入 BlendTree 参数
                _animator.SetFloat("dodgeX", dir.x);
                _animator.SetFloat("dodgeY", dir.y);

                // 触发闪避
                
                _animator.SetTrigger("Dodge");
                canMove = false;
                _animator.applyRootMotion = true;
                _input.roll = false;
                
                Invoke("EndDodge", 1.2f);


            }
        }

        private void EndDodge()
        {
            isdodging=false;
        }

        static Vector2 QuantizeTo8Dir(Vector2 raw, float dz)
        {
            if (raw.magnitude < dz) return Vector2.zero;

            raw.Normalize();

            float x = raw.x;
            float y = raw.y;

            // 以 22.5° 为阈值划分 8 区
            float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // 0°=Right，90°=Up（Forward）
            int sector = Mathf.RoundToInt(angle / 45f) % 8;

            return sector switch
            {
                0 => new Vector2(1, 0), // Right
                1 => new Vector2(1, 1), // UpRight
                2 => new Vector2(0, 1), // Up
                3 => new Vector2(-1, 1), // UpLeft
                4 => new Vector2(-1, 0), // Left
                5 => new Vector2(-1, -1), // DownLeft
                6 => new Vector2(0, -1), // Down
                7 => new Vector2(1, -1), // DownRight
                _ => Vector2.up
            };
        }
        private void Roll()
        {
            if (_input.roll)
            {
                if ((canMove ||isTargeting) && playerState.energyBar.fillAmount > playerState.energy_per_attack ) {
                    // 1. 计算翻滚方向（这里举例：相机方向 + 输入方向）
                    if(isTargeting ||_animator.GetBool("isSneak"))
                    {
                        Vector2 move = _input.move;
                        _animator.SetBool("isSneak", false);

                        if (move.sqrMagnitude < 0.01f)
                        {
                            // 没有输入时，你可以选择：
                            // _rollDir = transform.forward;      // 始终向前滚
                            // 或者 _rollDir = -transform.forward; // 后撤步
                            _rollDir = transform.forward;
                        }
                        else
                        {
                            // 相机前/右方向投射到地面
                            Vector3 camFwd = _mainCamera.transform.forward;
                            camFwd.y = 0f;
                            camFwd.Normalize();

                            Vector3 camRight = _mainCamera.transform.right;
                            camRight.y = 0f;
                            camRight.Normalize();

                            // 上下左右输入组合成世界空间的滚动方向
                            _rollDir = (camFwd * move.y + camRight * move.x).normalized;
                        }

                        // 2. 把角色朝向旋转到滚动方向
                        if (_rollDir.sqrMagnitude > 0.0001f)
                            transform.rotation = Quaternion.LookRotation(_rollDir);

                    }


                    //_animator.applyRootMotion = true;
                    _animator.SetTrigger("Roll");
                    roll_accelerate = true;


                    canMove =false;
                    isRolling = true;
                    playerState.ConsumeEnergy();
                    playerState.recoverEnergy = false;
                    AudioManager.Instance.PlayRoll();
                    

                    if (Time.timeScale != 1f &&!Tutorial.Instance.isFinishRollTip)
                    {
                        Time.timeScale = 1f;
                        Tutorial.Instance.isFinishRollTip = true;
                        isRollReward = true;
                        Tutorial.Instance.HideTip();

                    }

                    if (Time.timeScale != 1f && !Tutorial.Instance.isFinishRollTip_volumeBall)
                    {
                        Time.timeScale = 1f;
                        Tutorial.Instance.isFinishRollTip_volumeBall = true;
                        //isRollReward = true;
                        Tutorial.Instance.HideTip();

                    }


                }
                
                _input.roll = false;
            }
        }

        

        private void ChangeToSneak()
        {
            if (_input.sneak)
            {
                Debug.Log("变为潜行"+ _animator.GetBool("isSneak"));
                _animator.SetBool("isSneak", !_animator.GetBool("isSneak"));
                _input.sneak=false;
            }
        }

        private void ChangeMovement()
        {
            if (Input.GetKeyDown(KeyCode.Q)&&!isExecutingView&&!canExecute)
            {
                if (_animator.GetFloat("LockOn") == 0f)
                {

                    StartLock();
                    
                    if (!Tutorial.Instance.isFinishLockTip)
                    {
                        Tutorial.Instance.isFinishLockTip=true;
                        Tutorial.Instance.HideTip();
                    }
                    //currentMarker = Instantiate(lockOnPrefab, lockTarget.GetChild(0));
                    lockIcon.SetActive(true);
                    lockIcon.transform.position = Camera.main.WorldToScreenPoint(lockTarget.GetChild(0).position);
                }
                else
                {
                    ReleaseLock();
                    lockIcon.SetActive(false);
                }
                    
            }
        }

        private void ChangeExecutionView()
        {
            
            isExecutingView = !isExecutingView;
            if (isExecutingView)
            {
                CameraModeController.Instance.StartExecuteCamera();
                LockCameraPosition = true;
            }
            else
            {
                CameraModeController.Instance.EndExecuteCamera();
                if(GameObject.Find("Boss")!=null)
                    GameObject.Find("Boss").GetComponent<EnemyBase>().changeColorCoolTime = 10f;
                //LockCameraPosition = false;
                if (isTargeting)
                {
                    //Destroy(lockTarget.GetChild(0).transform.GetChild(0).gameObject);
                    //currentMarker = Instantiate(lockOnPrefab, lockTarget.GetChild(0));
                    lockIcon.SetActive(true);
                    lockIcon.transform.position = Camera.main.WorldToScreenPoint(lockTarget.GetChild(0).position);


                }
            }
            
        }
        public void StartLock()
        {
            EnemyBase nearest = EnemyManager.Instance.GetNearestEnemy(transform.position, lockOnRadius);
            if (nearest != null)
            {

                lockTarget = nearest.transform;
                CameraModeController.Instance.SetLockOn(true, lockTarget);
                _animator.SetFloat("LockOn", 1f);
                LockCameraPosition = true;
                
            }
        }
        public void ReleaseLock()
        {
            CameraModeController.Instance.SetLockOn(false, null);
            _animator.SetFloat("LockOn", 0f);
            
            canMove = true;
            LockCameraPosition = false;
           
        }

        private void ChangeCombo()
        {
            if (Input.GetKeyDown(KeyCode.Tab)&& switchCoolDown<=0 &&Tutorial.Instance.switchSwordBtn.activeInHierarchy)
            {
                isFliping = true;
                _animator.SetBool("changeCombo", !_animator.GetBool("changeCombo"));
                _animator.SetTrigger("Flip");
                ResetCombo();
                if (!isDark) {
                    isDark = true;
                    SwitchSwordUI.Instance.ShowSwitchLightBtn();
                    playerWeapon.GetComponent<MeshRenderer>().material = darkSwordMaterial;
                    glowSwordEffect.SetActive(false);
                    darkSwordEffect.SetActive(true);
                }
                else
                {
                    isDark = false;
                    SwitchSwordUI.Instance.ShowSwitchDarkBtn();
                    playerWeapon.GetComponent<MeshRenderer>().material = glowSwordMaterial;
                    darkSwordEffect.SetActive(false);
                    glowSwordEffect.SetActive(true);
                }
                AudioManager.Instance.PlaySwordWave();
                if (!Tutorial.Instance.isFinishSwapTip)
                {
                    Tutorial.Instance.isFinishSwapTip=true;
                    Tutorial.Instance.HideTip();
                    Time.timeScale = 1f;
                }

            }
        }

        public void CreateSwordWave()
        {
            currSwordWave = Instantiate(swordWavePrefab, swordWavePoint);
            currSwordWave.transform.parent = null;
            currSwordWave.EmitWave();
        }

        public void GetHeavyAttack()
        {
            _animator.SetTrigger("isHeavyAttacked");
        }

        private void EightDirectionMove()
        {
            Vector2 move = _input.move;

            if (comboStateNames.Any(name => stateInfo.IsName(name))|| stateInfo.IsName("Toss")|| !canFlipToMove)
            {
                // 输入参数平滑归零，避免动画树切到移动
                float x0 = Mathf.Lerp(_animator.GetFloat("inputX"), 0f, Time.deltaTime * 10f);
                float y0 = Mathf.Lerp(_animator.GetFloat("inputY"), 0f, Time.deltaTime * 10f);
                _animator.SetFloat("inputX", x0);
                _animator.SetFloat("inputY", y0);
                return;    // ★ 关键：直接返回，不再改 transform.position / rotation
            }

            //if (stateInfo.IsName("Backflip"))
            //    return;



            // ---------- 锁定视角移动 ----------
            if (isTargeting && lockTarget != null && !isRolling)
            {
                // ★ 修改 1：无论有没有输入，都先让角色朝向目标
                Vector3 toTarget = lockTarget.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toTarget);
                    // 用 Slerp 平滑一点，也可以直接赋值
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        Time.deltaTime * 10f);
                }

                // 下面再处理移动和动画
                if (move.sqrMagnitude < 0.0001f)
                {
                    _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
                    // 没有输入：inputX/inputY 平滑回 0
                    float x0 = Mathf.Lerp(_animator.GetFloat("inputX"), 0f, Time.deltaTime * 10f);
                    float y0 = Mathf.Lerp(_animator.GetFloat("inputY"), 0f, Time.deltaTime * 10f);
                    _animator.SetFloat("inputX", x0);
                    _animator.SetFloat("inputY", y0);
                    AudioManager.Instance.StopFootStep();
                    return;
                }
                else
                {
                    AudioManager.Instance.PlayFootStepSFX();
                }

                    Vector3 localMove = new Vector3(move.x, 0f, move.y);
                localMove = Vector3.ClampMagnitude(localMove, 1f);  // 保证最大长度 1

                // 转到世界空间：forward = 面向敌人，right = 围着敌人绕圈
                Vector3 worldMoveDir = transform.TransformDirection(localMove).normalized;

                // 5. 统一速度，防止斜向看起来“慢一档”或“滑步” ★
                float moveMag = localMove.magnitude;   // 0~1，手柄可用
                float lockMoveSpeed = 4f;              // 你原来写死的速度

                Vector3 horizontalMove = worldMoveDir * lockMoveSpeed * moveMag;
                Vector3 verticalMove = Vector3.up * _verticalVelocity;

                if (!_animator.GetBool("isSneak"))
                {

                     _controller.Move((horizontalMove + verticalMove) * Time.deltaTime);
                    //_controller.Move(worldMoveDir * 4f * moveMag * Time.deltaTime);

                    // ★ 修改 3：动画仍然用原始输入做 2D Blend，斜向不会显得“变慢”
                    float inputX_blend = Mathf.Lerp(_animator.GetFloat("inputX"), localMove.x, Time.deltaTime * 10f);
                    float inputY_blend = Mathf.Lerp(_animator.GetFloat("inputY"), localMove.z, Time.deltaTime * 10f);
                    _animator.SetFloat("inputX", inputX_blend);
                    _animator.SetFloat("inputY", inputY_blend);
                    

                }
                else
                {
                    float inputX_blend = Mathf.Lerp(_animator.GetFloat("inputX"), _input.move.x, Time.deltaTime * 10f);
                    float inputY_blend = Mathf.Lerp(_animator.GetFloat("inputY"), _input.move.y, Time.deltaTime * 10f);
                    /*_animator.SetFloat("inputX", _input.move.x);
                    _animator.SetFloat("inputY",_input.move.y);*/

                    _animator.SetFloat("inputX", inputX_blend);
                    _animator.SetFloat("inputY", inputY_blend);
                }

                return;
            }
            else
            {
                AudioManager.Instance.StopFootStep();

                if (_input.move != Vector2.zero && !isRolling)
                {

                    //_animator.applyRootMotion = false;

                    _targetRotation = _mainCamera.transform.eulerAngles.y;//潜行方向始终对着摄像机方向
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    //if(_input.move.x>0)
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    //Animator animator = GetComponent<Animator>();
                    

                }
                else
                {
                    
                }
                float inputX_blend = Mathf.Lerp(_animator.GetFloat("inputX"), _input.move.x, Time.deltaTime * 10f);
                float inputY_blend = Mathf.Lerp(_animator.GetFloat("inputY"), _input.move.y, Time.deltaTime * 10f);
                /*_animator.SetFloat("inputX", _input.move.x);
                _animator.SetFloat("inputY",_input.move.y);*/

                _animator.SetFloat("inputX", inputX_blend);
                _animator.SetFloat("inputY", inputY_blend);

            }

        }
        private void Move()
        {

            

            //if(stateInfo.IsName("Backflip"))
            //    return;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            
            if (_input.move != Vector2.zero)
            {
                
                //_animator.applyRootMotion = false;
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                //Animator animator = GetComponent<Animator>();
                AudioManager.Instance.PlayFootStepSFX();

                if (Tutorial.Instance.isFinishPerspectiveTip && !Tutorial.Instance.isFinishMoveTip)
                {
                    Tutorial.Instance.isFinishMoveTip = true;
                    Tutorial.Instance.HideTip();
                    Tutorial.Instance.ShowJumpTip();
                }

            }
            else
            {
                AudioManager.Instance.StopFootStep();
            }


                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (!canFlipToMove)
            {
                _controller.Move(targetDirection.normalized * (0f * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
            else
            {
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }
            // move the player

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _animator.applyRootMotion=false;
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }

                    if (Tutorial.Instance.isFinishMoveTip && !Tutorial.Instance.isFinishJumpTip)
                    {
                        Tutorial.Instance.isFinishJumpTip = true;
                        Tutorial.Instance.HideTip();
                        Tutorial.Instance.ShowAttackTip();
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        public void ChangeCharacterControllerRadius(float rate)
        {
            GetComponent<CharacterController>().radius = rate;
        }

        public void GetHit()
        {
            _animator.SetTrigger("getHit");
            ResetCombo();

        }

        public void BeginHit()
        {
            isHitState = true;
        }

        public void EndHit()
        {
            isHitState= false;
        }

        public void ShowSwapTip()
        {
            if (!Tutorial.Instance.isFinishSwapTip)
            {
                Tutorial.Instance.ShowSwapTip();
                //Time.timeScale = 0.05f;
            }
        }

        public void PlayHeavyAttackSound()
        {
            AudioManager.Instance.PlayAttack();
        }

        public void PlayTossSound()
        {
            AudioManager.Instance.PlayThrowFireBall();
        }

        public void DelayShowSwapTip()
        {
            Invoke("ShowSwapTip",2f);
        }

        public void EquipAxe()
        {
            axeEquipped.SetActive(true);
        }

        public void PlayerDead()
        {
            _animator.SetTrigger("isDead");
            _animator.SetBool("isRecover", false);
            Invoke("PlayerRecover", 3f);
            isDead = true;
        }

        public bool CheckDead()
        {
            return stateInfo.IsName("Dead");
        }

        public void PlayerRecover()
        {
            _animator.SetBool("isRecover",true);
            _animator.ResetTrigger("isDead");
            InitParameter();
            isDead = false;
            playerState.Heal(100);
            transform.position=bornPoint.position;
            transform.rotation=bornPoint.rotation;
        }
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void InitParameter()
        {
            isFliping = false;
            ResetCombo();
            //isRolling = false;
        }

        public void SetAttackID(int attackID)
        {
            _animator.SetInteger("attackID", attackID);
        }

        public void ChangeRightIdle()
        {
            _animator.SetFloat("HandIndex", 0f);
        }
        public void ChangeTopIdle()
        {
            _animator.SetFloat("HandIndex", 1f);
        }


        public void ChangeLeftIdle()
        {
            _animator.SetFloat("HandIndex", 2f);
            //playerWeapon.GetComponent<SwordPoseBlender>().BlendToDir(SwordPoseBlender.Dir.Left);
            //playerWeapon.GetComponent<SwordPoseBlender>().BlendToDir(SwordPoseBlender.Dir.Left);
        }

        public void ChangeDownIdle()
        {
            _animator.SetFloat("HandIndex", 3f);
            //playerWeapon.GetComponent<SwordPoseBlender>().BlendToDir(SwordPoseBlender.Dir.Down);
        }

        public void SwitchSwordPos(int index)
        {
            _animator.SetTrigger("switch");
            _animator.SetInteger("SwitchIndex", index);
            leftSword.SetActive(false);
            rightSword.SetActive(true);
            ResetLayerWeight();
        }


        public void ResetLayerWeight()
        {
            var driver = GetComponent<AnimatorLayerWeightDriver>();
            if (driver) driver.SetUpperTarget(0f);
        }


        public void BlendSwordLeft()
        {
            playerWeapon.GetComponent<SwordPoseBlender>().BlendToDir(SwordPoseBlender.Dir.Left);
            Debug.Log("触发向左");
        }


        public void BlendSwordDown()
        {
            playerWeapon.GetComponent<SwordPoseBlender>().BlendToDir(SwordPoseBlender.Dir.Down);
        }

        public void BlendSwordUp()
        {
            playerWeapon.GetComponent<SwordPoseBlender>().BlendToDir(SwordPoseBlender.Dir.Up);
        }


        public void BlendSwordRight()
        {
            playerWeapon.GetComponent<SwordPoseBlender>().BlendToDir(SwordPoseBlender.Dir.Right);
        }
    }
}