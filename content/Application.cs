using Rundeck.Cli.Config;
using Rundeck.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using System;

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
		private readonly RundeckClient _rundeckClient;

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
			_rundeckClient = new RundeckClient(
				new RundeckClientOptions
				{
					Uri = new Uri(_config.RundeckCredentials.Uri),
					ApiToken = _config.RundeckCredentials.ApiToken,
					Logger = _logger
				}
			);
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			// Use _logger for logging
			_logger.LogInformation($"Application start.  Setting1 is set to {_config.Setting1}");

			// Use asynchronous calls to _rundeckClient to interact with the API
			var projects = await _rundeckClient
				.Projects
				.GetAllAsync(cancellationToken)
				.ConfigureAwait(false);

			_logger.LogInformation($"You have access to {projects.Count} project{(projects.Count > 1 ? "s" : "")}:");

			// Summarize each one:
			foreach (var project in projects)
			{
				_logger.LogInformation($"- {project.Name}");
			}
		}
	}
}