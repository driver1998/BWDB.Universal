using Windows.UI.Xaml;
using Windows.System.Profile;

namespace BWDB.Universal
{
    public class DeviceFamilyTrigger : StateTriggerBase
    {
        string deviceFamily;

        public string DeviceFamily
        {
            get
            {
                return deviceFamily;
            }
            set
            {
                deviceFamily = value;

                SetActive(AnalyticsInfo.VersionInfo.DeviceFamily == deviceFamily);
            }
        }

    }
}