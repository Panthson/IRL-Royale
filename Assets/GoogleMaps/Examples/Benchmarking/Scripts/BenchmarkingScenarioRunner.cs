using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Responsible for running each benchmarking scenario consecutively, and displaying the results
/// at the end.
/// </summary>
public class BenchmarkingScenarioRunner : MonoBehaviour {
  /// <summary>
  /// The names of all the scenes that are to be run as benchmarking scenarios.
  /// </summary>
  public string[] ScenarioScenes;

  /// <summary>
  /// The camera to use when no benchmarking scenarios are being run.
  /// </summary>
  /// <remarks>
  /// We are required to know about this camera so we can disable it whenever we switch to a new
  /// scenario. If we weren't to disable it, Unity would not automatically switch to the camera
  /// the scene uses.
  /// </remarks>
  public Camera SceneCamera;

  /// <summary>
  /// The UI canvas responsible for displaying the benchmarking results.
  /// </summary>
  /// <remarks>
  /// Hidden when benchmark scenarios are running.
  /// </remarks>
  public GameObject Canvas;

  /// <summary>
  /// Displays the results of the benchmarking scenarios.
  /// </summary>
  public Text BenchmarkResultsText;

  /// <summary>
  /// API key to be used for benchmarking scenes.
  /// </summary>
  public string ApiKey;

  /// <summary>
  /// Stores the cumulative results of the benchmarking scenarios ran.
  /// </summary>
  private string BenchmarkingResults;

  /// <summary>
  /// The name of the GameObject that serves as the interface to the scenario runner in each
  /// benchmarking scene.
  /// </summary>
  private const string ScenarioInterfaceName = "ScenarioMonitor";

  /// <summary>
  /// Unity Start method.
  /// </summary>
  void Start() {
    // Purge default cache directory before running scenarios. Any scenarios that use custom cache
    // directories will need to be in control of their own purging.
    string cachePath = Application.temporaryCachePath;
    Directory.Delete(cachePath, true);
    ApiKeyStorage.Key = ApiKey;

    StartCoroutine(RunScenarios());
  }

  /// <summary>
  /// Runs all the benchmarking scenarios in series and displays the output at the end.
  /// </summary>
  private IEnumerator RunScenarios() {
    Canvas.SetActive(false);

    foreach (string scenarioScene in ScenarioScenes) {
      yield return SceneManager.LoadSceneAsync(scenarioScene, LoadSceneMode.Additive);

      // Disabling the camera is required so that the Unity can switch to the camera in the loaded
      // benchmarking scenario.
      SceneCamera.gameObject.SetActive(false);

      GameObject scenarioRunner = GameObject.Find(ScenarioInterfaceName);
      if (scenarioRunner == null) {
        throw new ArgumentException(
            String.Format("No {0} found for scene.", ScenarioInterfaceName));
      }

      BenchmarkingScenario scenario = scenarioRunner.GetComponent<BenchmarkingScenario>();
      if (scenario == null) {
        throw new ArgumentException(
            String.Format("No BenchmarkingScenario found in this {0}.", ScenarioInterfaceName));
      }

      // Run the benchmarking scenario.
      yield return new WaitUntil(scenario.IsDone);

      BenchmarkingResults += String.Format("{0}\n", scenario.GetResults());

      yield return SceneManager.UnloadSceneAsync(scenarioScene);

      SceneCamera.gameObject.SetActive(true);
    }

    Canvas.SetActive(true);
    BenchmarkResultsText.text =
        String.Format("Benchmarking Results:\n\n{0}", BenchmarkingResults);
  }
}
