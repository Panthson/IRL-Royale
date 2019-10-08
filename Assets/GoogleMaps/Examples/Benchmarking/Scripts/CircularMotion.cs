using Google.Maps.Event;
using UnityEngine;

/// <summary>
/// Rotates an object around its origin at a constant speed. Stops after one rotation.
/// </summary>
public class CircularMotion : MonoBehaviour {
  /// <summary>
  /// Speed of the rotation in degrees per second.
  /// </summary>
  public float Speed = 10f;

  /// <summary>
  /// Delegate type for receiving complete rotation events.
  /// </summary>
  public delegate void CompleteRotationAction();

  /// <summary>
  /// Called when a full rotation has been completed.
  /// </summary>
  public static event CompleteRotationAction OnCompleteRotation;

  /// <summary>
  /// Time elapsed since the behavior was started.
  /// </summary>
  private float totalTime = 0f;

  /// <summary>
  /// Whether the rotation has stopped. Rotation is stopped after a full rotation.
  /// </summary>
  public bool isStopped = true;

  /// <summary>
  /// Whether the benchmarking test has finished.
  /// </summary>
  public bool isDone = false;

  /// <summary>
  /// Whether the rotation has at some point been started. Used to ignore subsequent map load
  /// events.
  /// </summary>
  private bool hasBeenStarted = false;

  /// <summary>
  /// Called when the map has finished loading tiles.
  /// </summary>
  public void OnMapLoaded(MapLoadedArgs args) {
    if (hasBeenStarted) {
      // If we'd previously started moving, we don't need to change anything.
      return;
    }
    hasBeenStarted = true;
    isStopped = false;
  }

  void Update() {
    if (isStopped) {
      return;
    }
    totalTime += Time.deltaTime;
    transform.Rotate(Vector3.up * Time.deltaTime * Speed, Space.World);
    if (totalTime * Speed > 360) {
      isStopped = true;
      isDone = true;
      if (OnCompleteRotation != null) {
        OnCompleteRotation();
      }
    }
  }

  /// <summary>
  /// Reset the rotation of the circular motion.
  /// </summary>
  /// <remarks>
  /// Usually invoked when an object needs to be rotated again.
  /// </remarks>
  public void Reset() {
    totalTime = 0f;
    isStopped = false;
    isDone = false;
    hasBeenStarted = false;
    OnCompleteRotation = null;
  }
}
