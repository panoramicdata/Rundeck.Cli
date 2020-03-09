using Rundeck.Cli.Config;
using Rundeck.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Rundeck.Cli
{
	/// <summary>
	/// The main application
	/// </summary>
	internal class Application
	{
		/// <summary>
		/// Configuration
		/// </summary>
		private readonly Configuration _config;

		/// <summary>
		/// The client to use for API interaction
		/// </summary>
		private readonly RundeckClient _RundeckClient;

		/// <summary>
		/// The logger
		/// </summary>
		private readonly ILogger<Application> _logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="options"></param>
		/// <param name="loggerFactory"></param>
		public Application(
			IOptions<Configuration> options,
			ILoggerFactory loggerFactory)
		{
			// Store the config
			_config = options.Value;

			// Validate the credentials
			_config.RundeckCredentials.Validate();

			// Create a logger
			_logger = loggerFactory.CreateLogger<Application>();

			// Create a portal client
			_RundeckClient = new RundeckClient(
				new RundeckClientOptions
				{
					Uri = _config.RundeckCredentials.Uri,
					ApiToken = _config.RundeckCredentials.ApiToken,
					Logger = _logger
				}
			);
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			// Use _logger for logging
			_logger.LogInformation($"Application start.  Setting1 is set to {_config.Setting1}");

			// Use asynchronous calls to _RundeckClient to interact with the portal
			var organizations = await _RundeckClient
				.Organizations
				.GetAllAsync(cancellationToken)
				.ConfigureAwait(false);

			_logger.LogInformation($"You have access to {organizations.Count} organization{(organizations.Count > 1 ? "s" : "")}:");

			// Summarize each one:
			foreach (var organization in organizations)
			{
				// Get the networks:
				var networks = await _RundeckClient
				.Organizations
				.GetNetworksAsync(organization.Id)
				.ConfigureAwait(false);

				_logger.LogInformation($"- {organization.Name} with {networks.Count} network{(networks.Count > 1 ? "s" : "")}");
			}
		}
	}
}