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
    float moveDistance = 0f;
    float moveTotalDistance = 0f;

    public ShipPlayArea playArea;

    Vector2 fromPosR, fromPosL;
    Vector2 toPosR, toPosL;

    Quaternion fromRotR, fromRotL;
    Quaternion toRotR, toRotL;

    // Start is called before the first frame update
    void Start()
    {


    }

    public void Init(Vector2 pos, Vector2 direction)
    {
        Vector2 playerRight = new Vector2(direction.y, -direction.x);// Vector2.playArea.InverseTransformDirection(transform.right);

        fromPosR = toPosR = pos + playerRight * distanceBetweenFeet * 0.5f;
        fromPosL = toPosL = pos - playerRight * distanceBetweenFeet * 0.5f;

        rightFeet.position = playArea.TransformPoint(fromPosR);
        leftFeet.position = playArea.TransformPoint(fromPosL);


        fromRotR = toRotR =  Quaternion.identity;
        fromRotL = toRotL = Quaternion.identity;
        rightFeet.localRotation = fromRotR;
        leftFeet.localRotation = fromRotL;
    }

    public void UpdateFeet(Vector2 pos, Vector2 direction)
    {
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

                    toRotR = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)) * Quaternion.AngleAxis(pointOutFeetAngle, Vector3.up);
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

                toRotL = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)) * Quaternion.AngleAxis(-pointOutFeetAngle, Vector3.up);
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
            moveDistance += feetSpeed * Time.deltaTime;
            float moveFactor = Mathf.Clamp01(moveDistance / moveTotalDistance);

            float instantHeight = moveFactor * 2f - 1f;
            instantHeight *= instantHeight;
            instantHeight = (1f - instantHeight) * stepArchHeight;

            if (leftFeetMoving)
            {
                Vector2 lerpedP = Vector2.Lerp(fromPosL, toPosL, Easing.Cubic.Out(moveFactor));

                leftFeet.position = playArea.TransformPoint(lerpedP) + playArea.areaCenter.up * instantHeight;
                leftFeet.localRotation = Quaternion.Lerp(fromRotL, toRotL, moveFactor);

                if (moveFactor >= 1)
                {
                    leftFeetMoving = false;
                }
            }
            if (rightFeetMoving)
            {
                Vector2 lerpedP = Vector2.Lerp(fromPosR, toPosR, Easing.Cubic.Out(moveFactor));

                rightFeet.position = playArea.TransformPoint(lerpedP) + playArea.areaCenter.up * instantHeight;
                rightFeet.localRotation = Quaternion.Lerp(fromRotR, toRotR, moveFactor);

                if (moveFactor >= 1)
                {
                    rightFeetMoving = false;
                }
            }
        }

        if (!rightFeetMoving)
        {
            rightFeet.position = playArea.TransformPoint(toPosR);
            rightFeet.rotation = toRotR * playArea.areaCenter.rotation;
        }
        if (!leftFeetMoving) {
            leftFeet.position = playArea.TransformPoint(toPosL);
            leftFeet.rotation = toRotL * playArea.areaCenter.rotation;
        }
    }

    void MoveRightFeet(Vector2 pos, Vector2 direction, Vector2 playerRight) {
        fromPosR = toPosR;
        toPosR = pos + playerRight * distanceBetweenFeet * 0.5f + direction * forwardStep;
        fromRotR = toRotR;
        toRotR = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)) * Quaternion.AngleAxis(pointOutFeetAngle, Vector3.up);

        moveDistance = 0f;
        moveTotalDistance = Vector2.Distance(fromPosR, toPosR);
        rightFeetMoving = true;
    }

    void MoveLeftFeet(Vector2 pos, Vector2 direction, Vector2 playerRight)
    {
        fromPosL = toPosL;
        toPosL = pos - playerRight * distanceBetweenFeet * 0.5f + direction * forwardStep;
        fromRotL = toRotL;
        toRotL = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)) * Quaternion.AngleAxis(-pointOutFeetAngle, Vector3.up);

        moveDistance = 0f;
        moveTotalDistance = Vector2.Distance(fromPosL, toPosL);
        leftFeetMoving = true;
    }
}
