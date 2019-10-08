using System.Collections;
using Google.Maps;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps.Feature.Style;
using UnityEngine;

/// <summary>
/// Benchmarking scenario that counts the total number of pixels drawn, as a percentage of the
/// maximum number of pixels that would have been drawn in ideal conditions (infinitely fast
/// network, no object creation lag, etc.).
/// </summary>
/// <remarks>
/// It does this in two passes, moving the camera over the same part of the map:
///   - In the first pass (cold run), load from the network while the camera is moving.
///   - In the second pass (warm run), wait after every frame until the map has finished loading,
///     before moving the camera again.
///
/// It counts the pixels in each pass, and returns the ratio (as a percentage).
/// </remarks>
public class PixelCountingScenario : BenchmarkingScenario {
  [Tooltip("LatLng to load (must be set before hitting play).")]
  public LatLng LatLng = new LatLng(35.711503, 139.786968);

  [Tooltip("Distance to move the camera per frame, in world units.")]
  public Vector3 CameraSpeed = new Vector3(5, 0, 2);

  [Tooltip("Number of frames per pass.")]
  public int FramesPerPass = 1000;

  /// <summary>Maps service.</summary>
  private MapsService MapsService;

  /// <summary>Game object options to load with.</summary>
  private GameObjectOptions GameObjectOptions;

  /// <summary>
  /// Camera position when <see cref="Start"/> is called. Used to reset the camera for the second
  /// pass.
  /// </summary>
  private Vector3 CameraStartPosition;

  /// <summary>
  /// Camera rotation when <see cref="Start"/> is called. Used to reset the camera for the second
  /// pass.
  /// </summary>
  private Quaternion CameraStartRotation;

  /// <summary>Number of pixels counted in the first run.</summary>
  private long ColdRunPixels;

  /// <summary>Number of pixels counted in the second run.</summary>
  private long WarmRunPixels;

  /// <summary>Number of frames elapsed so far.</summary>
  private int FrameCount;

  /// <summary>Texture used to read pixels from the screen.</summary>
  private Texture2D ScreenshotTexture;

  /// <summary>
  /// Whether the most recent call to <see cref="MapLoadRegion.Load"/> has finished.
  /// </summary>
  private bool MapLoaded;

  /// <summary>
  /// Whether the scenario is finished.
  /// </summary>
  private bool Done;

  /// <summary>Called when the map has finished loading.</summary>
  public void OnMapLoaded(MapLoadedArgs args) {
    MapLoaded = true;
  }

  /// <inheritdoc />
  public override bool IsDone() {
    return FrameCount >= FramesPerPass * 2;
  }

  /// <inheritdoc />
  public override string GetResults() {
    return string.Format("Loaded pixels: {0:P2}", ((double) ColdRunPixels) / WarmRunPixels);
  }

  /// <summary>Starts the scenario.</summary>
  protected void Start() {
    MapsService = GetComponent<MapsService>();
    MapsService.InitFloatingOrigin(LatLng);

    // We count pixels where the blue channel is at least 50%. To make this work, everything is
    // textured with an unlit color with 100% blue. (Antialiasing will average out.)
    var color = new Material(Shader.Find("Unlit/Color")) {
      color = new Color(0.2f, 0.5f, 1.0f)
    };

    var extrudedStructureStyle = new ExtrudedStructureStyle.Builder {
      WallMaterial = color,
      RoofMaterial = color
    }.Build();
    var modeledStructureStyle = new ModeledStructureStyle.Builder {
      BuildingMaterial = color
    }.Build();
    var areaWaterStyle = new AreaWaterStyle.Builder {
      FillMaterial = color
    }.Build();
    var lineWaterStyle = new LineWaterStyle.Builder {
      Material = color
    }.Build();
    var segmentStyle = new SegmentStyle.Builder {
      Material = color
    }.Build();
    var regionStyle = new RegionStyle.Builder {
      FillMaterial = color
    }.Build();

    GameObjectOptions = new GameObjectOptions {
      ExtrudedStructureStyle = extrudedStructureStyle,
      ModeledStructureStyle = modeledStructureStyle,
      AreaWaterStyle = areaWaterStyle,
      LineWaterStyle = lineWaterStyle,
      SegmentStyle = segmentStyle,
      RegionStyle = regionStyle,
    };

    // Remember where the camera is so that we can reset it for the second pass.
    CameraStartPosition = Camera.main.transform.position;
    CameraStartRotation = Camera.main.transform.rotation;
  }

  /// <summary>Called every frame.</summary>
  private void Update() {
    if (FrameCount >= FramesPerPass * 2) {
      // Both passes have finished.
      return;
    }

    // During the second pass, wait for the map to load before updating statistics or moving the
    // camera, so that the baseline statistics include every pixel that could be visible.
    if (FrameCount >= FramesPerPass && !MapLoaded) {
      return;
    }

    // Move the camera a constant amount per frame.
    Camera.main.transform.position += CameraSpeed;

    // Load the map.
    MapLoaded = false;
    MapsService.MakeMapLoadRegion().AddViewport(Camera.main).Load(GameObjectOptions);

    // Count pixels on the screen and update statistics.
    StartCoroutine(CaptureScreenshot());
  }

  /// <summary>Grabs pixels from the screen, and updates statistics for the current run.</summary>
  public IEnumerator CaptureScreenshot() {
    // Wait for rendering.
    yield return new WaitForEndOfFrame();

    // During the second pass, don't update statistics until the map has loaded.
    if (FrameCount >= FramesPerPass) {
      while (!MapLoaded) {
        yield return null;
      }
    }

    // Dump the screen to a texture.
    int width = Screen.width;
    int height = Screen.height;
    if (ScreenshotTexture != null) {
      if (ScreenshotTexture.width != width || ScreenshotTexture.height != height) {
        Destroy(ScreenshotTexture);
        ScreenshotTexture = null;
      }
    }
    if (ScreenshotTexture == null) {
      ScreenshotTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    }
    ScreenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    ScreenshotTexture.Apply();

    // Count the blue pixels.
    int setPixels = 0;
    Color32[] pixels = ScreenshotTexture.GetPixels32();
    foreach (Color32 c in pixels) {
      if (c.b > 127) {
        setPixels++;
      }
    }

    // Update statistics for the current run.
    if (FrameCount < FramesPerPass) {
      ColdRunPixels += setPixels;
    } else {
      WarmRunPixels += setPixels;
    }

    FrameCount++;
    // If the cold run has finished, reset the camera.
    if (FrameCount == FramesPerPass) {
      Camera.main.transform.position = CameraStartPosition;
      Camera.main.transform.rotation = CameraStartRotation;
    }
  }
}
