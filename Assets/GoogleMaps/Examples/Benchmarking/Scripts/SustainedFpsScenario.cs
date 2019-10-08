using System;
using System.Diagnostics;
using Google.Maps.Event;
using UnityEngine.UI;

/// <summary>
/// Monitors the SustainedFpsScenario benchmarking scene.
/// </summary>
public class SustainedFpsScenario : BenchmarkingScenario {
  /// <summary>
  /// The <see cref="CircularMotion"/> controller controls the camera's position during the test and
  /// tracks whether the benchmarking scenario has finished running.
  /// </summary>
  public CircularMotion CircularMotionController;

  /// <summary>
  /// The <see cref="FramerateCalculator"/> tracks the performance metrics this benchmarking
  /// scenario tests.
  /// </summary>
  public FramerateCalculator FramerateCalculator;

  /// <summary>
  /// The time to load the initial map area.
  /// </summary>
  public TimeSpan InitialLoadTime;

  /// <summary>
  /// Text object in which to display the FPS.
  /// </summary>
  public Text FpsText;

  /// <summary>
  /// Text object in which to display the map load time.
  /// </summary>
  public Text LoadTimeText;

  /// <summary>
  /// Measures the time taken from the Awake() method being called to the first map load.
  /// </summary>
  private Stopwatch MapLoadingStopwatch;

  /// <summary>
  /// Start the stopwatch to calculate the load time and subscribe to the event of
  /// <see cref="CircularMotion"/> in order to display percentile fps upon completion of full
  /// rotation.
  /// </summary>
  public override void Awake() {
    base.Awake();
    // Display the percentile fps after the camera completes a full rotation.
    CircularMotion.OnCompleteRotation += DisplayPercentileFPS;
    MapLoadingStopwatch = new Stopwatch();
    MapLoadingStopwatch.Start();
  }

  /// <summary>
  /// Display the percentile fps by appending the result to FpsText.
  /// </summary>
  private void DisplayPercentileFPS() {
    foreach (float percentile in FramerateCalculator.FpsPercentiles) {
      int fpsAtPercentile = FramerateCalculator.GetFpsPercentile(percentile);
      UnityEngine.Debug.LogFormat(
          "Sustained FPS at percentile {0}: {1}", percentile, fpsAtPercentile);
      FpsText.text += String.Format("\n{0} percentile FPS: {1}", percentile, fpsAtPercentile);
    }
  }

  /// <summary>
  /// Displays the current frame rate by updating FpsText.
  /// </summary>
  private void Update() {
    if (FramerateCalculator.Stopped()) {
      return;
    }
    FpsText.text = FramerateCalculator.GetFrameRateString();
  }

  /// <summary>
  /// Called when the map has finished loading tiles.
  /// </summary>
  public void OnMapLoaded(MapLoadedArgs args) {
    MapLoadingStopwatch.Stop();
    InitialLoadTime = MapLoadingStopwatch.Elapsed;
    LoadTimeText.text = "Initial load time: " + InitialLoadTime.ToString();
  }

  /// <inheritdoc />
  public override bool IsDone() {
    return CircularMotionController.isDone;
  }

  /// <inheritdoc />
  public override string GetResults() {
    // Average FPS.
    string results = String.Format("Average FPS (when panning map): {0}", FramerateCalculator.Fps);

    // Percentile FPS.
    foreach (float percentile in FramerateCalculator.FpsPercentiles) {
      float fpsAtPercentile = FramerateCalculator.GetFpsPercentile(percentile);
      results += String.Format(
          "\n{0} Percentile FPS (when panning map): {1}", percentile, fpsAtPercentile);
    }

    return results;
  }
}
