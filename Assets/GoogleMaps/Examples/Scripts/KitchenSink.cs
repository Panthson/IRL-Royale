using Google.Maps;
using Google.Maps.Feature.Style;
using UnityEngine;

/// <summary>Example combining all the functionality of all previous examples.</summary>
[RequireComponent(typeof(DynamicMapsService), typeof(BuildingTexturer), typeof(EmissionController))]
[RequireComponent(typeof(RoadLabeller), typeof(BuildingLabeller), typeof(LodController))]
[RequireComponent(typeof(ErrorHandling))]
public sealed class KitchenSink : MonoBehaviour {
  [Tooltip("Script for controlling day-night cycle. Will wait until all geometry is loaded "
      + "before starting animated day-night cycle, said animation stopping as soon as any input "
      + "is received from the user. This parameter is not required if Day Night Cycle is unchecked "
      + "below.")]
  public SunAndMoonController SunAndMoonController;

  [Tooltip("Material to apply around bases of buildings and around roads.")]
  public Material BuildingAndRoadBorder;

  [Tooltip("Material to use for roads.")]
  public Material Roads;

  [Tooltip("Animate day and night cycling?")]
  public bool DayNightCycle = true;

  [Tooltip("Use Level of Detail to fade out buildings based on screen size?")]
  public bool LevelOfDetail = true;

  /// <summary>Has geometry been loaded at least once before.</summary>
  /// <remarks>
  /// This variable is used to detect the first time geometry is loaded, in which case this variable
  /// is set to true.
  /// </remarks>
  private bool HaveLoaded;

  /// <summary>
  /// Create a <see cref="MapsService"/> to load geometry.
  /// </summary>
  private void Awake() {
    // Verify that all required parameters have been correctly defined, printing an error if any
    // are missing and skipping setup.
    if (!VerifyParameters()) {
      return;
    }

    // Get required Building Texturer component on this GameObject.
    BuildingTexturer buildingTexturer = GetComponent<BuildingTexturer>();

    // Get required Emission Controller component, and give the Building Wall Materials to it so
    // that the building windows can be lit up at night time.
    GetComponent<EmissionController>().SetMaterials(buildingTexturer.WallMaterials);

    // Get required Dynamic Maps Service component on this GameObject.
    DynamicMapsService dynamicMapsService = GetComponent<DynamicMapsService>();

    // Create a roads style that defines a material for roads and for borders of roads. The specific
    // border material used is chosen to look just a little darker than the material of the ground
    // plane (helping the roads to visually blend into the surrounding ground).
    SegmentStyle roadsStyle = new SegmentStyle.Builder {
      Material = Roads,
      BorderMaterial = BuildingAndRoadBorder,
      Width = 7.0f,
      BorderWidth = 1.0f
    }.Build();

    // Get default style options.
    GameObjectOptions renderingStyles = ExampleDefaults.DefaultGameObjectOptions;

    // Replace default roads style with new, just created roads style.
    renderingStyles.SegmentStyle = roadsStyle;

    // Use this border-inclusive rendering style with loading geometry using dynamic maps service.
    dynamicMapsService.RenderingStyles = renderingStyles;

    // Sign up to event called just after any new road (segment) is loaded, so can add to stored
    // roads and can create/re-center road's name Label using required Road Labeller component. Note
    // that:
    // - DynamicMapsService.MapsService is auto-found on first access (so will not be null).
    // - This and all other events must be set now during Awake, so that when Dynamic Maps Service
    //   starts loading the map during Start, this event will be triggered for all Extruded
    //   Structures.
    RoadLabeller roadLabeller = GetComponent<RoadLabeller>();
    dynamicMapsService.MapsService.Events.SegmentEvents.DidCreate.AddListener(args
      => roadLabeller.NameRoad(args.GameObject, args.MapFeature));

    // Sign up to event called after each new extruded building is loaded, so can assign Materials
    // to this new building, and add an extruded base around the building to fake an Ambient
    // Occlusion contact shadow, as well as an extruded parapet around the roof.
    BuildingLabeller buildingLabeller = GetComponent<BuildingLabeller>();
    LodController lodController = GetComponent<LodController>();
    dynamicMapsService.MapsService.Events.ExtrudedStructureEvents.DidCreate.AddListener(args => {
      // Apply nine sliced wall and roof materials to this building.
      buildingTexturer.AssignNineSlicedMaterials(args.GameObject);

      // Add a border around base to building using Building Border Builder class, coloring it using
      // the given border Material.
      GameObject[] borders = Extruder.AddBuildingBorder(
          args.GameObject, args.MapFeature.Shape, BuildingAndRoadBorder);

      // Add a parapet to this building, making sure it shares the building's roof Material. This
      // should have just been added as the building's second SharedMaterial.
      Material roofMaterial = args.GameObject.GetComponent<MeshRenderer>().sharedMaterials[1];
      GameObject[] parapets = Extruder.AddRandomBuildingParapet(
          args.GameObject, args.MapFeature.Shape, roofMaterial);

      // Label this building with its name (if name found).
      buildingLabeller.NameExtrudedBuilding(args.GameObject, args.MapFeature);

      // Add modelled building, along with its extra geometry (borders and parapets) to an Lod group
      // so it can be faded out with distance. This is skipped if not using Lod groups at all.
      if (LevelOfDetail) {
        GameObject[] allGeo = new GameObject[borders.Length + parapets.Length + 1];
        allGeo[0] = args.GameObject;
        borders.CopyTo(allGeo, 1);
        parapets.CopyTo(allGeo, 1 + borders.Length);
        lodController.AddToLodGroup(args.MapFeature.Metadata.PlaceId, allGeo, 0);
      }
    });

    // Sign up to event called after each new modelled building is loaded, so can assign Materials,
    // so can name this building. Note that, at present, modelled buildings are not compatible with
    // Nine Sliced Textures or added extrusions.
    dynamicMapsService.MapsService.Events.ModeledStructureEvents.DidCreate.AddListener(args => {
      // Label this building with its name (if name found).
      buildingLabeller.NameModeledBuilding(args.GameObject, args.MapFeature);

      // Add modelled building to an LOD group so it can be faded out with distance. This is skipped
      // if we are not using Lod groups at all.
      if (LevelOfDetail) {
        lodController.AddToLodGroup(args.MapFeature.Metadata.PlaceId, args.GameObject, 0);
      }
    });

    // Sign up to event called after all buildings have been loaded, so we can start animating
    // day-night cycle.
    dynamicMapsService.MapsService.Events.MapEvents.Loaded.AddListener(args => {
      // If this is the first time all geometry is loaded, optionally start animating the Day/Night
      // cycle.
      if (!HaveLoaded) {
        HaveLoaded = true;
        if (DayNightCycle) {
          SunAndMoonController.StartAnimating();
        }
      }

      // Remove any Lod groups for buildings that have now been unloaded (skipped if we are not
      // using Lod groups at all).
      if (LevelOfDetail) {
        lodController.RemoveNull();
      }
    });
  }

  /// <summary>
  /// Verify that all required parameters have been correctly defined, returning false if not.
  /// </summary>
  private bool VerifyParameters() {
    // Verify that a Sun and Moon Controller has been defined.
    if (SunAndMoonController == null) {
      // Note: 'name' and 'GetType()' just give the name of the GameObject this script is on, and
      // the name of this script respectively.
      Debug.LogErrorFormat("No {0} defined for {1}.{2}, which requires a {0} to run!",
        typeof(SunAndMoonController), name, GetType());
      return false;
    }

    // Verify a Building Base Material has been given.
    if (BuildingAndRoadBorder == null) {
      Debug.LogErrorFormat("Null Building Base Material defined for {0}.{1}, which needs a "
          + "Material to apply around the bases of buildings.",
          name, GetType());
      return false;
    }

    // Verify a Roads Material has been given.
    if (Roads == null) {
      Debug.LogError(ExampleErrors.MissingParameter(this, Roads, "Roads", "to apply to roads"));
      return false;
    }

    // Verify a Sun and Moon Controller has been given (skipped if we are not animating Day/Night
    // cycle).
    if (DayNightCycle && SunAndMoonController == null) {
      Debug.LogError(ExampleErrors.MissingParameter(this, SunAndMoonController,
          "Sun And Moon Controller", "to animate Day/Night cycle"));
      return false;
    }

    // If have reached this point then have verified that all required parts are present and
    // properly setup.
    return true;
  }
}
