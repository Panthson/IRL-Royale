using Google.Maps;
using Google.Maps.Coord;
using UnityEngine;

/// <summary>
/// A basic settings component, used to initialize MapsServices during benchmarking scenarios.
/// </summary>
/// <remarks>
/// By default, <see cref="Latitude"/> and <see cref="Longitude"/> is set to downtown Tokyo. This
/// area is chosen due to its relatively high geometry density compared to the rest of the world,
/// and thus allows Musk to be benchmarked on the edge of its performance window.
/// </remarks>
[RequireComponent(typeof(MapsService))]
public sealed class BenchmarkMapSettings : MonoBehaviour {
  [Tooltip("LatLng to load (must be set before hitting play).")]
  public LatLng LatLng = new LatLng(35.711503, 139.786968);

  /// <summary>
  /// Use <see cref="MapsService"/> to load geometry.
  /// </summary>
  private void Start() {
    // Get required Maps Service component on this GameObject.
    MapsService mapsService = GetComponent<MapsService>();

    // Set real-world location to load.
    mapsService.InitFloatingOrigin(LatLng);
  }
}
