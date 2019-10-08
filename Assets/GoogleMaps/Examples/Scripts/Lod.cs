using Google.Maps;
using Google.Maps.Coord;
using Google.Maps.Feature.Style;
using UnityEngine;

/// <summary>
/// Example demonstrating how to setup <see cref="LODGroup"/>s with multiple
/// <see cref="MapsService"/>s loading different <see cref="MapsService.ZoomLevel"/>s.
/// </summary>
/// <remarks>
/// By default loads Manhattan. If a new latitude/longitude is set in Inspector (before
/// pressing start), will load new location instead.
/// <para>
/// Also uses <see cref="ErrorHandling"/> component to display any errors encountered by the
/// <see cref="MapsService"/> component when loading geometry.
/// </para></remarks>
[RequireComponent(typeof(LodController), typeof(ErrorHandling))]
public sealed class Lod : MonoBehaviour {
  [Tooltip("LatLng to load (must be set before hitting play).")]
  public LatLng LatLng = new LatLng(40.7571312, -73.9736319);

  [Tooltip("Zoom level to load (MapsService's default zoom level is 17).")]
  public int ZoomLevel = 17;

  [Tooltip("Lower detail zoom level to load (MapsService's default zoom level is 17).")]
  public int ZoomLevelLower = 16;

  [Tooltip("Distance around player to load in meters (MapsService's default is 500 meters).")]
  [Range(0f, 2000f)]
  public float LoadDistance = 2000f;

  [Tooltip("Material to apply to buildings (must be configured to work with Level of Detail "
      + "cross-blending - see given example material, which is commented to show changes needed "
      + "for Level of Detail cross-fading.")]
  public Material LodMaterial;

  [Tooltip("Script for controlling camera zooming. Will wait until all geometry is loaded before "
      + "starting animated zooming in and out, said animation stopping as soon as any input is "
      + "received from the user.")]
  public ZoomController ZoomController;

  /// <summary>
  /// Create two <see cref="MapsService"/>s to load geometry, and connect them to
  /// required <see cref="LodController"/>.
  /// </summary>
  private void Start () {
    // Verify all required parameters are defined and correctly setup, skipping any further setup if
    // any parameter is missing or invalid.
    if (!VerifyParameters()) {
      return;
    }

    // Set this GameObject to be inactive. This is so that when we add MapsService components to
    // this GameObject, their Awake functions are not immediately called, giving us a chance to set
    // their parameters (like their ZoomLevel).
    gameObject.SetActive(false);

    // Add required Maps Service components to this GameObject. Two Maps Services are added, one to
    // load regular geometry, the other to load lower Level of Detail geometry for the same area.
    MapsService regularLodMapsService = gameObject.AddComponent<MapsService>();
    MapsService lowerLodMapsService = gameObject.AddComponent<MapsService>();

    // Set Api Key so MapsService components can download tiles.
    string apiKey = GetComponent<MapsService>().ApiKey;
    regularLodMapsService.ApiKey = apiKey;
    lowerLodMapsService.ApiKey = apiKey;

    // Set Zoom Levels on MapsService components. This is the reason why two MapsService components
    // are created - so we can load two different zoom levels of geometry.
    regularLodMapsService.ZoomLevel = ZoomLevel;
    lowerLodMapsService.ZoomLevel = ZoomLevelLower;

    // Re-active this GameObject, which will allow Awake to be called on added MapsService
    // components.
    gameObject.SetActive(true);

    // Set real-world location to load.
    regularLodMapsService.InitFloatingOrigin(LatLng);
    lowerLodMapsService.InitFloatingOrigin(LatLng);

    // Set custom load distance (given as a parameter), which should be larger than the default 500m
    // in order to better demonstrate Level of Detail effect over a large area.
    Bounds loadBounds = new Bounds(Vector3.zero, new Vector3(LoadDistance, 0, LoadDistance));

    // Create building styles that define a material for buildings. The specific material used
    // must be Level of Detail enabled (see example material and its shader for details).
    ExtrudedStructureStyle extrudedStructureStyle = new ExtrudedStructureStyle.Builder {
      WallMaterial = LodMaterial,
      RoofMaterial = LodMaterial
    }.Build();

    ModeledStructureStyle modeledStructureStyle = new ModeledStructureStyle.Builder {
      BuildingMaterial = LodMaterial
    }.Build();

    // Get default Game Object Options and replace building styles.
    GameObjectOptions renderingStyles = ExampleDefaults.DefaultGameObjectOptions;
    renderingStyles.ExtrudedStructureStyle = extrudedStructureStyle;
    renderingStyles.ModeledStructureStyle = modeledStructureStyle;

    // Get required Lod Controller for sorting created geometry into Lod Groups.
    LodController lodController = GetComponent<LodController>();

    // Sign up to event called after each new building is loaded, so we can place all buildings in
    // Level of Detail groups.
    regularLodMapsService.Events.ExtrudedStructureEvents.DidCreate.AddListener(args =>
        lodController.AddToLodGroup(args.MapFeature.Metadata.PlaceId, args.GameObject, 0));
    regularLodMapsService.Events.ModeledStructureEvents.DidCreate.AddListener(args =>
        lodController.AddToLodGroup(args.MapFeature.Metadata.PlaceId, args.GameObject, 0));
    lowerLodMapsService.Events.ExtrudedStructureEvents.DidCreate.AddListener(args =>
        lodController.AddToLodGroup(args.MapFeature.Metadata.PlaceId, args.GameObject, 1));
    lowerLodMapsService.Events.ModeledStructureEvents.DidCreate.AddListener(args =>
        lodController.AddToLodGroup(args.MapFeature.Metadata.PlaceId, args.GameObject, 1));

    // Add a listener that starts the lowerLodMapsService loading when the regularLodMapsService
    // has finished loading the map. This is to make sure all the starting, regular Level of Detail
    // geometry is loaded first, with extra, lower Level of Detail geometry added after the regular
    // geometry has been displayed on screen.
    regularLodMapsService.Events.MapEvents.Loaded.AddListener(args =>
        lowerLodMapsService.LoadMap(loadBounds, ExampleDefaults.DefaultGameObjectOptions));

    // Add a listener that starts the camera zooming in and out when the lowerLodMapsService has
    // finished loading. This is to make sure all Level of Detail Groups are setup before the camera
    // starts zooming out to show Level of Detail effects.
    lowerLodMapsService.Events.MapEvents.Loaded.AddListener(args =>
        ZoomController.StartAnimating());

    // Load first Maps Service (the second Maps Service will start loading when the first is
    // finished).
    regularLodMapsService.LoadMap(loadBounds, renderingStyles);
  }

  /// <summary>
  /// Verify that all required parameters have been correctly defined, returning false if not.
  /// </summary>
  private bool VerifyParameters() {
    // Verify that a Level of Detail enabled Material has been given for applying to buildings.
    if (LodMaterial == null) {
      Debug.LogError(ExampleErrors.MissingParameter(this, LodMaterial, "Lod Material"));
      return false;

    }

    // Verify that a valid load distance was given, i.e. that given distance was not negative nor
    // zero. Comparison is made to float.Epsilon instead of zero to account for float rounding
    // errors.
    if (LoadDistance < float.Epsilon) {
      Debug.LogError(ExampleErrors.NotGreaterThanZero(this, LoadDistance, "Load Distance"));
      return false;
    }

    // Verify Zoom Controller was defined.
    if (ZoomController == null) {
      Debug.LogError(ExampleErrors.MissingParameter(this, ZoomController, "Zoom Controller"));
      return false;
    }

    // If have reached this point then have verified that all required parts are present and
    // properly setup.
    return true;
  }
}
