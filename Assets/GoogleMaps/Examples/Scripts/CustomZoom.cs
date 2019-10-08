using Google.Maps;
using Google.Maps.Coord;
using UnityEngine;

/// <summary>
/// Example that demonstrates the custom zoom,
/// using <see cref="MapLoadRegion.Load(GameObjectOptions, int)"/>
/// </summary>
/// <remarks>
/// Uses <see cref="ErrorHandling"/> component to display any errors encountered by the
/// <see cref="MapsService"/> component when loading geometry.
/// </remarks>
[RequireComponent(typeof(MapsService), typeof(ErrorHandling))]
public sealed class CustomZoom : MonoBehaviour {
  [Tooltip("LatLng to load (must be set before hitting play).")]
  public LatLng LatLng = new LatLng(-33.86882, 151.209296);

  /// <summary>
  /// Use <see cref="MapsService"/> to load one zoom 16 tile and one zoom 17 tile.
  /// </summary>
  private void Start () {
    // Get required Maps Service component on this GameObject.
    MapsService mapsService = GetComponent<MapsService>();

    // Set real-world location to load.
    mapsService.InitFloatingOrigin(LatLng);

    // Using the default material, load one zoom 16 tile.
    mapsService.MakeMapLoadRegion()
        .AddBounds(new Bounds(new Vector3(250, 0, 250), new Vector3(1, 0, 1)))
        .Load(ExampleDefaults.DefaultGameObjectOptions, 16);

    // Using the default material, load one zoom 17 tile.
    mapsService.MakeMapLoadRegion()
        .AddBounds(new Bounds(new Vector3(0, 0, 0), new Vector3(1, 0, 1)))
        .Load(ExampleDefaults.DefaultGameObjectOptions, 17);
  }
}
