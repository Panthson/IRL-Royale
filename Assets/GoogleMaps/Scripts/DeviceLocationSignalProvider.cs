#if ENABLE_GOOGLE_MAPS_LOCATION_AUTH
using Google.Maps.Location;

#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
#endif
#if UNITY_ANDROID
using UnityEngine;
#endif

namespace Google.Maps.Scripts {
  /// <summary>
  /// Script to provide device location signals from platform-specific libraries on Android and iOS.
  /// </summary>
  public class DeviceLocationSignalProvider : LocationSignalProvider {
#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>Android object that provides location signals.</summary>
    private AndroidJavaObject LocationAuthSignalsProvider;
#endif

#if UNITY_IOS && !UNITY_EDITOR
    /// <summary>Return values for MuskLocationAuthStartRecording from the iOS library.</summary>
    private enum RecordingState {
      Unknown = 0,
      Ok = 10,
      LocationServicesDisabled = 20,
      LocationAuthorizationDenied = 30,
      LocationAuthorizationUnknown = 31,
    }
#endif

    /// <inheritdoc/>
    public override bool StartRecording() {
#if UNITY_EDITOR
      return false;
#elif UNITY_ANDROID
      if (LocationAuthSignalsProvider != null) {
        // Already recording.
        return false;
      }
      try {
        var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

        LocationAuthSignalsProvider = new AndroidJavaObject(
            "com.google.geo.platform.locationauth.LocationAuthSignalsProvider", activity);

        LocationAuthSignalsProvider.Call("startRecording");
        return true;
      } catch (System.Exception e) {
        Debug.Log(e);
        return false;
      }
#elif UNITY_IOS
      var result = (RecordingState) MuskLocationAuthStartRecording();
      return result == RecordingState.Ok;
#else
      return false;
#endif
    }

    public override void SetKnownTrustLevel(LocationAuthenticator.KnownTrustLevelEnum trustLevel) {
#if UNITY_EDITOR
#elif UNITY_ANDROID
      string trustLevelStr;
      if (trustLevel == LocationAuthenticator.KnownTrustLevelEnum.LowTrust) {
        trustLevelStr = "LOW_TRUST";
      } else if (trustLevel == LocationAuthenticator.KnownTrustLevelEnum.HighTrust) {
        trustLevelStr = "HIGH_TRUST";
      } else {
        trustLevelStr = "TRUST_LEVEL_UNSPECIFIED";
      }
      var trustLevelEnum =
          new AndroidJavaClass("com.google.geo.locationauthenticity.v1.TrustLevel");
      var trustLevelValue = trustLevelEnum.GetStatic<AndroidJavaObject>(trustLevelStr);
      LocationAuthSignalsProvider.Call("setKnownTrustLevel", trustLevelValue);
#elif UNITY_IOS
      MuskLocationAuthSetKnownTrustLevel((int) trustLevel);
#endif
    }

    /// <inheritdoc/>
    public override byte[] StopRecording() {
#if UNITY_EDITOR
      return null;
#elif UNITY_ANDROID
      if (LocationAuthSignalsProvider == null) {
        return null;
      }
      try {
        var request = LocationAuthSignalsProvider.Call<AndroidJavaObject>("stopRecording");

        return request.Call<byte[]>("toByteArray");
      } catch (System.Exception e) {
        Debug.Log(e);
        return null;
      } finally {
        LocationAuthSignalsProvider = null;
      }
#elif UNITY_IOS
      IntPtr signalsPtr = IntPtr.Zero;
      try {
        int size = 0;
        signalsPtr = MuskLocationAuthStopRecording(out size);
        if (signalsPtr != IntPtr.Zero) {
          byte[] signalsData = new byte[size];
          Marshal.Copy(signalsPtr, signalsData, 0, size);
          return signalsData;
        }
      } finally {
        if (signalsPtr != IntPtr.Zero) {
          MuskLocationAuthRelease(signalsPtr);
        }
      }
      return null;
#else
      return null;
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern int MuskLocationAuthStartRecording();

    [DllImport("__Internal")]
    private static extern int MuskLocationAuthSetKnownTrustLevel(int trustLevel);

    [DllImport("__Internal")]
    private static extern IntPtr MuskLocationAuthStopRecording(out int size);

    [DllImport("__Internal")]
    private static extern void MuskLocationAuthRelease(IntPtr ptr);
#endif
  }
}
#endif  // ENABLE_GOOGLE_MAPS_LOCATION_AUTH
