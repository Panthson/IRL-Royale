using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A simple camera controller, to allow for user-controlled movement in example scenes.
/// </summary>
/// <remarks>
/// Intended to be attached to the <see cref="Camera"/> <see cref="GameObject"/> being controlled.
/// <para>
/// Movement is performed via WASD keys (transverse), QE (up/down) and arrow keys (rotation).
/// </para></remarks>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
  [Tooltip("Movement speed when pressing movement keys (WASD for panning, QE for up/down).")]
  public float MovementSpeed = 200f;

  [Tooltip("Rotation speed when pressing arrow keys.")]
  public float RotationSpeed = 100f;

  [Tooltip("Minimum height off the ground.")]
  public float MinHeight = 2f;

  [Tooltip("Maximum height off the ground.")]
  public float MaxHeight = 600f;

  [Tooltip("Minimum angle above ground.")]
  public float MinXRotation = 0;

  [Tooltip("Maximum angle above ground.")]
  public float MaxXRotation = 90;

  /// <summary>
  /// Optional <see cref="Action"/> called whenever the <see cref="Camera"/> is moved in any way.
  /// </summary>
  /// <remarks>
  /// Passes in the amount moved so the type/direction of movement can be queried.
  /// </remarks>
  public MoveEvent OnMove = new MoveEvent();

  /// <summary>
  /// Optional <see cref="Action"/> called whenever the <see cref="Camera"/> is rotated.
  /// </summary>
  public UnityEvent OnRotate = new UnityEvent();

  /// <summary>
  /// Optional <see cref="Action"/> called whenever the <see cref="Camera"/> is moved or rotated
  /// in any way.
  /// </summary>
  public UnityEvent OnTransform = new UnityEvent();

  private void Update() {
    // Determine which keys are currently being pressed.
    bool pressingW = Input.GetKey(KeyCode.W);
    bool pressingS = Input.GetKey(KeyCode.S);
    bool pressingA = Input.GetKey(KeyCode.A);
    bool pressingD = Input.GetKey(KeyCode.D);
    bool pressingQ = Input.GetKey(KeyCode.Q);
    bool pressingE = Input.GetKey(KeyCode.E);
    bool pressingUp = Input.GetKey(KeyCode.UpArrow);
    bool pressingDown = Input.GetKey(KeyCode.DownArrow);
    bool pressingLeft = Input.GetKey(KeyCode.LeftArrow);
    bool pressingRight = Input.GetKey(KeyCode.RightArrow);

    // Convert to simple summaries of whether movement and/or rotation is required this frame.
    bool isMoving = pressingW || pressingS || pressingA || pressingD || pressingQ || pressingE;
    bool isRotating = pressingUp || pressingDown || pressingLeft || pressingRight;
    bool isChanging = isMoving || isRotating;

    // If no change is to be applied this frame, we skip any further processing.
    if (!isChanging) {
      return;
    }

    // Convert key presses to directions of movement and rotation.
    float xInput = pressingD ? 1 : pressingA ? -1 : 0;
    float yInput = pressingE ? 1 : pressingQ ? -1 : 0;
    float zInput = pressingW ? 1 : pressingS ? -1 : 0;
    float rotX = pressingDown ? 1 : pressingUp ? -1 : 0;
    float rotY = pressingRight ? 1 : pressingLeft ? -1 : 0;

    // Get current rotation. We temporarily override this rotation so that movement is parallel to
    // the ground plane.
    float eulerX = transform.localEulerAngles.x;
    float eulerY = transform.localEulerAngles.y;

    // Apply movement. We skip this if there is no movement this frame.
    Vector3 positionBefore = transform.position;
    if (isMoving) {
      // Move the camera at a speed that is linearly dependent on the height of the camera above
      // the ground plane to make camera manual camera movement practicable. The movement speed
      // is clamped between 1% and 100% of the configured MovementSpeed.
      float speed = Mathf.Clamp(transform.position.y, MovementSpeed * 0.01f, MovementSpeed);
      transform.localEulerAngles = new Vector3(0, eulerY, 0);
      transform.Translate(
          new Vector3(xInput, yInput, zInput) * speed * Time.deltaTime);

      // Enforce min/max height.
      Vector3 position = transform.position;
      position.y = Mathf.Clamp(position.y, MinHeight, MaxHeight);
      transform.position = position;
    }

    // Rotate, adding change in rotation to current rotation (recorded before overriden for
    // movement). We skip this if there is no rotation this frame.
    if (isRotating) {
      transform.localEulerAngles = new Vector3(
        Mathf.Clamp(eulerX + rotX * RotationSpeed * Time.deltaTime, MinXRotation, MaxXRotation),
        eulerY + rotY * RotationSpeed * Time.deltaTime,
        0);
    } else if (isMoving) {
      // If not rotating but did move, reset rotation to original, pre-overriden values.
      transform.localEulerAngles = new Vector3(eulerX, eulerY, 0f);
    }

    // Call any defined Actions to inform other classes of change in Camera's movement/rotation
    // this frame.
    if (isMoving) {
      // Pass in the amount moved this frame (current position minus position last frame).
      OnMove.Invoke(transform.position - positionBefore);
    }
    if (isRotating) {
      OnRotate.Invoke();
    }
    OnTransform.Invoke();
  }

  /// <summary>
  /// Event triggered when the <see cref="Camera"/> moves, passing back the amount moved.
  /// </summary>
  [Serializable]
  public class MoveEvent : UnityEvent<Vector3> { }
}
