using OneSignalPush.MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    [SerializeField] private GameObject panelViewStatusOcurence;
    [SerializeField] private GameObject iconViewStatusOcurence;
    [SerializeField] private GameObject panelInformationOcurrence;
    [SerializeField] private GameObject iconInformationOcurrence;
    [SerializeField] private GameObject panelCallEmergency;
    [SerializeField] private GameObject iconCallEmergency;
    [SerializeField] private GameObject panelChat;
    [SerializeField] private GameObject iconChat;

    public void OnButtonViewStatusOcurrenceCliked()
    {
        DisablePanels();
        DisableIcons();
        panelViewStatusOcurence.SetActive(true);
        iconViewStatusOcurence.SetActive(true);
        SendNotification();
    }

    public void OnButtonInformationOcurrenceCliked()
    {
        DisablePanels();
        DisableIcons();
        panelInformationOcurrence.SetActive(true);
        iconInformationOcurrence.SetActive(true);
    }

    public void OnButtonCallEmergencyCliked()
    {
        DisablePanels();
        DisableIcons();
        panelCallEmergency.SetActive(true);
        iconCallEmergency.SetActive(true);
    }

    public void OnButtonChatCliked()
    {
        DisablePanels();
        DisableIcons();
        panelChat.SetActive(true);
        iconChat.SetActive(true);
    }

    public void OpenMaps()
    {
        Application.OpenURL(string.Concat("https://www.google.com/maps/dir/Current+Location/", StatusCallManager.getAddressComplete));
    }

    private void DisablePanels()
    {
        panelViewStatusOcurence.SetActive(false);
        panelInformationOcurrence.SetActive(false);
        panelCallEmergency.SetActive(false);
        panelChat.SetActive(false);
    }

    private void DisableIcons()
    {
        iconViewStatusOcurence.SetActive(false);
        iconInformationOcurrence.SetActive(false);
        iconCallEmergency.SetActive(false);
        iconChat.SetActive(false);
    }

    // Test Notification

    private void SendNotification()
    {
        var extraMessage = "";

        OneSignal.IdsAvailable((userId, pushToken) => {

            userId = OneSignal.GetPermissionSubscriptionState().subscriptionStatus.userId;

            if (pushToken != null)
            {
                // See http://documentation.onesignal.com/docs/notifications-create-notification for a full list of options.
                // You can not use included_segments or any fields that require your OneSignal 'REST API Key' in your app for security reasons.
                // If you need to use your OneSignal 'REST API Key' you will need your own server where you can make this call.

                var notification = new Dictionary<string, object>();
                notification["contents"] = new Dictionary<string, string>() { { "en", "Made with Unity" } };
                // Send notification to this device.
                notification["include_player_ids"] = new List<string>() { userId };
                // Example of scheduling a notification in the future.
                notification["send_after"] = DateTime.Now.ToUniversalTime().AddSeconds(0).ToString("U");

                extraMessage = "Posting test notification now.";
                OneSignal.PostNotification(notification, (responseSuccess) => {
                    extraMessage = "Notification posted successful! Delayed by about 30 secounds to give you time to press the home button to see a notification vs an in-app alert.\n" + Json.Serialize(responseSuccess);
                }, (responseFailure) => {
                    extraMessage = "Notification failed to post:\n" + Json.Serialize(responseFailure);
                });
            }
            else
                extraMessage = "ERROR: Device is not registered.";
        });
    }

    // Finish test
}
