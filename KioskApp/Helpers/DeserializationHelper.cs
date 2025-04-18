namespace KioskApp.Helpers
{
    // Provides a flag to indicate when JSON deserialization is in progress
    public static class DeserializationHelper
    {
        // True while the application is performing deserialization, false otherwise
        public static bool IsDeserializing { get; set; } = false;
    }
}
