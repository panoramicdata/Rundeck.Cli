using System.Collections.Generic;

namespace Rundeck.Cli.Config
{
	/// <summary>
	/// Rundeck credentials
	/// </summary>
	public class RundeckCredentials
	{
		/// <summary>
		/// The Rundeck URI
		/// </summary>
		public string Uri { get; set; }

		/// <summary>
		/// The Rundeck API token
		/// </summary>
		public string ApiToken { get; set; }

		/// <summary>
		/// Ensures that all values are set and are of the expected length
		/// Throws an exception if this is not the case
		/// </summary>
		internal void Validate()
		{
			// Create a list of issues
			var issues = new List<ConfigurationIssue>();

			// Uri
			if (string.IsNullOrWhiteSpace(Uri))
			{
				issues.Add(new ConfigurationIssue($"{nameof(Uri)} is not set"));
			}

			// ApiToken
			if (string.IsNullOrWhiteSpace(ApiToken))
			{
				issues.Add(new ConfigurationIssue($"{nameof(ApiToken)} is not set"));
			}

			// Is everything OK?
			if (issues.Count == 0)
			{
				// Yes - return
				return;
			}
			// No

			throw new ConfigurationException(issues);
		}
	}
}