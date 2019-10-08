using System;
using System.IO;
using Google.Maps;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stores the API key to be used across all benchmarking scenarios. When the benchmarking
/// scenario starts, it will override the API key for the map service with this value, if it is
/// set. This is set by the benchmarking scenario runner before starting the benchmark scenes.
///
/// We do this so that the API key can be set in one place rather than needing to be set separately
/// in each benchmark scene.
/// </summary>
public static class ApiKeyStorage {
  /// <summary>
  /// The API key to use for benchmarking scenes. If set, this will override the API key that is
  /// set in other benchmarking scenes.
  /// </summary>
  public static string Key;
}

/// <summary>
/// An interface between benchmarking scenarios and the benchmark runner.
/// </summary>
/// <remarks>
/// Each benchmarking scenario scene should include a MonoBehaviour that implements this, and run
/// it as a component inside a GameObject called "ScenarioMonitor".
/// </remarks>
public abstract class BenchmarkingScenario : MonoBehaviour {
  /// <summary>
  /// Returns true when the scenario has finished running.
  /// </summary>
  public abstract bool IsDone();

  /// <summary>
  /// Returns the results found by running the scenario.
  /// </summary>
  public abstract string GetResults();

  /// <summary>
  /// A collection of MapsServices running in the benchmarking scenario. These are stored so their
  /// disk caches can be purged during a scenario teardown.
  /// </summary>
  private MapsService[] MapsServices;

  /// <summary>
  /// Performs teardown of the benchmarking scenario when the containing scene is unloaded. A
  /// correct teardown is required to maintain a sterile working environment for the next test to
  /// be run.
  /// </summary>
  /// <param name="scene">The Unity scene being unloaded.</param>
  protected virtual void Teardown(Scene scene) {
    // Purge the disk caches of all MapsService components in this scene.
    foreach (MapsService mapsService in MapsServices) {
      string cachePath = mapsService.CacheOptions.BasePath;
      if (String.IsNullOrEmpty(cachePath)) {
        cachePath = Application.temporaryCachePath;
      }

      Directory.Delete(cachePath, true);
    }

    SceneManager.sceneUnloaded -= Teardown;
  }

  public virtual void Awake() {
    MapsServices = Resources.FindObjectsOfTypeAll<MapsService>();

    // The project settings ensure that this script is run before MapsService, so setting the API
    // key is safe.
    foreach (MapsService service in MapsServices) {
      if (ApiKeyStorage.Key != null) {
        service.ApiKey = ApiKeyStorage.Key;
      }
    }

    SceneManager.sceneUnloaded += Teardown;
  }
}
