using UnityEngine;
using System.Collections;

public class IKFollow : MonoBehaviour
{
    [System.NonSerialized] public Animator animator;

    [Header("Head")]
    [Range(0, 1)]
    public float lookWeight;
    public Transform lookTarget;

    [Header("Hands")]
    [Range(0, 1)]
    public float leftHandWeight;
    public Transform leftHandTarget;

    [Range(0, 1)]
    public float rightHandWeight;
    public Transform rightHandTarget;

    [Header("Elbows")]
    [Range(0, 1)]
    public float leftElbowWeight = 1;
    public Transform leftElbowTarget;
    [Range(0, 1)]
    public float rightElbowWeight = 1;
    public Transform rightElbowTarget;

    [Header("Legs")]
    public Transform leftLegTarget;
    public Transform rightLegTarget;

    [Header("Knees")]
    public Transform leftKneeTarget;
    public Transform rightKneeTarget;

    public Rigidbody targetRigidbody;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (targetRigidbody)
        {
            transform.position = targetRigidbody.transform.position;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator != null)
        {
            if (rightHandTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);

                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }

            if (leftHandTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);

                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            }

            if (rightLegTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightLegTarget.position);

                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightLegTarget.rotation);
            }

            if (leftLegTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftLegTarget.position);

                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftLegTarget.rotation);
            }

            if (leftKneeTarget)
            {
                animator.SetIKHintPosition(AvatarIKHint.LeftKnee, leftKneeTarget.position);
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
            }

            if (rightKneeTarget)
            {
                animator.SetIKHintPosition(AvatarIKHint.RightKnee, rightKneeTarget.position);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
            }


            if (leftElbowTarget)
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftElbowWeight);
                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowTarget.position);
            }

            if (rightElbowTarget)
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightElbowWeight);
                animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowTarget.position);
            }

            if (lookTarget)
            {
                animator.SetLookAtPosition(lookTarget.position);
                animator.SetLookAtWeight(lookWeight);
            }
        }
    }
}

