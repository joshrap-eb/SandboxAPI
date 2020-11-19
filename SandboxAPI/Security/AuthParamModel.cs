using System.ComponentModel.DataAnnotations;

namespace SandboxAPI.Security
{
	public class AuthParamModel
	{
		[Required] public string TenantId { get; set; }

		public string ClientId { get; set; }

		public bool WritePermissions { get; set; }

		public bool ReadPermissions { get; set; }
	}
}