using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rundeck.Cli.Config
{
	/// <summary>
	/// A configuration exception
	/// </summary>
	[Serializable]
	public class ConfigurationException : Exception
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ConfigurationException()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="issues">The issues</param>
		public ConfigurationException(List<ConfigurationIssue> issues)
		{
			Issues = issues.AsReadOnly();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">The message</param>
		public ConfigurationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="innerException">The inner exception</param>
		public ConfigurationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// The issues
		/// </summary>
		/// <returns></returns>
		public ReadOnlyCollection<ConfigurationIssue> Issues { get; }

		/// <inheritdoc />
		public override string ToString() => $"Configuration issues:\r\n{Issues.Select(i => i.Message + "\r\n")}";
	}
}