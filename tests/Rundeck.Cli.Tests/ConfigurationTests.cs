using Rundeck.Cli.Config;
using Xunit;

namespace Rundeck.Cli.Tests;

public class ConfigurationTests
{
	[Fact]
	public void Validate_WithUriAndToken_DoesNotThrow()
	{
		var credentials = new RundeckCredentials { Uri = "https://rundeck.example.com", ApiToken = "token" };

		credentials.Validate();
	}

	[Theory]
	[InlineData(null, "token", "Uri")]
	[InlineData("", "token", "Uri")]
	[InlineData("   ", "token", "Uri")]
	[InlineData("https://rundeck.example.com", null, "ApiToken")]
	[InlineData("https://rundeck.example.com", " ", "ApiToken")]
	public void Validate_WithMissingValue_ThrowsReportingThatValue(string uri, string token, string expectedName)
	{
		var credentials = new RundeckCredentials { Uri = uri, ApiToken = token };

		var exception = Assert.Throws<ConfigurationException>(credentials.Validate);

		var issue = Assert.Single(exception.Issues);
		Assert.Equal($"{expectedName} is not set", issue.Message);
	}

	[Fact]
	public void Validate_WithNothingSet_ReportsEveryIssue()
	{
		var exception = Assert.Throws<ConfigurationException>(new RundeckCredentials().Validate);

		Assert.Equal(2, exception.Issues.Count);
	}

	[Fact]
	public void ToString_WithIssues_ListsEachMessage()
	{
		var exception = new ConfigurationException([new ConfigurationIssue("first"), new ConfigurationIssue("second")]);

		var text = exception.ToString();

		Assert.Contains("first", text);
		Assert.Contains("second", text);
	}

	[Fact]
	public void ToString_WithoutIssues_DoesNotThrow()
	{
		var exception = new ConfigurationException("boom");

		Assert.Contains("boom", exception.ToString());
	}
}
