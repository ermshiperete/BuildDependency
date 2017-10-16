// Copyright (c) 2014-2017 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BuildDependency.Tools;

namespace BuildDependency.Artifacts
{
	public class DownloadFileJob: IJob
	{
		public DownloadFileJob(string serializedJob)
		{
			var parts = serializedJob.Split('\t');
			if (parts.Length < 4 || parts[0] != JobTypeMarker)
				throw new SerializationException($"Unexpected line for DownloadFileJob: {serializedJob}");

			Conditions = (Conditions)Convert.ToInt32(parts[1]);
			Url = parts[2];
			TargetFile = parts[3];
		}

		public DownloadFileJob(Conditions conditions, string sourceFile, string url, string targetFile)
		{
			Conditions = conditions;
			Url = url;
			TargetFile = targetFile;
			SourceFile = sourceFile;
		}

		protected virtual string JobTypeMarker => "F";

		public string Url { get; }

		public string TargetFile { get; }

		public string SourceFile { get; }

		#region IJob implementation

		public Conditions Conditions { get; }

		public async Task<bool> Execute(ILog log, string workDir)
		{
			if (!Conditions.AreTrue())
				return true;

//			string httpUsername;
//			string httpPassword;

			// Assign values to these objects here so that they can
			// be referenced in the finally block
			Stream remoteStream = null;
			Stream localStream = null;
			HttpWebResponse response = null;
			var lastModified = DateTime.Now;

			var targetFile = Path.Combine(workDir, TargetFile);
			var tmpTargetFile = targetFile + ".~tmp";

			bool gotCachedFile = false;
			if (FileCache.Enabled)
			{
				var cachedFile = FileCache.GetCachedFile(Url);
				if (File.Exists(cachedFile))
				{
					gotCachedFile = true;
					log.LogMessage($"Found cached file {targetFile}");
					if (File.Exists(targetFile))
					{
						var targetFileInfo = new FileInfo(targetFile);
						var cachedFileInfo = new FileInfo(cachedFile);
						if (cachedFileInfo.LastWriteTimeUtc > targetFileInfo.LastWriteTimeUtc)
							CopyCachedFile(log, cachedFile, targetFile);
					}
					else
					{
						CopyCachedFile(log, cachedFile, targetFile);
					}
				}
			}
			if (!Network.IsOnline)
			{
				return gotCachedFile;
			}

			log.LogMessage("Checking {0}, downloading if newer...", TargetFile);

			// Use a try/catch/finally block as both the WebRequest and Stream
			// classes throw exceptions upon error
			try
			{
				// Create a request for the specified remote file name
				var request = WebRequest.Create(Url) as HttpWebRequest;
//				// If a username or password have been given, use them
//				if (!string.IsNullOrEmpty(httpUsername) || !string.IsNullOrEmpty(httpPassword))
//				{
//					string username = httpUsername;
//					string password = httpPassword;
//					request.Credentials = new NetworkCredential(username, password);
//				}

				bool appendFile = false;
				long tmpFileLength = 0;
				if (File.Exists(targetFile))
				{
					var fi = new FileInfo(targetFile);
					request.IfModifiedSince = fi.LastWriteTimeUtc;
				}
				if (File.Exists(tmpTargetFile))
				{
					// Interrupted download
					var fi = new FileInfo(tmpTargetFile);
					tmpFileLength = fi.Length;
					request.AddRange(tmpFileLength);
					appendFile = true;
				}

				// Send the request to the server and retrieve the
				// WebResponse object
				response = (HttpWebResponse) await Task.Factory.FromAsync(
					request.BeginGetResponse, request.EndGetResponse, null);

				// Once the WebResponse object has been retrieved,
				// get the stream object associated with the response's data
				remoteStream = response.GetResponseStream();

				// Create the local file
				Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
				localStream = File.OpenWrite(tmpTargetFile);
				if (appendFile && tmpFileLength < response.ContentLength)
					localStream.Position = localStream.Length;

				// Allocate a 1k buffer
				var buffer = new byte[1024];
				int bytesRead;

				// Simple do/while loop to read from stream until
				// no bytes are returned
				do
				{
					// Read data (up to 1k) from the stream
					bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

					// Write the data to the local file
					localStream.Write(buffer, 0, bytesRead);
				} while (bytesRead > 0);

				localStream.Close();
				File.Delete(targetFile);
				File.Move(tmpTargetFile, targetFile);
				if (FileCache.Enabled)
					FileCache.CacheFile(Url, targetFile);
			}
			catch (WebException wex)
			{
				if (wex.Status == WebExceptionStatus.ProtocolError)
				{
					var resp = wex.Response as HttpWebResponse;
					if (resp.StatusCode == HttpStatusCode.NotModified)
					{
						log.LogMessage("File {0} not modified.", TargetFile);
						return true;
					}
				}
				else if (wex.Status == WebExceptionStatus.ConnectFailure || wex.Status == WebExceptionStatus.NameResolutionFailure)
				{
					// We probably don't have a network connection (despite the check in the caller).
					if (File.Exists(TargetFile))
					{
						log.LogMessage("Could not retrieve latest {0}. No network connection. Keeping existing file.", TargetFile);
					}
					else
					{
						log.LogError("Could not retrieve latest {0}. No network connection.", Url);
					}
					return false;
				}
				if (wex.Response != null)
				{
					string html;
					using (var sr = new StreamReader(wex.Response.GetResponseStream()))
						html = sr.ReadToEnd();
					log.LogMessage("Could not download from {0}. Server responds {1}.", Url, html);
				}
				else
				{
					log.LogMessage("Could not download from {0}. No server response. Exception {1}. Status {2}.",
						Url, wex.Message, wex.Status);
				}
				return false;
			}
			catch (Exception e)
			{
				log.LogError(e.Message);
				log.LogMessage(e.StackTrace);
				return false;
			}
			finally
			{
				// Close the response and streams objects here
				// to make sure they're closed even if an exception
				// is thrown at some point
				response?.Close();
				remoteStream?.Close();
				if (localStream != null)
				{
					localStream.Close();
					var fi = new FileInfo(targetFile) { LastWriteTime = lastModified };
				}
			}

			log.LogMessage("Download of {0} finished after {1}.", TargetFile, DateTime.Now - lastModified);

			return true;
		}

		private static void CopyCachedFile(ILog log, string cachedFile, string targetFile)
		{
			log.LogMessage($"Copying {targetFile} from cache");
			Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
			File.Copy(cachedFile, targetFile, true);
		}

		public override string ToString()
		{
			return $"{JobTypeMarker}\t{(int) Conditions}\t{Url}\t{TargetFile}";
		}

		#endregion
	}
}

