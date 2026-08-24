using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

public class FirebasePushNotificationSender : IPushNotificationSender
{
    private readonly ILogger<FirebasePushNotificationSender> _logger;

    public FirebasePushNotificationSender(ILogger<FirebasePushNotificationSender> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(
        string deviceToken,
        string title,
        string body,
        IDictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
            Data = new Dictionary<string, string>(data),
            Android = new AndroidConfig
            {
                // Delivery priority (wakes the device promptly) — separate from the
                // channel below, which controls visual importance/banner behavior.
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    // Must match the channel id NotificationService.initialize() creates
                    // on the Flutter side, or Android silently falls back to its default
                    // (low-importance, no heads-up banner) channel.
                    ChannelId = "physiolink_default",
                },
            },
        };

        try
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
        }
        catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
        {
            
            // service layer can null out Patient.DeviceToken instead of retrying forever.
            _logger.LogWarning("FCM token unregistered for send, caller should clear it");
            throw;
        }
    }
}