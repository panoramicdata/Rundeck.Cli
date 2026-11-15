using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rundeck.Cli.Config;
using Xunit;

namespace Rundeck.Cli.Tests;

public sealed class ProgramTests : IDisposable
{
	private const string TwoProjects = """[{"name":"alpha","description":"first"},{"name":"beta","description":"second"}]""";

	private readonly string _directory = Directory.CreateTempSubdirectory("rundeck-cli-tests").FullName;

	public void Dispose() => Directory.Delete(_directory, true);

	private string WriteConfig(string uri, string token, string setting = "value")
	{
		var path = Path.Combine(_directory, "appsettings.json");
		File.WriteAllText(
			path,
			$$$"""{"Configuration":{"RundeckCredentials":{"Uri":"{{{uri}}}","ApiToken":"{{{token}}}"},"Setting1":"{{{setting}}}"}}""");
		return path;
	}

	private static ServiceProvider BuildProvider(IConfiguration configuration)
		=> Program.ConfigureServices(new ServiceCollection(), configuration).BuildServiceProvider();

	[Fact]
	public void BuildConfig_WithPathArgument_ReadsThatFile()
	{
		var path = WriteConfig("http://rundeck.example.com/", "secret", "custom");

		var configuration = Program.BuildConfig([path]);

		Assert.Equal("custom", configuration["Configuration:Setting1"]);
		Assert.Equal("secret", configuration["Configuration:RundeckCredentials:ApiToken"]);
	}

	[Fact]
	public void BuildConfig_WithMissingFile_Throws()
		=> Assert.Throws<FileNotFoundException>(() => Program.BuildConfig([Path.Combine(_directory, "missing.json")]));

	[Fact]
	public void ConfigureServices_BindsConfigurationSection()
	{
		var path = WriteConfig("http://rundeck.example.com/", "secret", "bound");
		using var provider = BuildProvider(Program.BuildConfig([path]));

		var options = provider.GetRequiredService<IOptions<Configuration>>().Value;

		Assert.Equal("bound", options.Setting1);
		Assert.Equal("http://rundeck.example.com/", options.RundeckCredentials.Uri);
	}

	[Fact]
	public void ConfigureServices_WithBlankCredentials_FailsWhenApplicationResolved()
	{
		var path = WriteConfig(string.Empty, string.Empty);
		using var provider = BuildProvider(Program.BuildConfig([path]));

		var exception = Assert.Throws<ConfigurationException>(() => provider.GetRequiredService<Application>());

		Assert.Equal(2, exception.Issues.Count);
	}

	[Fact]
	public async Task Main_WithMissingConfigFile_ReturnsUnhandledException()
	{
		var code = await Program.Main([Path.Combine(_directory, "missing.json")]);

		Assert.Equal((int)ExitCode.UnhandledException, code);
	}

	[Fact]
	public async Task Main_WithBlankCredentials_ReturnsUnhandledException()
	{
		var code = await Program.Main([WriteConfig(string.Empty, string.Empty)]);

		Assert.Equal((int)ExitCode.UnhandledException, code);
	}

	[Fact]
	public async Task Main_WithReachableServer_ReturnsOkAndSendsToken()
	{
		using var server = new FakeRundeckServer(200, TwoProjects);

		var code = await Program.Main([WriteConfig(server.Uri.ToString(), "my-token")]);

		Assert.Equal((int)ExitCode.Ok, code);
		var request = Assert.Single(server.Requests);
		Assert.EndsWith("/projects", request.Path);
		Assert.Equal("my-token", request.Token);
	}

	[Fact]
	public async Task Main_WhenServerFails_ReturnsUnhandledException()
	{
		using var server = new FakeRundeckServer(500, "{}");

		var code = await Program.Main([WriteConfig(server.Uri.ToString(), "my-token")]);

		Assert.Equal((int)ExitCode.UnhandledException, code);
	}

	[Fact]
	public async Task Application_RunAsync_WithCancelledToken_ThrowsOperationCancelled()
	{
		using var server = new FakeRundeckServer(200, TwoProjects);
		using var provider = BuildProvider(Program.BuildConfig([WriteConfig(server.Uri.ToString(), "my-token")]));
		var application = provider.GetRequiredService<Application>();
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => application.RunAsync(cancellation.Token));
	}

	[Fact]
	public void ExitCode_HasStableValues()
	{
		Assert.Equal(0, (int)ExitCode.Ok);
		Assert.Equal(1, (int)ExitCode.UnhandledException);
		Assert.Equal(2, (int)ExitCode.Cancelled);
	}
}
