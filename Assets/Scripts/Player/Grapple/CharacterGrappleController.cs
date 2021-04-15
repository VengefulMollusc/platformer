using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
    public class CharacterGrappleController : MonoBehaviour
    {
        [SerializeField]
        string grappleInputKey = "Fire1";

        [SerializeField]
        float grappleSpeed = 1200f;

        [SerializeField]
        float grappleLaunchSpeed = 800f;

        [SerializeField]
        float adjustmentStrength = 200f;

        [SerializeField]
        CameraController cameraController;

        [SerializeField]
        float targetAngleX = 20f;

        [SerializeField]
        float targetAngleY = 40f;

        [SerializeField]
        bool debugTargeting = false;

        Transform currentAimTarget;
        Transform lockedTarget;

        List<GrappleTarget> targets;

        bool grappleIsPressed = false;
        bool grappleWasReleased = true;

        float initialDistance = 0f;

        Vector3 grappleDirection = Vector3.zero;

        protected Mover characterMover;
        protected CharacterInput characterInput;

        GrappleState currentGrappleState = GrappleState.None;

        public enum GrappleState
        {
            Grappling,
            Releasing,
            None
        }

        private void Start()
        {
            characterMover = GetComponent<Mover>();
            characterInput = GetComponent<CharacterInput>();
        }

        private void OnEnable()
        {
            if (!cameraController)
                Debug.LogWarning("No camera controller reference has been assigned to this script.", this);

            targets = new List<GrappleTarget>();
            StartCoroutine(GrappleTargetCheckCoroutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            grappleIsPressed = IsGrappleKeyPressed();

            if (currentAimTarget != null && grappleIsPressed && grappleWasReleased && currentGrappleState == GrappleState.None)
            {
                grappleWasReleased = false;
                // lock target
                lockedTarget = currentAimTarget;
                initialDistance = (lockedTarget.position - transform.position).sqrMagnitude;
                currentGrappleState = GrappleState.Grappling;
            }
            if (!grappleIsPressed && currentGrappleState == GrappleState.None)
            {
                grappleWasReleased = true;
            }
        }

        void FixedUpdate()
        {
            if (currentGrappleState == GrappleState.Grappling)
            {
                PerformGrapple();
            }
            else if (currentGrappleState == GrappleState.Releasing)
            {
                PerformRelease();
            }
        }

        private void LateUpdate()
        {
            if (debugTargeting)
            {
                Debug.DrawRay(cameraController.transform.position, cameraController.GetAimingDirection() * 10f, Color.red, Time.deltaTime);
                if (currentAimTarget != null)
                {
                    Debug.DrawLine(transform.position, currentAimTarget.position, Color.green, Time.deltaTime);
                }
            }
        }

        IEnumerator GrappleTargetCheckCoroutine()
        {
            while (true)
            {
                if (currentGrappleState == GrappleState.None && targets != null && targets.Count > 0)
                {
                    Transform bestTarget = null;
                    float bestAngle = float.MaxValue;

                    Vector3 cameraPos = cameraController.transform.position;
                    Vector3 cameraDir = cameraController.GetAimingDirection();
                    Vector3 cameraUp = cameraController.transform.up;
                    Vector3 cameraRight = cameraController.GetStrafeDirection();
                    foreach (GrappleTarget target in targets)
                    {
                        Transform targetTransform = target.GetTargetTransform();
                        Vector3 dirToTarget = targetTransform.position - cameraPos;

                        float angleX = Vector3.Angle(cameraDir, Vector3.ProjectOnPlane(dirToTarget, cameraUp));
                        if (angleX < targetAngleX)
                        {
                            float angleY = Vector3.Angle(cameraDir, Vector3.ProjectOnPlane(dirToTarget, cameraRight));
                            if (angleY < targetAngleY)
                            {
                                // TODO: add a raycast here
                                float angleTotal = angleX + angleY;
                                if (angleTotal < bestAngle)
                                {
                                    bestTarget = target.GetTargetTransform();
                                }
                            }
                        }
                    }

                    currentAimTarget = bestTarget;
                } else
                {
                    currentAimTarget = null;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void PerformGrapple()
        {
            Vector3 toTarget = lockedTarget.position - transform.position;
            grappleDirection = toTarget.normalized;
            float distToTarget = toTarget.sqrMagnitude;

            if (distToTarget < 0.5f)
            {
                currentGrappleState = GrappleState.Releasing;
                characterMover.SetVelocity(grappleDirection * Time.deltaTime * grappleLaunchSpeed, true);
            } 
            else
            {
                float speed = GeneralUtilities.MapValues(distToTarget, 0, initialDistance, grappleSpeed, grappleSpeed * 0.2f, true);
                Vector3 grappleVelocity = grappleDirection * speed;

                if (distToTarget > 2f)
                {
                    // Allow player adjustment of grapple angle
                    float moveX = characterInput.GetHorizontalMovementInput();
                    float moveY = characterInput.GetVerticalMovementInput();
                    moveY = (moveY - 1f) * 0.5f;
                    characterMover.CheckForGround();
                    if (characterMover.IsGrounded() && moveY < 0)
                    {
                        moveY = 0f;
                    }
                    Vector3 adjustVerAxis = Vector3.ProjectOnPlane(Vector3.up, grappleDirection).normalized;
                    Vector3 adjustHorAxis = Vector3.Cross(adjustVerAxis, grappleDirection).normalized;
                    Vector3 adjustment = (adjustHorAxis * moveX + adjustVerAxis * moveY) * adjustmentStrength;

                    characterMover.SetVelocity((grappleVelocity + adjustment) * Time.deltaTime, true);
                } 
                else
                {
                    characterMover.SetVelocity(grappleVelocity * Time.deltaTime, true);
                }
            }
        }

        private void PerformRelease()
        {
            // apply release launch force here
            characterMover.SetVelocity(grappleDirection * Time.deltaTime * grappleLaunchSpeed, true);

            grappleDirection = Vector3.zero;
            currentGrappleState = GrappleState.None;
            lockedTarget = null;
        }

        private bool IsGrappleKeyPressed()
        {
            return Input.GetButton(grappleInputKey);
        }

        public bool IsGrappling()
        {
            return currentGrappleState != GrappleState.None;
        }

        public void AddGrappleTarget(GrappleTarget target)
        {
            if (targets == null)
            {
                targets = new List<GrappleTarget>();
            }
            targets.Add(target);
        }

        public void RemoveGrappleTarget(GrappleTarget target)
        {
            targets.Remove(target);
        }
    }
}
