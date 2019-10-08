using UnityEngine;
using System;
using Google.Maps.Event;

/// <summary>
/// Simple class for calculating average framerate since the behavior was started.
/// </summary>
public class FramerateCalculator : MonoBehaviour {
  #region Constants

  /// <summary>
  /// The percentiles of FPS that we're interested in measuring. We'll capture a metric like "99%
  /// of the time, we're above this FPS number". This is useful to catch intermittent jank at
  /// differing levels of severity.
  /// </summary>
  public readonly float[] FpsPercentiles = { 0.5f, 0.95f, 0.99f };

  #endregion

  #region Parameters

  [Header("Read Only")]
  [SerializeField, Tooltip("Average FPS since measurement started.")]
  public int Fps;

  #endregion

  #region Variables

  /// <summary>
  /// Total frames counted.
  /// </summary>
  private int FrameCount = 0;

  /// <summary>
  /// Total time elapsed since the behavior was started.
  /// </summary>
  private float TotalTime;

  /// <summary>
  /// Whether the framerate has stopped updating.
  /// </summary>
  private bool IsStopped = true;

  /// <summary>
  /// Whether we've started calculating the FPS at some point. Used to ignore subsequent map load
  /// events.
  /// </summary>
  private bool HasBeenStarted = false;

  /// <summary>
  /// A histogram of framerates from 0 FPS to 99 FPS.
  /// </summary>
  /// <remarks>
  /// A histogram is used so that we can record cumulative framerates over time without performing
  /// memory allocations, as any memory allocations will throw off profiling values.
  /// </remarks>
  private readonly int[] FrameRates = new int[100];

  /// <summary>
  /// A collection of strings for reporting 0 - 99 FPS to the benchmark scenario's UI.
  /// </summary>
  /// <remarks>
  /// These are pre-defined so that they can be used at runtime without performing extra GC
  /// allocations or frees. This is to reduce the interference the benchmarker has the potential to
  /// create with the performance measurements of the Maps Unity SDK.
  /// </remarks>
  private readonly static string[] FrameRateStrings = {
    "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
    "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
    "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
    "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
    "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
    "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
    "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
    "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
    "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
    "90", "91", "92", "93", "94", "95", "96", "97", "98", "99"
  };

  #endregion

  /// <summary>
  /// Stop the framerate from updating.
  /// </summary>
  /// <remarks>
  /// After calling this, metrics will stop being updated/collected. Consequently, from this point
  /// onward, additional memory allocations or garbage collection operations are no longer of
  /// concern.
  /// </remarks>
  public void Stop() {
    IsStopped = true;
  }


  /// <summary>
  /// Start framerate calculation.
  /// </summary>
  private void Awake() {
    // Stop the framerate from updating after the camera completes a full rotation.
    CircularMotion.OnCompleteRotation += Stop;
  }

  /// <summary>
  /// Called when the map has finished loading tiles.
  /// </summary>
  public void OnMapLoaded(MapLoadedArgs args) {
    if (HasBeenStarted) {
      return;
    }
    HasBeenStarted = true;
    IsStopped = false;
  }

  /// <summary>
  /// Calculates the framerate the scenario runs at or above for a specified percentage of the time.
  /// </summary>
  /// <remarks>
  /// For example: <code>GetFpsPercentile(0.99);</code> would return the framerate the scenario runs
  /// at or above for 99% of the time.
  /// </remarks>
  /// <remarks>
  /// If called before any framerates have been captured, it will report 0 FPS.
  /// </remarks>
  /// <param name="percentile">
  /// The nth percentile framerate to return. percentile must be in the range [0, 1] inclusive.
  /// </param>
  /// <returns>The framerate the scenario runs </returns>
  public int GetFpsPercentile(float percentile) {
    if (percentile < 0 || percentile > 1) {
      throw new ArgumentOutOfRangeException("percentile must be in the range [0, 1] inclusive.");
    }

    // Calculate the index of the recorded frame which represents the percentile'th framerate.
    // Rounding is performed to prevent incorrect results when a low number of frames have been
    // sampled. For example, if 1 Frame has been sampled, then the 99th percentile FPS should
    // include that sampled frame.
    // Note: MidpointRounding.AwayFromZero only rounds away from zero when a number is halfway
    // between the two closest integers. E.g: 3.5 would round to 4, but 3.4 would round to 3.
    int fpsPercentileFrameIndex =
        (int)Math.Round(FrameCount * percentile, MidpointRounding.AwayFromZero);

    // Start calculating percentile framerates at the first occurrence of a recorded frame. This is
    // required to prevent the incorrect results when sample counts are low. For example, if only 1
    // frame has been sampled, and the 40th percentile Framerate has been requested, then the 0th
    // recorded frame would be polled to produce the result. If calculating didn't start at the
    // first recorded frame, this would result in returning the highest recordable framerate,
    int i = FrameRates.Length - 1;
    while (FrameRates[i] == 0 && i > 0) {
      i--;
    }

    // Perform a linear probe until the percentile'th framerate has been found.
    for (; i > 0; i--) {
      fpsPercentileFrameIndex -= FrameRates[i];
      if (fpsPercentileFrameIndex <= 0) {
        break;
      }
    }

    return i;
  }

  /// <summary>
  /// Accumulate frames and divide by delta time for accurate fps calculation.
  /// </summary>
  void Update() {
    if (IsStopped) {
      return;
    }

    // Record framerate. Because Unity on mobile caps framerates at 60 FPS (https://goo.gl/YkYbB8),
    // it is acceptable to cap the reported framerate at 99 FPS.
    int frameRate = (int)(1f / Time.unscaledDeltaTime);
    if (frameRate >= FrameRates.Length) {
      frameRate = FrameRates.Length - 1;
    }
    FrameRates[frameRate]++;

    FrameCount++;
    TotalTime += Time.unscaledDeltaTime;

    Fps = (int)(FrameCount / TotalTime);

    // Also cap the running average FPS.
    if (Fps >= FrameRateStrings.Length) {
      Fps = FrameRateStrings.Length - 1;
    }
  }

  /// <summary>
  /// Return the current frame rate of type string.
  /// </summary>
  public string GetFrameRateString() {
    return FrameRateStrings[Fps];
  }

  /// <summary>
  /// Whether the frame rate has stopped updating.
  /// </summary>
  public bool Stopped() {
    return IsStopped;
  }

  /// <summary>
  /// Reset the calculation of frame rate.
  /// </summary>
  /// <remarks>
  /// Usually invoked when the frame rate needs to be calculated again.
  /// </remarks>
  public void Reset() {
    FrameCount = 0;
    TotalTime = 0f;
    IsStopped = true;
    HasBeenStarted = false;
    CircularMotion.OnCompleteRotation += Stop;
  }
}
