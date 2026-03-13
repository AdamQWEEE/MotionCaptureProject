using StarterAssets;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;



public enum Faction
{
    Warrior,
    Archer,
    Boss,
}



public class EnemyBase : MonoBehaviour
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public Faction faction;
    public bool isDark;
    public SkinnedMeshRenderer bodyMesh;
    public Material bossGlowMaterial;
    public Material bossDarkMaterial;

    [Header("玩家")]
    public ThirdPersonController player;

    [Header("Lock-On")]
    //[Tooltip("锁定时摄像机/准星指向的位置（一般挂在胸口/头部空物体上）")]
    //public Transform lockOnPoint;
    [Tooltip("是否允许被锁定")]
    public bool canBeLocked = true;
    [Tooltip("锁定优先级（Boss/精英可以设大一点）")]
    public float lockOnPriority = 0f;

    [Header("Health")]
    public float maxHealth = 100f;

    [SerializeField, Tooltip("初始血量，为空则自动 = maxHealth")]
    public float currentHealth;

    [Header("Components (可选)")]
    public Animator animator;
    public Collider mainCollider;
    public NavMeshAgent agent;

    // 状态属性
    public bool IsDead { get; private set; }

    // 事件（用于 UI / 掉落等）
    public event Action<EnemyBase, float, float> OnHealthChanged; // (enemy, current, max)
    public event Action<EnemyBase> OnKilled;

    [Header("UI血条")]
    public WorldSpaceHealthBar hpBar;

    [Header("UI架势条")]
    public EnemyStanceBar stanceBar;

    [Header("怪物模型")]
    public EnemyModel enemyModel;

    public bool canFallBackMove;
    public bool canHitBackMove;

    [Header("刚体组件")]
    //public Rigidbody rb;

    [Header("KnockBack")]

    public float knockbackHorizontal = 4f;   // 水平方向力度
    public float knockbackVertical = 3f;   // 向上力度

    [Header("感知 & 距离")]
    public float sightRange = 12f;      // 发现玩家距离
    public float chaseRange = 15f;      // 超出则放弃追击
    public float attackRange = 2.5f;     // 进入攻击距离

    [Header("移动速度")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float springSpeed = 12f;
    public float backOffSpeed = 4f;
    public float backOffDistance = 2f;

    [Header("巡逻点(可选)")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 1.5f;

    [Header("回位点")]
    public Transform homePoint; //回位点
    public float backHomeSpeed = 3.5f;
    public float losePlayerTime = 3f;
    Vector3 _initialPosition;
    float _losePlayerTimer;

    [Header("攻击节奏（秒）")]
    public float attackRecoverDelay = 0.3f;  // 每段攻击间的间隔
    public bool isJumpAttack; //是否是跳劈
    public bool canApplyDamage;

    [Header("击退效果")]
    public float hitBackForce = 4f;
    public float hitUpForce = 2f;
    public float stunDuration = 0.25f;     // 被打硬直时间
    public bool isHit;
    public bool canGetHitBack;

    [Header("后撤步")]
    public float retreatMinDistance = 6f;   // 撤退到离玩家至少这么远
    public float retreatMaxTime = 2f;
    private float _retreatTimer;

    [Header("回旋镖")]
    public Boomerang boomerangPrefab;
    public Transform throwPoint;

    [Header("动画参数")]
    public int _patrolIndex = 0;
    public bool _isAttacking = false;
    public bool _isStunned = false;
    public bool _superArmor = false;

    [Header("被处决")]
    [SerializeField] private LockOnMarker executionMarkPrefab;
    private Transform currExecutionMarker;
    public Transform BloodVfxPrefab;
    private Transform currBloodVfx;
    public Transform BloodPoint;

    public float changeColorCoolTime=30f;



    [Header("是否锁血")]
    public bool isLockHp;

    [Header("AxeReward")]
    public Transform axeRewardPrefab;

    [Header("Wintext")]
    public GameObject winText;
    public GameObject divinity;





    private AnimatorStateInfo currstate;

    public enum EnemyState
    {
        Idle,
        Patrol,
        ReturnHome,
        Chase,
        GetHit,
        Attack,
        Retreat,
        ChangeYinYang,
        AttackSprint,
        GetExecuted,
        Dead
    }

    [Header("敌人当前状态")]
    public EnemyState state = EnemyState.Idle;
   
   

    [Header("Behaviour")]
    public bool canPatrol = true;

    protected virtual void Awake()
    {
        //if (currentHealth <= 0f)
        //    currentHealth = maxHealth;

        //if (mainCollider == null)
        //    mainCollider = GetComponent<Collider>();
        //agent=GetComponent<NavMeshAgent>();
        //animator = GetComponent<Animator>();
        ////rb = GetComponent<Rigidbody>();
        //SetNavMode(true);
    }

    private void Start()
    {
        if (homePoint == null)
        {
            // 默认就把出生点当作回位点
            homePoint = transform;
        }

        if (hpBar != null)
        {
            hpBar.maxHp = maxHealth;
            hpBar.currentHp = maxHealth;
        }
        //enemyModel = GetComponent<EnemyModel>();
        
        player = ThirdPersonController.Instance;
        
    }

    protected virtual void OnEnable()
    {
        EnemyManager.Register(this);
        //state = patrolPoints != null && patrolPoints.Length > 0 ? EnemyState.Patrol : EnemyState.Idle;
    }

    protected virtual void OnDisable()
    {
        // 注意：OnDisable 也会在场景卸载/对象销毁时调用
        EnemyManager.Unregister(this);
    }

    private void Update()
    {
        //if (changeColorCoolTime<0f && !player.canExecute && CameraModeController.Instance.vcamExecute.Priority == 0)
        //{
        //    ChangeState(EnemyState.ChangeYinYang);
        //    changeColorCoolTime = 30f;
        //}
        //changeColorCoolTime-=Time.deltaTime;

        //FallBackMove();
        //HitBackMove();

        //if (state == EnemyState.Dead || player == null) return;

        //switch (state)
        //{
        //    case EnemyState.Idle: UpdateIdle(); break;
        //    case EnemyState.Patrol: UpdatePatrol(); break;
        //    case EnemyState.Chase: UpdateChase(); break;
        //    case EnemyState.GetHit: UpdateGetHit(); break;
        //    case EnemyState.ReturnHome: UpdateReturnHome(); break;
        //    case EnemyState.Attack: UpdateAttack(); break;
        //    case EnemyState.Retreat: UpdateRetreat(); break;
        //    case EnemyState.AttackSprint: UpdateAttackSprint(); break;
        //    case EnemyState.GetExecuted: break;
        //    case EnemyState.ChangeYinYang: break;
        //    case EnemyState.Dead:  break;
        //}

        //currstate = animator.GetCurrentAnimatorStateInfo(0);
        


    }

    void ChangeState(EnemyState newState)
    {
        if (state == newState) return;
        state = newState;
        

        switch (state)
        {
            case EnemyState.Chase:
                animator.applyRootMotion=false;
                break;

            case EnemyState.Attack:
                if(agent.enabled) agent.isStopped = true;
                SetNavMode(false);
                animator.applyRootMotion = true;
                animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
                if (!_isAttacking) StartAttack(); //StartCoroutine(AttackLoop());
                _isAttacking = true;
                break;

            case EnemyState.ChangeYinYang:
                animator.SetTrigger("ChangeColor");
                SetNavMode(false);
                _isAttacking=false;
                break;           
        }
    }

    void UpdateIdle()
    {
        FacePlayerIfNear();
        animator.SetFloat("MoveSpeed", 0f);
        animator.SetBool("isRetreat",false);

        // 看见玩家就进入追击
        if (currstate.IsName("retreatIdle")) return;

        if (DistanceToPlayer() <= sightRange &&!isHit)
        {
            ChangeState(EnemyState.Chase);
        }

        if (canPatrol && patrolPoints != null && patrolPoints.Length > 0 )
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    void UpdatePatrol()
    {
        if (currstate.IsName("retreatIdle")) { 

            return;
        }
        if (!canPatrol)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        agent.enabled = true;
        agent.speed = patrolSpeed;
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
        agent.isStopped = false;

        Transform targetPoint = patrolPoints[_patrolIndex];
        agent.SetDestination(targetPoint.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StartCoroutine(SwitchPatrolPointAfterWait());
        }

        if (DistanceToPlayer() <= sightRange && !isHit)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    IEnumerator SwitchPatrolPointAfterWait()
    {
        // 避免多个协程叠加
        if (state != EnemyState.Patrol) yield break;

        state = EnemyState.Idle;
        agent.isStopped = true;
        animator.SetFloat("MoveSpeed", 0f);

        yield return new WaitForSeconds(patrolWaitTime);

        if (state == EnemyState.Idle)
        {
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            ChangeState(EnemyState.Patrol);
        }
    }

    void UpdateChase()
    {
        if (currstate.IsName("retreatIdle")) return;
        float dist = DistanceToPlayer();

        if (dist > chaseRange)
        {
            // 超出追击范围，回到巡逻 / Idle
            ChangeState(EnemyState.ReturnHome);
            return;
        }

        agent.enabled = true;       
        agent.speed = chaseSpeed;
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
        
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);

        

        FacePlayerIfNear();

        if (dist <= attackRange && !_isAttacking && !_isStunned)
        {
            ChangeState(EnemyState.Attack);
        }
    }

    void UpdateReturnHome()
    {
        agent.isStopped = false;
        agent.speed = backHomeSpeed;
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);

        Vector3 targetPos = homePoint.position;       // 或 homePoint.position
        agent.SetDestination(targetPos);

        // 如果离原位足够近，就认为已经回到了
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            agent.isStopped = true;

            // ★ 回来之后根据类型决定去 Idle 还是 Patrol
            if (canPatrol && patrolPoints != null && patrolPoints.Length > 0)
                ChangeState(EnemyState.Patrol);
            else
                ChangeState(EnemyState.Idle);

            return;
        }
    }

    void UpdateGetHit()
    {
        if (isHit)
        {
            SetNavMode(false);
            //agent.isStopped = true;
            //agent.speed = 0f;
            animator.SetFloat("MoveSpeed", 0f);
        }
        else
        {
            SetNavMode(true);
            agent.enabled = true;
            ChangeState(EnemyState.Idle);
        }
    }

    public void FinishGetHit()
    {
        isHit = false;
        _isAttacking = false;
    }

    void UpdateAttack()
    {
        // 攻击状态主要由协程驱动，这里只负责面向玩家
        FacePlayerIfNear();
    }

    void StartAttack()
    {
        _isAttacking = true;

        if (state == EnemyState.Attack && !_isStunned && state != EnemyState.Dead)
        {
            
            FacePlayerIfNear();
            animator.SetBool("isJumpAttack", isJumpAttack);
            animator.SetTrigger("Combo");
                       
        }

        //_isAttacking = false;
    }

    private void SetAttackRange()
    {
       attackRange=isJumpAttack? 3f : 1.5f;
    }

    public void ShowCounterTip()
    {
        
        
        Tutorial.Instance.ShowCounterTip();
        
    }

    public void ShowRollTip()
    {
        
        
        Tutorial.Instance.ShowRollTip();
        
    }

    public void ChangePlayerRadius()
    {
        player.ChangeCharacterControllerRadius(0.32f);
    }

    public void ResetPlayerRadius()
    {
        player.ChangeCharacterControllerRadius(0.25f);
        
    }

    public void HideAllTip()
    {
        Time.timeScale = 1f;
        Tutorial.Instance.HideTip();
    }


    public void StartRetreat()
    {
        isJumpAttack = !isJumpAttack;
        SetAttackRange();
        CancelInvoke("BackIdle");
        StopAllCoroutines();
        _isAttacking = false;
        if (state == EnemyState.Dead) return;

        // 先面向玩家
        FacePlayerIfNear();

        // 切换成 Root Motion 模式
        SetNavMode(false);

        // 计时器，用来防止动画/导航问题导致一直撤退
        _retreatTimer = retreatMaxTime;

        // 发动画 Trigger：AnyState → dodgeback → runback
        animator.ResetTrigger("Combo");   // 视你自己的参数而定，可有可无
        animator.SetTrigger("DodgeBack");
        animator.SetBool("isRetreat", true);

        ChangeState(EnemyState.Retreat);
    }

    void UpdateRetreat()
    {
        FacePlayerIfNear();

        // 计时
        _retreatTimer -= Time.deltaTime;

        // 距离判断
        float dist = DistanceToPlayer();

        // 到达预期距离 或 时间超时 → 回到追击
        if (dist >= retreatMinDistance || _retreatTimer <= 0f)
        {
            
            // 回到导航驱动
            animator.SetBool("isRetreat",false);
            //Invoke("BackIdle", 1f);
        }

        
    }
    public void ChangeAfterThrow()
    {
        CancelInvoke();
        if (isJumpAttack)
        {
            if(DistanceToPlayer()>attackRange)
                ChangeState(EnemyState.AttackSprint);
            else
                ChangeState(EnemyState.Attack);
        }
        else
        {
            BackIdle();
        }
    }

    void UpdateAttackSprint()
    {
        //SetNavMode(false);

        if (player == null)
        {
            // 没玩家就回到 Idle（或 ReturnHome）
            SetNavMode(true);
            ChangeState(EnemyState.Idle);
            return;
        }

        // 指向玩家的方向（只算水平）
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;
        if (dist <= attackRange)
        {
            // 进入攻击 / 跳劈
            ChangeState(EnemyState.Attack);
            return;
        }

        if (dist > 0.0001f)
        {
            Vector3 dir = toPlayer / dist; // 等价于 normalized

            // 位移
            transform.position += dir * 12f * Time.deltaTime;

            // 朝向插值到玩家方向
            FacePlayerIfNear();
        }
    }

    public void BackIdle()
    {
        CancelInvoke();
        //SetNavMode(true);
        ChangeState(EnemyState.Idle);
        Debug.Log("回到idle");
    }

    float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, player.transform.position);
    }

    void FacePlayerIfNear()
    {
        if (player == null) return;

        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        
    }

    #region 生命 / 受击 / 死亡

    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        if (hpBar != null)
            hpBar.TakeDamage(amount);

        OnHealthChanged?.Invoke(this, currentHealth, maxHealth);

        // 播放受击反馈
        //OnHit(amount, hitPoint, hitNormal);

        if (currentHealth <= 0f)
        {
            ChangeState(EnemyState.Dead);
            Die();
        }
    }

    // 如果你不关心 hitPoint，可以用这个重载
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, transform.position, Vector3.up);
    }

    public void WaveDamage(float amount)
    {
        if (currentHealth - amount >= 0)
        {
            TakeDamage(amount);
        }
        else
        {
            TakeDamage(currentHealth-1);
        }
    }

    public void AddStance(float amount)
    {
        stanceBar.AddStance(amount);
    }

    /// <summary>
    /// 敌人被击中时的通用处理（播放受击动画、硬直等），子类覆写。
    /// </summary>
    protected virtual void OnHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 示例：小硬直动画
        if (animator != null)
        {
            //animator.SetTrigger("Hit");
        }

        // TODO: 在这里触发受击特效 / 闪白等
    }

    /// <summary>
    /// 敌人死亡时的通用处理，子类可以覆写扩展（掉落、特殊演出等）。
    /// </summary>
    protected virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.speed = 0f;
            agent.enabled=false;
        }
        animator.SetFloat("MoveSpeed", 0f);
        hpBar.gameObject.SetActive(false);
        stanceBar.gameObject.SetActive(false);
        ThirdPersonController.Instance.ReleaseLock();
        //ThirdPersonController.Instance.isExecutingView = false;

        // 禁用碰撞
        //if (mainCollider != null)
        //    mainCollider.enabled = false;

        // 播放死亡动画
        if (animator != null)
        {
            animator.SetBool("Dead", true);
            animator.SetTrigger("Die");
        }

        canBeLocked = false;
        enemyModel.Die();
        OnKilled?.Invoke(this);
        Invoke("DropAxe", 1.9f);
        Destroy(gameObject, 2f);


    }

    #endregion

    #region 朝向 / 工具方法（AI 可复用）

    /// <summary>
    /// 让敌人平滑朝向某个位置（只转 Y 轴）。
    /// </summary>
    protected void FaceTarget(Vector3 worldPos, float turnSpeedDegPerSec)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion current = transform.rotation;
        Quaternion target = Quaternion.LookRotation(dir);

        float maxStep = turnSpeedDegPerSec * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(current, target, maxStep);
    }

    #endregion

    public void FallBack()
    {
        StopAllCoroutines();
        _isAttacking = false;
        animator.applyRootMotion = false;
        CancelInvoke();
        enemyModel.FallBack();
        canFallBackMove = true;
        isHit = true;
        SetNavMode(false);
        ChangeState(EnemyState.GetHit);
        
    }

    public void LoseBalance()
    {
        animator.SetTrigger("LoseBalance");
        SetNavMode(false);
        isHit = true;
        ChangeState(EnemyState.GetHit);
    }

    public void FallBackMove()
    {
        if (canFallBackMove)
        {
            transform.position+= transform.forward * (-2f) * Time.deltaTime;
        }
    }

    public void OpenDamageWindow()
    {
        canApplyDamage = true;
        //PlayAxeSound();
        //Debug.Log("触发攻击");
    }

    public void CloseDamageWindow()
    {
        canApplyDamage = false;
    }

    public void StopFallBackMove()
    {
        canFallBackMove=false;
    }

    public void HitBack()
    {
        enemyModel.GetHit();
        canHitBackMove = true;
       // GetComponent<Rigidbody>().addvel
    }

    public void ApplyKnockback(Vector3 attackerPosition)
    {
        if (!canGetHitBack) return;
        //StopAllCoroutines();
        CancelInvoke();
        enemyModel.GetHit();
        SetNavMode(false);

        isHit=true;
        //Invoke("ChanceRetreat", 1.5f);
        Invoke("FinishGetHit", 1.5f);
        
        ChangeState(EnemyState.GetHit);

    }

    public void PlayAxeSound()
    {
        AudioManager.Instance.PlayAxeAttack();
    }

    public void PlayJumpAttackSound()
    {
        AudioManager.Instance.PlayJumpAttack();
    }

    private void ChanceRetreat()
    {
        if (UnityEngine.Random.Range(0,100)>40)
        {
            StartRetreat();
        }
        else
        {

        }
    }

    public void HitBackMove()
    {
        if (canHitBackMove)
        {
            transform.position += transform.forward * (-1f) * Time.deltaTime;
        }
    }

    public void StopHitBackMove()
    {
        canHitBackMove=false;
    }

    public void ShowExecutionMarker()
    {
        if(transform.GetChild(0).childCount!=0)
            Destroy(transform.GetChild(0).transform.GetChild(0).gameObject);
        currExecutionMarker = Instantiate(executionMarkPrefab, transform.GetChild(0)).transform;
        player.canExecute = true;
        //ChangeState(EnemyState.GetExecuted);
        Debug.Log("生成斩杀点");
        player.StartLock();
        Tutorial.Instance.ShowExecutionTip();
        //CameraModeController.Instance.SetLockOn(true,transform);
        AudioManager.Instance.PlayExecute();
    }

    public void HideExecutionMarker()
    {
        Destroy(currExecutionMarker.gameObject);
    }

    public void PlayExecutionAnim()
    {
        animator.SetTrigger("TakeExecution");
        Invoke("Recover", 1.8f);
    }

    public void ShowBloodEffect()
    {
        if(currBloodVfx!=null)
            Destroy(currBloodVfx.gameObject);
        currBloodVfx = Instantiate(BloodVfxPrefab,BloodPoint);
        currBloodVfx.parent = null;
        TakeDamage(10f);
        AudioManager.Instance.PlayExecuteDamage();
        

    }


    private void Recover()
    {
        animator.SetTrigger("Recover");
        stanceBar.ResetStance();
    }

    public void ThrowBoomerang()
    {
        if (boomerangPrefab == null || throwPoint == null || player == null) return;

        var boomerang = Instantiate(boomerangPrefab, throwPoint.position, throwPoint.rotation);

        Vector3 initialDir = (player.transform.position + Vector3.up * 1.0f - throwPoint.position).normalized;

        boomerang.Init(
            owner: transform,
            initialDir: initialDir,
            player: player.transform,
            useHomingOnOut: false   // 想试弱追踪就改成 true
        );
        AudioManager.Instance.PlayThrow();
    }
    public void EnableNavMesh()
    {
        SetNavMode(true);
        Debug.Log("回到正常");
    }

    public void ClosePlayerDamageWindow()
    {
        isLockHp = true;
    }

    public void OpenPlayerDamageWindow()
    {
        isLockHp=false;
    }

    public void CloseHitBackWindow()
    {
        canGetHitBack=false;
    }
    public void OpenHitBackWindow()
    {
        canGetHitBack = true;
    }

    public void DropAxe()
    {
        Instantiate(axeRewardPrefab).position=transform.position+new Vector3(0,0.2f,0);
        winText.SetActive(true);
        divinity.SetActive(true);
    }
    void SetNavMode(bool useNav)
    {
        if (useNav)
        {
            // 导航接管
            //rb.isKinematic = false;
            //rb.linearVelocity = Vector3.zero;
            agent.enabled = true;
            animator.applyRootMotion = false;
        }
        else
        {
            //rb.isKinematic = true;
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
            animator.applyRootMotion = true;

        }
    }



}
