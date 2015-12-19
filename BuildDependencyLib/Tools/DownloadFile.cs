//// Copyright (c) 2014 Eberhard Beilharz
//// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
//using System;
//using System.IO;
//using System.Net;
//using System.Reflection;
//
//namespace BuildDependencyManager.Tools
//{
//	public class DownloadFile
//	{
//		public DownloadFile()
//		{
//		}
//
//		public int DoDownloadFile(String remoteFilename, String localFilename, String httpUsername, String httpPassword, out bool success)
//		{
//			// Function will return the number of bytes processed
//			// to the caller. Initialize to 0 here.
//			int bytesProcessed = 0;
//			success = true;
//
//			// Assign values to these objects here so that they can
//			// be referenced in the finally block
//			Stream remoteStream = null;
//			Stream localStream = null;
//			HttpWebResponse response = null;
//			DateTime lastModified = DateTime.Now;
//
//			// Use a try/catch/finally block as both the WebRequest and Stream
//			// classes throw exceptions upon error
//			try
//			{
//				// Create a request for the specified remote file name
//				var request = WebRequest.CreateHttp(remoteFilename);
//				// If a username or password have been given, use them
//				if (!string.IsNullOrEmpty(httpUsername) || !string.IsNullOrEmpty(httpPassword))
//				{
//					string username = httpUsername;
//					string password = httpPassword;
//					request.Credentials = new NetworkCredential(username, password);
//				}
//
//				// Prevent caching of requests so that we always download latest
//				var cacheControl = request.Headers[HttpRequestHeader.CacheControl];
//				request.Headers[HttpRequestHeader.CacheControl] = "no-cache";
//
//				if (File.Exists(localFilename))
//					request.IfModifiedSince = File.GetLastWriteTime(localFilename);
//
//				// Send the request to the server and retrieve the
//				// WebResponse object
//				response = (HttpWebResponse) request.GetResponse();
//
//				lastModified = response.LastModified;
//				if (File.Exists(localFilename) && lastModified == File.GetLastWriteTime(localFilename))
//					return bytesProcessed;
//
//				// Once the WebResponse object has been retrieved,
//				// get the stream object associated with the response's data
//				remoteStream = response.GetResponseStream();
//
//				// Create the local file
//				localStream = File.Create(localFilename);
//
//				// Allocate a 1k buffer
//				var buffer = new byte[1024];
//				int bytesRead;
//
//				// Simple do/while loop to read from stream until
//				// no bytes are returned
//				do
//				{
//					// Read data (up to 1k) from the stream
//					bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
//
//					// Write the data to the local file
//					localStream.Write(buffer, 0, bytesRead);
//
//					// Increment total bytes processed
//					bytesProcessed += bytesRead;
//				} while (bytesRead > 0);
//			}
//			catch (WebException wex)
//			{
//				if (wex.Status == WebExceptionStatus.ConnectFailure || wex.Status == WebExceptionStatus.NameResolutionFailure)
//				{
//					// We probably don't have a network connection (despite the check in the caller).
//					if (File.Exists(localFilename))
//					{
//						Log.LogMessage("Could not retrieve latest {0}. No network connection. Keeping existing file.", localFilename);
//					}
//					else
//					{
//						Log.LogError("Could not retrieve latest {0}. No network connection.", remoteFilename);
//						success = false; // Presumably can't continue
//					}
//					return 0;
//				}
//				string html = "";
//				if (wex.Response != null)
//				{
//					using (var sr = new StreamReader(wex.Response.GetResponseStream()))
//						html = sr.ReadToEnd();
//					Log.LogMessage("Could not download from {0}. Server responds {1}", remoteFilename, html);
//				}
//				else
//				{
//					Log.LogMessage("Could not download from {0}. no server response. Exception {1}. Status {2}",
//						remoteFilename, wex.Message, wex.Status);
//				}
//				success = false;
//				return 0;
//			}
//			catch (Exception e)
//			{
//				Log.LogError(e.Message);
//				Log.LogMessage(MessageImportance.Normal, e.StackTrace);
//				success = false;
//			}
//			finally
//			{
//				// Close the response and streams objects here
//				// to make sure they're closed even if an exception
//				// is thrown at some point
//				if (response != null) response.Close();
//				if (remoteStream != null) remoteStream.Close();
//				if (localStream != null)
//				{
//					localStream.Close();
//					var fi = new FileInfo(localFilename);
//					fi.LastWriteTime = lastModified;
//				}
//			}
//
//			// Return total bytes processed to caller.
//			return bytesProcessed;
//		}
//	}
//}
//
