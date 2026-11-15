using System.Net;
using System.Text;

namespace Rundeck.Cli.Tests;

internal sealed class FakeRundeckServer : IDisposable
{
	private readonly HttpListener _listener = new();
	private readonly Task _loop;
	private readonly int _statusCode;
	private readonly string _body;

	public FakeRundeckServer(int statusCode, string body)
	{
		_statusCode = statusCode;
		_body = body;
		var port = GetFreePort();
		Uri = new Uri($"http://localhost:{port}/");
		_listener.Prefixes.Add(Uri.ToString());
		_listener.Start();
		_loop = Task.Run(ServeAsync);
	}

	public Uri Uri { get; }

	public List<(string Path, string? Token)> Requests { get; } = [];

	public void Dispose()
	{
		_listener.Close();
		try
		{
			_loop.Wait(TimeSpan.FromSeconds(5));
		}
		catch (AggregateException)
		{
			// The loop ends with an exception when the listener closes.
		}
	}

	private static int GetFreePort()
	{
		var probe = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
		probe.Start();
		var port = ((IPEndPoint)probe.LocalEndpoint).Port;
		probe.Stop();
		return port;
	}

	private async Task ServeAsync()
	{
		while (_listener.IsListening)
		{
			var context = await _listener.GetContextAsync().ConfigureAwait(false);
			lock (Requests)
			{
				Requests.Add((context.Request.Url!.AbsolutePath, context.Request.Headers["X-Rundeck-Auth-Token"]));
			}

			var bytes = Encoding.UTF8.GetBytes(_body);
			context.Response.StatusCode = _statusCode;
			context.Response.ContentType = "application/json";
			await context.Response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
			context.Response.Close();
		}
	}
}