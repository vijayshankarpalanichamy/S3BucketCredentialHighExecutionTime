
using System.Configuration;

namespace MessageProcessor
{
    internal class ConfigService
    {
        private static ConfigService _instance;
        private string _profileName, _bucketName, _region, _profileLocation, _fileSystemUploadLocation;

        private ConfigService()
        {
            _region = ConfigurationManager.AppSettings["Region"];
            _bucketName = ConfigurationManager.AppSettings["BucketName"];
            AccessKey = ConfigurationManager.AppSettings["AccessKey"];
            SecretKey = ConfigurationManager.AppSettings["SecretKey"];
        }

        public static ConfigService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ConfigService();
            }

            return _instance;
        }

        public string ProfileName { get { return _profileName; } }
        public string BucketName { get { return _bucketName; } }
        public string Region { get { return _region; } }
        public string ProfileLocation { get { return _profileLocation; } }
        public string AccessKey { get; private set; }
        public string SecretKey { get; private set; }
        public string FileSystemUploadLocation
        {
            get { return _fileSystemUploadLocation; }
        }
    }
}
