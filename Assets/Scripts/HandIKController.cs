using UnityEngine;

public class HandIKController : MonoBehaviour
{
    public Animator animator;

    [Header("Target")]
    public Transform leftHandTarget;

    [Header("IK Weight")]
    public bool enableIK = true;
    [Range(0f, 1f)] public float positionWeight = 1f;
    [Range(0f, 1f)] public float rotationWeight = 1f;

    private void Reset()
    {
        animator = GetComponentInParent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || leftHandTarget == null||!enableIK) return;

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, positionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, rotationWeight);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
    }

    public void OpenIK()
    {
        enableIK = true;
    }
    public void CloseIK() 
    {
        enableIK = false;
    }
}
