using System;
using System.Diagnostics;
using Google.Maps;
using Google.Maps.Event;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Monitors the SustainedFpsScenario benchmarking scene.
/// </summary>
public class LoadTimeScenario : BenchmarkingScenario {
  /// <summary>
  /// The possible types of loading operations to be performed.
  /// </summary>
  private enum MapLoadPassType {
    ColdStart,
    CacheStart,
  }

  #region Editor Fields

  /// <summary>
  /// Text object in which to display the map load time.
  /// </summary>
  public Text LoadTimeText;

  /// <summary>
  /// The <see cref="MapsService"/> responsible for loading the map.
  /// </summary>
  public MapsService MapsService;

  #endregion

  #region Private Fields

  /// <summary>
  /// The region of the map to load during the benchmark. It is set to cover the view of the main
  /// camera.
  /// </summary>
  private MapLoadRegion MapLoadRegion;

  /// <summary>
  /// Measures the amount of time taken to perform a series of map loading operations.
  /// </summary>
  private Stopwatch MapLoadingStopwatch;

  /// <summary>
  /// The time taken to load the map area on cold start.
  /// </summary>
  private TimeSpan ColdStartLoadTime;

  /// <summary>
  /// The time taken to load the map on cache start.
  /// </summary>
  private TimeSpan CacheStartLoadTime;

  /// <summary>
  /// Whether the benchmarking scenario has started. This is used for running initialization logic
  /// that cannot occur in Start() or Awake().
  /// </summary>
  private bool ScenarioStarted = false;

  /// <summary>
  /// The current loading operation being performed.
  /// </summary>
  private MapLoadPassType MapLoadPass = MapLoadPassType.ColdStart;

  /// <summary>
  /// Whether this scenario has finished running.
  /// </summary>
  private bool ScenarioDone = false;

  #endregion

  /// <inheritdoc />
  public override bool IsDone() {
    return ScenarioDone;
  }

  /// <inheritdoc />
  public override string GetResults() {
    return GenerateReport(ColdStartLoadTime.ToString(), CacheStartLoadTime.ToString());
  }

  /// <summary>
  /// Generate a standardised report of the results of the benchmarking scenario
  /// </summary>
  /// <param name="coldStartTime">The data to display for the cold start pass.</param>
  /// <param name="cacheStartTime">The data to display for the cache start pass.</param>
  /// <returns>A formatted report of the benchmarking scenario.</returns>
  private string GenerateReport(string coldStartTime, string cacheStartTime) {
    return String.Format(
        "Cold Start Load Time (Including Network Time): {0}\nCache Start Load Time: {1}",
        coldStartTime, cacheStartTime);
  }

  /// <summary>
  /// Start the scenario.
  /// </summary>
  private void Start() {
    LoadTimeText.text = GenerateReport("In Progress", "Not Run");
    MapLoadingStopwatch = new Stopwatch();
  }

  /// <summary>
  /// Runs the scenario.
  /// </summary>
  void Update() {
    // This is performed here to ensure the camera has been correctly initialized. It was found that
    // computing the MapLoadRegion in Start() or Awake() would result in incorrect region sizing,
    // and the resulting loaded area would not completely fill the camera view.
    if (!ScenarioStarted) {
      MapLoadRegion = MapsService.MakeMapLoadRegion().AddViewport(Camera.main);
      ScenarioStarted = true;
      MapLoadingStopwatch.Start();
      MapLoadRegion.Load(ExampleDefaults.DefaultGameObjectOptions);
    }
  }

  /// <summary>
  /// Called when the map has finished loading tiles.
  /// </summary>
  public void OnMapLoaded(MapLoadedArgs args) {
    MapLoadingStopwatch.Stop();

    // Clean up current pass and prepare for the next one.
    switch (MapLoadPass) {
      case MapLoadPassType.ColdStart: {
        ColdStartLoadTime = MapLoadingStopwatch.Elapsed;

        LoadTimeText.text = GenerateReport(ColdStartLoadTime.ToString(), "In Progress");

        MapLoadingStopwatch.Reset();
        MapLoadPass = MapLoadPassType.CacheStart;

        // Delete created GameObjects (but don't clear cache).
        DestroyMap();

        MapLoadingStopwatch.Start();
        MapLoadRegion.Load(ExampleDefaults.DefaultGameObjectOptions);
        break;
      }
      case MapLoadPassType.CacheStart: {
        CacheStartLoadTime = MapLoadingStopwatch.Elapsed;

        LoadTimeText.text =
            GenerateReport(ColdStartLoadTime.ToString(), CacheStartLoadTime.ToString());

        MapLoadingStopwatch.Reset();
        ScenarioDone = true;
        break;
      }
    }
  }

  /// <summary>
  /// Destroys the GameObjects created by Musk.
  /// </summary>
  private void DestroyMap() {
    foreach (Transform child in MapsService.transform) {
      Destroy(child.gameObject);
    }
    MapsService.GameObjectManager.DidDestroyAll();
  }
}
