using System.Runtime.Serialization;

namespace S3WebAPI
{
    public class ApiResponse
    {

        [DataMember]
        public string Success { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object Error { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object Payload { get; set; }

        public ApiResponse(string success, object result = null, object errorMessage = null)
        {
            Success = success;
            if (result != null)
            {
                Payload = result;
            }
            if (errorMessage != null)
            {
                Error = errorMessage;//new { code = (int)statusCode, message = errorMessage };
            }
        }
    }
}
