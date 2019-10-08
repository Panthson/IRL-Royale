using System.Collections.Generic;
using Google.Maps;
using UnityEngine;

/// <summary>Class for setting up <see cref="LODGroup"/>s based on Place ID.</summary>
/// <remarks>
/// Used with multiple <see cref="MapsService"/>s loading different
/// <see cref="MapsService.ZoomLevel"/>s.
/// </remarks>
public sealed class LodController : MonoBehaviour {
  [SerializeField, Tooltip("Whether all LOD Groups are disabled (showing Level of Detail 0 only). "
      + "This value can be set before hitting Play in order to start all Lod Groups disabled. "
      + "During play changing this value has no affect, and instead the Enable and Disable "
      + "functions must be used.")]
  private bool Disabled;

  [Header("Read Only")]
  [Tooltip("Total number of created Lod Groups for unique real world elements.")]
  public int TotalGroups;

  /// <summary>
  /// All created geometry, sorted into Level of Detail groups based on Place Id.
  /// </summary>
  private readonly Dictionary<string, LodSet> Geometry = new Dictionary<string, LodSet>();

  /// <summary>
  /// Sort a given piece of geometry created by <see cref="MapsService"/>s into a
  /// <see cref="LODGroup"/> by its Place Id.
  /// </summary>
  /// <param name="placeId">
  /// Place Id of newly created geometry. This uniquely identifies this newly created geometry as
  /// being a part of a specific, real world element, such as a building or road. Thus Place ID can
  /// be used to group together all parts (and all Level of Detail versions) of the same real world
  /// element into a single <see cref="LODGroup"/>.
  /// </param>
  /// <param name="geometry"><see cref="GameObject"/> of this newly created geometry.</param>
  /// <param name="levelOfDetail">
  /// Level of detail to store this geometry under (0 for regular, 1 for lower Level of Detail,
  /// etc).
  /// </param>
  internal void AddToLodGroup(string placeId, GameObject geometry, int levelOfDetail) {
    // Make sure given geometry is not null.
    if (geometry == null) {
      Debug.LogErrorFormat("Null geometry given to {0}.{1}.AddToLodGroup.\nSkipping adding null "
          + "geometry.",
          name, GetType());
      return;
    }

    // See if we have already encountered any parts of this real world element before and, if so,
    // store this new part in the already created LOD Group for this real world element.
    if (Geometry.ContainsKey(placeId)) {
      Geometry[placeId].Add(geometry, levelOfDetail, Disabled);
      return;
    }

    // Add this new geometry to a new LOD Set for this new real world element.
    Geometry.Add(placeId, new LodSet(geometry, levelOfDetail, Disabled));
    TotalGroups++;
  }

  /// <summary>
  /// Sort a given piece of geometry created by <see cref="MapsService"/>s into a
  /// <see cref="LODGroup"/> by its Place Id.
  /// </summary>
  /// <param name="placeId">
  /// Place Id of newly created geometry. This uniquely identifies this newly created geometry as
  /// being a part of a specific, real world element, such as a building or road. Thus Place ID can
  /// be used to group together all parts (and all Level of Detail versions) of the same real world
  /// element into a single <see cref="LODGroup"/>.
  /// </param>
  /// <param name="geometry">
  /// Array of <see cref="GameObject"/>s for this newly created geometry.</param>
  /// <param name="levelOfDetail">
  /// Level of detail to store this geometry under (0 for regular, 1 for lower Level of Detail,
  /// etc).
  /// </param>
  internal void AddToLodGroup(string placeId, GameObject[] geometry, int levelOfDetail) {
    // Make sure given array of geometry is not empty and contains no null elements.
    if (geometry == null) {
      Debug.LogErrorFormat("Null geometry array given to {0}.{1}.AddToLodGroup.\nSkipping adding "
          + "null geometry array.",
          name, GetType());
      return;
    }
    if (geometry.Length == 0) {
      Debug.LogErrorFormat("Empty geometry array given to {0}.{1}.AddToLodGroup.\nSkipping adding "
          + "empty geometry array.",
          name, GetType());
      return;
    }
    for (int i = 0; i < geometry.Length; i++) {
      if (geometry[i] == null) {
        Debug.LogErrorFormat("Geometry array with null element given to {0}.{1}.AddToLodGroup.\n"
            + "Skipping adding geometry with empty element {2} of {3} to lod group.",
            name, GetType(), i + 1, geometry.Length);
        return;
      }
    }

    // See if we have already encountered any parts of this real world element before and, if so,
    // store this new part in the already created LOD Group for this real world element.
    if (Geometry.ContainsKey(placeId)) {
      Geometry[placeId].Add(geometry, levelOfDetail, Disabled);
      return;
    }

    // Add this new geometry to a new LOD Set for this new real world element.
    Geometry.Add(placeId, new LodSet(geometry, levelOfDetail, Disabled));
    TotalGroups++;
  }

  /// <summary>
  /// Remove all null elements from <see cref="LODGroup"/>s, and remove all <see cref="LODGroup"/>s
  /// with all null elements.
  /// </summary>
  internal void RemoveNull() {
    // Get the Place Id's of all null Lod sets so they can all be removed at. We preform the actual
    // removal in a second foreach loop, in order to prevent an InvalidOperationException caused by
    // trying to remove a Dictionary element during a foreach traversal of said Dictionary.
    LinkedList<string> nullLodSetIds = new LinkedList<string>();
    foreach (string placeId in Geometry.Keys) {
      if (Geometry[placeId].IsEmpty()) {
        nullLodSetIds.AddLast(placeId);
      }
    }

    // Remove all null Lod Sets.
    foreach (string placeIdToRemove in nullLodSetIds) {
      Geometry[placeIdToRemove].Clear();
      Geometry.Remove(placeIdToRemove);
      TotalGroups--;
    }
  }

  /// <summary>
  /// Allow all existing and future <see cref="LODGroup"/>s to display correct Level of Detail.
  /// </summary>
  /// <param name="enable">Optionally set to true to enable, false to disable.</param>
  internal void EnableAll(bool enable = true) {
    DisableAll(!enable);
  }

  /// <summary>
  /// Force all existing and future <see cref="LODGroup"/>s to display only Level of Detail 0.
  /// </summary>
  /// <param name="disable">Optionally set to true to disable, false to enable.</param>
  internal void DisableAll(bool disable = true) {
    // Skip if value already set.
    if (Disabled == disable) {
      return;
    }

    // Store chosen value, and apply disabling/enabling to all lod sets.
    Disabled = disable;
    foreach (LodSet lodSet in Geometry.Values) {
      lodSet.Disable(disable);
    }
  }

  /// <summary>
  /// Class for containing all parts of a real world element in a single, Place Id sorted LOD group.
  /// </summary>
  private sealed class LodSet {
    /// <summary>
    /// Proportion of screen size when should start cross-fading from regular to lower Level of
    /// Detail (e.g. 0.1f means that when the geometry is taking up 10% of the screen, Unity starts
    /// fading to lower LOD group).
    /// </summary>
    private const float LodSize = 0.1f;

    /// <summary>
    /// Unity component that manages LOD functionality for this group.
    /// </summary>
    private readonly LODGroup LodComponent;

    /// <summary>
    /// Create a new <see cref="LodSet"/> from a single piece of starting geometry, setting up
    /// <see cref="LODGroup"/> so future geometry can be added.
    /// </summary>
    /// <param name="initialGeometry">First part of this real world element.</param>
    /// <param name="initialLevelOfDetail">LOD of this first part.</param>
    /// <param name="isDisabled">
    /// Are all <see cref="LodSet"/>s currently disabled (force to display only LOD 0 geometry).
    /// </param>
    public LodSet(GameObject initialGeometry, int initialLevelOfDetail, bool isDisabled) {
      // Make a new GameObject with the same name and hierarchy position as the given initial
      // geometry part. This new GameObject will be used to hold the LOD Group script.
      var lodGroupTransform = new GameObject(initialGeometry.name).transform;
      lodGroupTransform.SetParent(initialGeometry.transform.parent);
      lodGroupTransform.localPosition = initialGeometry.transform.localPosition;
      lodGroupTransform.localRotation = initialGeometry.transform.localRotation;
      lodGroupTransform.localScale = initialGeometry.transform.localScale;

      // Add LOD Group script to manage showing different Levels of Detail.
      LodComponent = lodGroupTransform.gameObject.AddComponent<LODGroup>();
      LodComponent.animateCrossFading = true;
      LodComponent.fadeMode = LODFadeMode.SpeedTree;

      // Make starting initial geometry part as a child of the newly created LOD GameObject.
      initialGeometry.transform.SetParent(LodComponent.transform);
      initialGeometry.name = string.Format("LOD{0}.0", initialLevelOfDetail);

      // Setup Levels of Detail for this initial geometry part, and store part in LOD Group.
      MeshRenderer meshRenderer = initialGeometry.GetComponent<MeshRenderer>();
      LOD[] lods = new LOD[initialLevelOfDetail + 1];
      lods[initialLevelOfDetail]
          = new LOD(Mathf.Pow(LodSize, initialLevelOfDetail + 1), new Renderer[] { meshRenderer });

      // Fill all preceding Levels of Detail with empty groups, so that if this is a lower Level
      // of Detail geometry part, it will only fade in when this LOD should be shown.
      for (int i = 0; i < initialLevelOfDetail; i++) {
        lods[i] = CreateEmptyLod(i);
      }
      LodComponent.SetLODs(lods);
      LodComponent.RecalculateBounds();

      // If all LOD Sets are disabled, disable this set now.
      if (isDisabled) {
        Disable(true);
      }
    }

    /// <summary>
    /// Create a new <see cref="LodSet"/> from an array of starting geometry, setting up
    /// <see cref="LODGroup"/> so future geometry can be added.
    /// </summary>
    /// <param name="initialGeometry">First part of this real world element.</param>
    /// <param name="initialLevelOfDetail">LOD of this first part.</param>
    /// <param name="isDisabled">
    /// Are all <see cref="LodSet"/>s currently disabled (force to display only LOD 0 geometry).
    /// </param>
    public LodSet(GameObject[] initialGeometry, int initialLevelOfDetail, bool isDisabled) {
      // Make a new GameObject with the same name and hierarchy position as the given initial
      // geometry part. This new GameObject will be used to hold the LOD Group script.
      var lodGroupTransform = new GameObject(initialGeometry[0].name).transform;
      lodGroupTransform.SetParent(initialGeometry[0].transform.parent);
      lodGroupTransform.localPosition = initialGeometry[0].transform.localPosition;
      lodGroupTransform.localRotation = initialGeometry[0].transform.localRotation;
      lodGroupTransform.localScale = initialGeometry[0].transform.localScale;

      // Add LOD Group script to manage showing different Levels of Detail.
      LodComponent = lodGroupTransform.gameObject.AddComponent<LODGroup>();
      LodComponent.animateCrossFading = true;
      LodComponent.fadeMode = LODFadeMode.SpeedTree;

      // Make starting initial geometry part as a child of the newly created LOD
      // GameObject.
      initialGeometry[0].transform.SetParent(LodComponent.transform);
      initialGeometry[0].name = string.Format("LOD{0}.0", initialLevelOfDetail);

      // Setup Levels of Detail for these initial geometry parts, and store part in LOD
      // Group.
      MeshRenderer[] meshRenderers = new MeshRenderer[initialGeometry.Length];
      for (int i = 0; i < initialGeometry.Length; i++) {
        meshRenderers[i] = initialGeometry[i].GetComponent<MeshRenderer>();
      }
      LOD[] lods = new LOD[initialLevelOfDetail + 1];
      lods[initialLevelOfDetail]
          = new LOD(Mathf.Pow(LodSize, initialLevelOfDetail + 1), meshRenderers);

      // Fill all preceding Levels of Detail with empty groups, so that if this is a lower Level
      // of Detail geometry part, it will only fade in when this LOD should be shown.
      for (int i = 0; i < initialLevelOfDetail; i++) {
        lods[i] = CreateEmptyLod(i);
      }
      LodComponent.SetLODs(lods);
      LodComponent.RecalculateBounds();

      // If all LOD Sets are disabled, disable this set now.
      if (isDisabled) {
        Disable(true);
      }
    }

    /// <summary>
    /// Add a new geometry part to this <see cref="LodSet"/> and its <see cref="LODGroup"/>.
    /// </summary>
    /// <param name="newGeometry">New geometry to add.</param>
    /// <param name="lodLevel">
    /// LOD to add this geometry to (0 for regular geometry, 1 for lower LOD geometry, etc).
    /// </param>
    /// <param name="isDisabled">
    /// Are all <see cref="LodSet"/>s currently disabled (force to display only LOD 0 geometry).
    /// </param>
    public void Add(GameObject newGeometry, int lodLevel, bool isDisabled) {
      // Place new geometry under GameObject containing all previously stored parts of this real
      // world element.
      newGeometry.transform.SetParent(LodComponent.transform, true);

      // See if there is already a LOD created for this geometry.
      LOD[] lods = LodComponent.GetLODs();
      if (lodLevel >= lods.Length) {
        // If this is the first part of this real world element found of this specific Level of
        // Detail, name it as such.
        newGeometry.gameObject.name = string.Format("LOD{0}.0", lodLevel);

        // Insert new LOD into group of existing Levels of Detail.
        LOD newLod = CreateLod(lodLevel + 1, newGeometry.GetComponent<MeshRenderer>());
        LOD[] expandedLods = ExpandArray(lods, newLod, lodLevel + 1);
        LodComponent.SetLODs(expandedLods);
      } else {
        // If this is not the first geometry found of this LOD, name and store it as
        // another element in an existing LOD.
        newGeometry.gameObject.name = string.Format("LOD{0}.{1}",
            lodLevel, lods[lodLevel].renderers.Length);

        // Expand existing LOD to include this new geometry.
        Renderer[] expandedRenderers = ExpandArray(
            lods[lodLevel].renderers, newGeometry.GetComponent<MeshRenderer>());

        // Add expanded LOD into group of existing Levels of Detail.
        lods[lodLevel] = CreateLod(lodLevel, expandedRenderers);
        LodComponent.SetLODs(lods);
      }

      // Now that we have updated Levels of Detail for this real world element, recalculate the
      // bounds of all stored geometry. This is so the LOD Group component can properly
      // determine when to fade between different Levels of Detail based on how much screen space
      // this element occupies.
      LodComponent.RecalculateBounds();

      // If all LOD Sets are disabled, disable this set now.
      if (isDisabled) {
        Disable(true);
      }
    }

    /// <summary>
    /// Add a new geometry part to this <see cref="LodSet"/> and its <see cref="LODGroup"/>.
    /// </summary>
    /// <param name="newGeometry">Array of new geometry to add.</param>
    /// <param name="lodLevel">
    /// LOD to add this geometry to (0 for regular geometry, 1 for lower LOD geometry, etc).
    /// </param>
    /// <param name="isDisabled">
    /// Are all <see cref="LodSet"/>s currently disabled (force to display only LOD 0).
    /// </param>
    public void Add(GameObject[] newGeometry, int lodLevel, bool isDisabled) {
      // Place new geometry under GameObject containing all previously stored parts of this real
      // world element. While doing this, name all elements based on the current LOD and
      // any existing elements already in this group.
      LOD[] lods = LodComponent.GetLODs();
      bool lodIsNew = lodLevel >= lods.Length;
      int existingElements = lodIsNew ? 0 : lods[lodLevel].renderers.Length;
      MeshRenderer[] meshRenderers = new MeshRenderer[newGeometry.Length];
      for(int i = 0; i < newGeometry.Length; i++) {
        newGeometry[i].transform.SetParent(LodComponent.transform, true);
        newGeometry[i].name = string.Format("LOD{0}.{1}", lodLevel, i + existingElements);
        meshRenderers[i] = newGeometry[i].GetComponent<MeshRenderer>();
      }

      // If there is not already a LOD for this geometry, create one now.
      if (lodIsNew) {
        LOD newLod = CreateLod(lodLevel + 1, meshRenderers);
        LOD[] expandedLods = ExpandArray(lods, newLod, lodLevel + 1);
        LodComponent.SetLODs(expandedLods);
      } else {
        // Expand existing LOD to include this new geometry.
        Renderer[] expandedRenderers = ExpandArray(lods[lodLevel].renderers, meshRenderers);

        // Add expanded LOD into group of existing Levels of Detail.
        lods[lodLevel] = CreateLod(lodLevel, expandedRenderers);
        LodComponent.SetLODs(lods);
      }

      // Now that we have updated Levels of Detail for this real world element, recalculate the
      // bounds of all stored geometry. This is so the LOD Group component can properly determine
      // when to fade between different Levels of Detail based on how much screen space this element
      // occupies.
      LodComponent.RecalculateBounds();

      // If all LOD Sets are disabled, disable this set now.
      if (isDisabled) {
        Disable(true);
      }
    }

    /// <summary>
    /// Return whether or not this <see cref="LodSet"/>'s <see cref="GameObject"/> is empty (i.e.
    /// has been removed or has had all of its children geometry removed).
    /// </summary>
    internal bool IsEmpty() {
      return LodComponent == null || LodComponent.transform.childCount == 0;
    }

    /// <summary>
    /// Remove the <see cref="GameObject"/> created by this <see cref="LodSet"/> to contain its
    /// <see cref="LodComponent"/>.
    /// </summary>
    internal void Clear() {
      // Skip if GameObject containing LodComponent has already been removed.
      if (LodComponent == null) {
        return;
      }
      Destroy(LodComponent.gameObject);
    }

    /// <summary>Apply/revert disabling to this <see cref="LodSet"/>.</summary>
    /// <param name="disable">
    /// True to forcing this <see cref="LodSet"/> to display only Level of
    /// Detail 0 only, true to allowing display of all Levels of Detail.
    /// </param>
    internal void Disable(bool disable) {
      // To disable, disable Lod Component and manually set all geometry not inside LOD Group 0 to
      // inactive.
      if (disable) {
        LodComponent.enabled = false;
        bool isFirst = true;
        foreach (LOD lod in LodComponent.GetLODs()) {
          foreach (Renderer renderer in lod.renderers) {
            renderer.enabled = isFirst;
          }
          isFirst = false;
        }
        return;
      }

      // To enable, enable Lod Component and activate all geometry, allowing Lod Component to
      // control which geometry is shown.
      LodComponent.enabled = false;
      foreach (LOD lod in LodComponent.GetLODs()) {
        foreach (Renderer renderer in lod.renderers) {
          renderer.enabled = true;
        }
      }
    }

    /// <summary>Creates a new LOD of a given size with given geometry.</summary>
    private static LOD CreateLod(int lodLevel, params Renderer[] renderers) {
      return new LOD(Mathf.Pow(LodSize, lodLevel + 1), renderers);
    }

    /// <summary>Creates a new LOD of a given size but with no geometry.</summary>
    private static LOD CreateEmptyLod(int lodLevel) {
      return new LOD(Mathf.Pow(LodSize, lodLevel + 1), new Renderer[0]);
    }

    /// <summary>
    /// Return an expanded version of a given array that has one more element added to the
    /// end.
    /// </summary>
    /// <param name="array">Array to expand.</param>
    /// <param name="newEnd">New element to add to the end after all existing elements.</param>
    private static T[] ExpandArray<T>(T[] array, T newEnd) {
      return ExpandArray(array, newEnd, array.Length + 1);
    }

    /// <summary>
    /// Return an expanded version of the given array that has one more element added to the
    /// end.
    /// </summary>
    /// <param name="array">Array to expand.</param>
    /// <param name="newEnd">New element to add to the end after all existing elements.</param>
    /// <param name="newLength">
    /// New final length of the array (if not set, will just be one greater than the current
    /// length).
    /// </param>
    /// <remarks>
    /// It is assumed that the given array is not null, and that the new length is greater than the
    /// current length of the array.
    /// </remarks>
    private static T[] ExpandArray<T>(T[] array, T newEnd, int newLength) {
      T[] expandedArray = new T[newLength];
      array.CopyTo(expandedArray, 0);
      expandedArray[newLength - 1] = newEnd;
      return expandedArray;
    }

    /// <summary>
    /// Return an expanded version of a given array that has more elements added to the end.
    /// </summary>
    /// <param name="array">Array to expand.</param>
    /// <param name="newElements">
    /// New elements to add to the end after all existing elements.
    /// </param>
    private static T[] ExpandArray<T>(T[] array, T[] newElements) {
      T[] expandedArray = new T[array.Length + newElements.Length];
      array.CopyTo(expandedArray, 0);
      newElements.CopyTo(expandedArray, array.Length);
      return expandedArray;
    }
  }
}
