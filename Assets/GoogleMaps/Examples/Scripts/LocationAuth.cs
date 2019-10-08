using System.Collections;
using Google.Maps;
using Google.Maps.Location;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Example demonstrating the use of the Location Authenticity API to detect location spoofing.
/// </summary>
/// <remarks>
/// This example sends location data directly to the Location Authenticity API from the app. If the
/// app is compromised, it could change the result. We recommend instead sending the location data
/// to a game server and forwarding it to the Location Authenticity API from there.
/// </remarks>
[RequireComponent(typeof(MapsService), typeof(LocationAuthenticator))]
public sealed class LocationAuthExample : MonoBehaviour {
  [Tooltip("Text element to display results.")]
  public Text Text;

  [Tooltip("API key with Location Authenticity API enabled. " +
           "If empty, uses the key from MapsService.")]
  public string ApiKey;

  /// <summary>Setup the script.</summary>
  private void Start() {
    if (string.IsNullOrEmpty(ApiKey)) {
      ApiKey = GetComponent<MapsService>().ApiKey;
    }
  }

  /// <summary>
  /// Handles the event indicating that location signals are ready. This happens on average 1 minute
  /// after <see cref="LocationAuthenticator"/> is enabled.
  /// </summary>
  public void OnLocationSignalReady(LocationSignalReadyEvent.Args args) {
    Text.text += "Signal data: " + args.Data.Length + " bytes\n";
    StartCoroutine(EvaluateLocationAuthenticity(args.Data));
  }

  /// <summary>
  /// Sends location signal data via POST request to the Location Authenticity API.
  /// </summary>
  /// <remarks>
  /// This example sends location data directly to the Location Authenticity API from the app. If
  /// the app is compromised, it could change the result. We recommend instead sending the location
  /// data to a game server and forwarding it to the Location Authenticity API from there.
  /// </remarks>
  private IEnumerator EvaluateLocationAuthenticity(byte[] locationSignalData) {
    Text.text += "Sending to Location Authenticity API...\n";

    // In a deployed game, this request should be sent from the game server, not the app.
    var request = new UnityWebRequest(
        "https://locationauthenticity.googleapis.com/v1:evaluate?alt=json");
    request.method = UnityWebRequest.kHttpVerbPOST;
    request.uploadHandler = new UploadHandlerRaw(locationSignalData);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/x-protobuf");
    request.SetRequestHeader("X-Goog-Api-Key", ApiKey);

    yield return request.SendWebRequest();

    if (request.isHttpError || request.isNetworkError) {
      Text.text += "Call failed: " + request.error;
      yield break;
    }

    // Show the JSON response.
    Text.text += "Response: " + request.downloadHandler.text;
  }
}
