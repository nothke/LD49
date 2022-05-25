using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeet : MonoBehaviour
{
    public float maxDistanceFromBody = 0.35f;
    public float distanceBetweenFeet = 0.25f;
    public float forwardStep = 0.25f;
    public float pointOutFeetAngle = 15f;
    public float feetSpeed = 3f;
    public float stepArchHeight = 0.3f;

    public Transform rightFeet, leftFeet;

    bool rightFeetMoving = true;
    bool leftFeetMoving = true;
    float moveDistance = 1f;
    float moveTotalDistance = 1f;

    ShipPlayArea playArea;

    Vector2 fromPosR, fromPosL;
    Vector2 toPosR, toPosL;

    Quaternion fromRotR, fromRotL;
    Quaternion toRotR, toRotL;
    AudioSource leftStep, rightStep;

    const float feetPlayareaMargin = 0.3f;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetPlayArea(ShipPlayArea newArea)
    {
        if (playArea != null)
        {
            fromPosL = newArea.InverseTransformPoint(playArea.TransformPoint(fromPosL));
            fromPosR = newArea.InverseTransformPoint(playArea.TransformPoint(fromPosR));
            toPosL = newArea.InverseTransformPoint(playArea.TransformPoint(toPosL));
            toPosR = newArea.InverseTransformPoint(playArea.TransformPoint(toPosR));
        }
        playArea = newArea;
    }

    bool initialized = false;
    public void Init(Vector2 pos, Vector2 direction, UnityEngine.Audio.AudioMixerGroup soundGroup)
    {
        Vector2 playerRight = new Vector2(direction.y, -direction.x);// Vector2.playArea.InverseTransformDirection(transform.right);

        fromPosR = toPosR = pos + playerRight * distanceBetweenFeet * 0.5f;
        fromPosL = toPosL = pos - playerRight * distanceBetweenFeet * 0.5f;

        rightFeet.position = playArea.TransformPoint(fromPosR);
        leftFeet.position = playArea.TransformPoint(fromPosL);


        fromRotR = toRotR = Quaternion.identity;
        fromRotL = toRotL = Quaternion.identity;
        rightFeet.localRotation = fromRotR;
        leftFeet.localRotation = fromRotL;


        leftStep = leftFeet.GetComponent<AudioSource>();
        rightStep = rightFeet.GetComponent<AudioSource>();
        leftStep.outputAudioMixerGroup = soundGroup;
        rightStep.outputAudioMixerGroup = soundGroup;

        initialized = true;
    }

    public void UpdateFeet(Vector2 pos, Vector2 direction, bool onLand)
    {
        if (!initialized) return;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.up;

        Vector2 playerRight = new Vector2(direction.y, -direction.x);

        bool justMoved = false;
        float rightDistance = Vector2.Distance(toPosR, pos + direction * forwardStep);
        float leftDistance = Vector2.Distance(toPosL, pos + direction * forwardStep);

        if (rightDistance > leftDistance)
        {
            if (rightDistance > maxDistanceFromBody)
            {
                if (rightFeetMoving)
                {
                    fromPosR = Vector2.Lerp(fromPosR, toPosR, 0.5f);

                    toPosR = pos + playerRight * distanceBetweenFeet * 0.5f + direction * forwardStep;

                    //moveDistance = 0f;
                    //moveTotalDistance = Vector2.Distance(fromPosR, toPosR);
                    moveTotalDistance *= 0.3f;

                    toRotR = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)) * Quaternion.AngleAxis(pointOutFeetAngle, Vector3.up);
                    if (float.IsNaN(toRotR.x))
                        toRotR = fromRotR;

                    justMoved = true;
                }
                else if (!leftFeetMoving)
                {
                    MoveRightFeet(pos, direction, playerRight);
                    justMoved = true;
                }
            }
        }

        if (!justMoved && leftDistance > maxDistanceFromBody) {
            if (leftFeetMoving)
            {
                fromPosL = Vector2.Lerp(fromPosL, toPosL, 0.5f);

                toPosL = pos - playerRight * distanceBetweenFeet * 0.5f + direction * forwardStep;

                //moveDistance = 0f;
                //moveTotalDistance = Vector2.Distance(fromPosL, toPosL);
                moveTotalDistance *= 0.3f;

                toRotL = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)) * Quaternion.AngleAxis(-pointOutFeetAngle, Vector3.up);
                if (float.IsNaN(toRotL.x))
                    toRotL = fromRotL;
                justMoved = true;
            }
            else if (!rightFeetMoving)
            {
                MoveLeftFeet(pos, direction, playerRight);
                justMoved = true;
            }
        }

        if (!justMoved && !leftFeetMoving && !rightFeetMoving)
        {
            if (Vector2.Distance(toPosL, toPosR) < distanceBetweenFeet * 0.9f)
            {
                if (leftDistance < rightDistance)
                    MoveRightFeet(pos, direction, playerRight);
                else MoveLeftFeet(pos, direction, playerRight);
            }
        }

        // Update
        if (leftFeetMoving || rightFeetMoving)
        {
            float speed = feetSpeed;
            if ((rightFeetMoving && leftDistance > maxDistanceFromBody) ||
                (leftFeetMoving && rightDistance > maxDistanceFromBody)) speed *= 3.5f;

            moveDistance += speed * Time.deltaTime;
            float moveFactor = Mathf.Clamp01(moveDistance / moveTotalDistance);

            float instantHeight = moveFactor * 2f - 1f;
            instantHeight *= instantHeight;
            instantHeight = (1f - instantHeight) * stepArchHeight;

            if (leftFeetMoving)
            {
                Vector2 lerpedP = Vector2.Lerp(fromPosL, toPosL, Easing.Cubic.Out(moveFactor));

                Vector3 aux = new Vector3(lerpedP.x, 0, lerpedP.y);
                playArea.EnsureCircleInsideArea(ref aux, feetPlayareaMargin);
                lerpedP = new Vector2(aux.x, aux.z);

                leftFeet.position = playArea.TransformPoint(lerpedP) + playArea.areaCenter.up * instantHeight;
                leftFeet.localRotation = Quaternion.Lerp(fromRotL, toRotL, moveFactor);

                if (moveFactor >= 1)
                {
                    leftFeetMoving = false;
                    if (onLand)
                    {
                        if (leftStep.isPlaying) leftStep.Stop();
                        leftStep.Play();
                    }
                }
            }
            if (rightFeetMoving)
            {
                Vector2 lerpedP = Vector2.Lerp(fromPosR, toPosR, Easing.Cubic.Out(moveFactor));

                Vector3 aux = new Vector3(lerpedP.x, 0, lerpedP.y);
                playArea.EnsureCircleInsideArea(ref aux, feetPlayareaMargin);
                lerpedP = new Vector2(aux.x, aux.z);

                rightFeet.position = playArea.TransformPoint(lerpedP) + playArea.areaCenter.up * instantHeight;
                rightFeet.localRotation = Quaternion.Lerp(fromRotR, toRotR, moveFactor);

                if (moveFactor >= 1)
                {
                    rightFeetMoving = false;
                    if (onLand)
                    {
                        if (rightStep.isPlaying) rightStep.Stop();
                        rightStep.Play();
                    }
                }
            }
        }

        if (!rightFeetMoving)
        {
            Vector2 p = toPosR;
            Vector3 aux = new Vector3(p.x, 0, p.y);
            playArea.EnsureCircleInsideArea(ref aux, feetPlayareaMargin);
            p = new Vector2(aux.x, aux.z);

            rightFeet.position = playArea.TransformPoint(p);
            rightFeet.rotation = toRotR * playArea.areaCenter.rotation;
        }
        if (!leftFeetMoving)
        {
            Vector2 p = toPosL;
            Vector3 aux = new Vector3(p.x, 0, p.y);
            playArea.EnsureCircleInsideArea(ref aux, feetPlayareaMargin);
            p = new Vector2(aux.x, aux.z);

            leftFeet.position = playArea.TransformPoint(p);
            leftFeet.rotation = toRotL * playArea.areaCenter.rotation;
        }
    }

    void MoveRightFeet(Vector2 pos, Vector2 direction, Vector2 playerRight) {
        fromPosR = toPosR;
        toPosR = pos + playerRight * distanceBetweenFeet * 0.5f + direction * forwardStep;
        fromRotR = toRotR;
        toRotR = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)) * Quaternion.AngleAxis(pointOutFeetAngle, Vector3.up);
        if (float.IsNaN(toRotR.x))
            toRotR = fromRotR;

        moveDistance = 0f;
        moveTotalDistance = Mathf.Min(1.5f, Vector2.Distance(fromPosR, toPosR));
        rightFeetMoving = true;
    }

    void MoveLeftFeet(Vector2 pos, Vector2 direction, Vector2 playerRight)
    {
        fromPosL = toPosL;
        toPosL = pos - playerRight * distanceBetweenFeet * 0.5f + direction * forwardStep;
        fromRotL = toRotL;
        toRotL = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)) * Quaternion.AngleAxis(-pointOutFeetAngle, Vector3.up);
        if (float.IsNaN(toRotL.x))
            toRotL = fromRotL;

        moveDistance = 0f;
        moveTotalDistance = Mathf.Min(1.5f, Vector2.Distance(fromPosL, toPosL));
        leftFeetMoving = true;
    }
}
