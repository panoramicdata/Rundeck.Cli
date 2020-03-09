namespace Rundeck.Cli.Config
{
	/// <summary>
	/// Application configuration, loaded from an appsettings.json file upon execution
	/// You can modify/extend this class and provide your own settings
	/// </summary>
	internal class Configuration
	{
		/// <summary>
		/// Rundeck credentials
		/// </summary>
		public RundeckCredentials RundeckCredentials { get; set; }

		/// <summary>
		/// DELETE THIS!
		/// Provided as a first example
		/// </summary>
		public string Setting1 { get; set; }
	}
}
