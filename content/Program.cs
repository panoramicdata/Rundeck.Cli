using Rundeck.Cli.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rundeck.Cli
{
	/// <summary>
	/// The program entry point
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The entry point
		/// </summary>
		/// <param name="args">The command line arguments</param>
		/// <returns>The process exit code</returns>
		public static async Task<int> Main(string[] args)
		{
			using var cancellationTokenSource = new CancellationTokenSource();

			void CancelEventHandler(object? sender, ConsoleCancelEventArgs eventArgs)
			{
				Console.WriteLine($"Key pressed: {eventArgs.SpecialKey}");

				// Prevent the process from terminating so the application can stop cleanly.
				eventArgs.Cancel = true;

				cancellationTokenSource.Cancel();
			}

			try
			{
				var serviceCollection = new ServiceCollection();
				var configurationRoot = BuildConfig(args);
				ConfigureServices(serviceCollection, configurationRoot);
				using var serviceProvider = serviceCollection.BuildServiceProvider();

				var application = serviceProvider.GetRequiredService<Application>();

				Console.CancelKeyPress += CancelEventHandler;

				await application
					.RunAsync(cancellationTokenSource.Token)
					.ConfigureAwait(false);
				return (int)ExitCode.Ok;
			}
			catch (OperationCanceledException)
			{
				// Cleanly handled.
				Console.WriteLine("Quitting.");
				return (int)ExitCode.Cancelled;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return (int)ExitCode.UnhandledException;
			}
			finally
			{
				Console.CancelKeyPress -= CancelEventHandler;
			}
		}

		internal static IConfigurationRoot BuildConfig(string[] args)
		{
			var appsettingsFilename = "appsettings.json";
			// If specifying any command line arguments, the first argument must be the path to the appsettings.json file ConfigFile: xxx
			if (args.Length > 0)
			{
				appsettingsFilename = args[0];
			}

			// Convert appsettingsFilename to absolute path for the ConfigurationBuilder to be able to find it
			appsettingsFilename = Path.GetFullPath(appsettingsFilename);

			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(appsettingsFilename, false, false)
				.Build();
		}

		internal static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration) =>
			// Add logging
			services
			.AddLogging(b =>
				{
					b.AddConsole();
					b.AddDebug();
				})
			.AddOptions()
			.Configure<Configuration>(c => configuration.GetSection("Configuration").Bind(c))
			.AddTransient<Application>();
	}
}