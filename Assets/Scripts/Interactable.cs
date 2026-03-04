using UnityEngine;
using System;

public abstract class Interactable : MonoBehaviour
{

    protected bool canInteract = false;
    public event Action OnInteractEvent;
    protected virtual void Awake()
    {
        // 确保 Collider 为触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 统一玩家检测逻辑
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            OnEnter();
        }

        if (other.CompareTag("PlayerFoot"))
        {
            OnHideWall();
            OnShowWall();
            Debug.Log("检测到玩家躯体");
        }

        if (other.CompareTag("boss"))
        {
            OnBossEnter();
        }

        if (other.CompareTag("codepiece"))
        {
            OnCodeEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            OnExit();
        }       
    }

    protected virtual void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
            Debug.Log("interact");
        }

        //Debug.Log("interact");
    }

    /// <summary>
    /// 玩家按下 E 键时调用
    /// </summary>
    protected virtual void Interact()
    {
        // 通知订阅者
        OnInteractEvent?.Invoke();
        OnInteract();
        
    }

    // 子类重写这两个方法
    protected virtual void OnEnter() { }

    protected virtual void OnBossEnter() { }
    protected virtual void OnExit() { }

    protected virtual void OnInteract() { }

    protected virtual void OnShowWall() { }

    protected virtual void OnHideWall() { }

    protected virtual void OnCodeEnter() { }
}
