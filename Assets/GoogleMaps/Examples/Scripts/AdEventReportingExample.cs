using Google.Maps;
using Google.Maps.Ads;
using UnityEngine;

/// <summary>
/// Example demonstrating usage of the AdEventManager.
/// </summary>
[RequireComponent(typeof(MapsService))]
public sealed class AdEventReportingExample : MonoBehaviour {
  /// <summary>
  /// The <see cref="AdEventManager"/> used for logging events to.
  /// </summary>
  private AdEventManager AdEventManager;

  private void Start() {
    MapsService.EnableVerboseLogging(true);

    AdEventManager = GetComponent<MapsService>().AdEventManager;

    ReportAdImpressions();
  }

  /// <summary>
  /// Reports a series of fake ad impressions.
  /// </summary>
  private void ReportAdImpressions() {
    // An ad was displayed in the game.
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.PinShown,
        playerDistanceMetres: 100,
        normalizedInGameRewardValue: 0.9f);

    // The user clicked on the in-game pin, displaying the offer text and image.
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.OfferShown,
        playerDistanceMetres: 100,
        normalizedInGameRewardValue: 0.9f);
    // This also causes the game action to be displayed.
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.GameActionDisplayed,
        playerDistanceMetres: 100,
        normalizedInGameRewardValue: 0.9f);

    // The user approaches the ad.
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.LocationProximity90,
        playerDistanceMetres: 90,
        normalizedInGameRewardValue: 0.9f);
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.LocationProximity80,
        playerDistanceMetres: 80,
        normalizedInGameRewardValue: 0.9f);
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.LocationProximity70,
        playerDistanceMetres: 70,
        normalizedInGameRewardValue: 0.9f);
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.LocationProximity60,
        playerDistanceMetres: 60,
        normalizedInGameRewardValue: 0.9f);
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.LocationProximity50,
        playerDistanceMetres: 50,
        normalizedInGameRewardValue: 0.9f);
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.LocationProximity40,
        playerDistanceMetres: 40,
        normalizedInGameRewardValue: 0.9f);
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.LocationProximity30,
        playerDistanceMetres: 30,
        normalizedInGameRewardValue: 0.9f);
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.LocationProximity20,
        playerDistanceMetres: 20,
        normalizedInGameRewardValue: 0.9f);

    // At <20 metres, the user is able to complete the associated in-game action.
    AdEventManager.LogAdEvent(
        adName: "fakeAds/sampleSceneFakeAd",
        type: AdEventManager.AdEventType.GameActionCompleted,
        playerDistanceMetres: 20,
        normalizedInGameRewardValue: 0.9f);
  }
}
