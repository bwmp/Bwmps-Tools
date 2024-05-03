using System.IO;
using System.Runtime.Serialization.Json;

namespace BwmpsTools.Utils
{
    public class SettingsManager
    {
        private static SettingsManager instance;
        public static SettingsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SettingsManager();
                    instance.LoadSettings();
                }
                return instance;
            }
        }

        public string settingsFilePath = Utilities.GetBwmpAssetPath("settings.json");
        public Settings currentSettings;

        public bool DebugMode
        {
            get { return currentSettings.debugMode; }
            set { currentSettings.debugMode = value; }
        }

        private SettingsManager()
        {
            if (!File.Exists(settingsFilePath))
            {
                currentSettings = new Settings();
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                using (FileStream fs = new FileStream(settingsFilePath, FileMode.Create))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Settings));
                    serializer.WriteObject(fs, currentSettings);
                }

                Utilities.DebugLog("Settings saved successfully!");
            }
            catch (System.Exception e)
            {
                Utilities.DebugLog("Failed to save settings: " + e.Message);
            }
        }

        public void LoadSettings()
        {
            try
            {
                using (FileStream fs = new FileStream(settingsFilePath, FileMode.OpenOrCreate))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Settings));
                    currentSettings = (Settings)serializer.ReadObject(fs);
                }

                Utilities.DebugLog("Settings loaded successfully!");
            }
            catch (System.Exception e)
            {
                Utilities.DebugLog("Failed to load settings: " + e.Message);
            }
        }
    }
}
