﻿using Daddoon.Blazor.Xam.Template.Interop;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Xamarin.Forms;

namespace Daddoon.Blazor.Xam.Template.Services
{
    public static class WebApplicationFactory
    {
        private static HttpServer server;
        private static bool _isStarted = false;

        private static bool IsStarted()
        {
            return _isStarted;
        }

        private static object streamLock = new object();

        private static string BlazorPackageFolder = "Mobile.bin.app.zip";

        /// <summary>
        /// Only work for on this assembly
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static void SetBlazorPackageRelativePath(string path)
        {
            path = path.Replace(@"\", ".");
            path = path.Replace("/", ".");
            BlazorPackageFolder = path;
        }

        public static MemoryStream GetResourceStream(string path)
        {
            var assembly = typeof(WebApplicationFactory).Assembly;

            string appPackage = $"{assembly.GetName().Name}.{BlazorPackageFolder}";
            using (Stream dataSource = assembly.GetManifestResourceStream(appPackage))
            {
                ZipArchive archive = null;
                archive = new ZipArchive(dataSource, ZipArchiveMode.Read);

                //data will contain found file output as stream
                MemoryStream data = new MemoryStream();
                ZipArchiveEntry entry = archive.GetEntry(path);

                if (entry != null)
                {
                    entry.Open().CopyTo(data);
                    data.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    data?.Dispose();
                    return null;
                }

                archive?.Dispose();

                return data;
            }
        }

        public static byte[] GetResource(string path)
        {
            MemoryStream content = GetResourceStream(path);
            if (content == null)
                return null;
            byte[] resultSet = content?.ToArray();
            content?.Dispose();

            return resultSet;
        }

        public static string GetContentType(string path)
        {
            if (path.EndsWith(".wasm"))
            {
                return "application/wasm";
            }
            if (path.EndsWith(".dll"))
            {
                return "application/octet-stream";
            }

            //No critical mimetypes to check
            return MimeTypes.GetMimeType(path);
        }

        private const int HttpPort = 8888;

        public static int GetHttpPort()
        {
            return HttpPort;
        }

        public static string GetBaseURL()
        {
            return $"http://localhost:{GetHttpPort()}";
        }

        public static string GetQueryPath(string path)
        {
            path = WebUtility.UrlDecode(path.TrimStart('/'));

            if (string.IsNullOrEmpty(path))
            {
                //We are calling the index page. We must call index.html but hide it from url for Blazor routing.
                path = "index.html";
            }

            return path;
        }

        public static void StartWebServer()
        {
            //Request are managed by the WebView on Android
            if (Device.RuntimePlatform == Device.Android)
            {
                _isStarted = true;
                return;
            }

            server = new HttpServer(HttpPort);
            server.Register(string.Empty, (req, resp) =>
            {
                //req.SubPath doesn't return QueryString, so we don't have to do additional operations for ZIP search
                string path = GetQueryPath(req.SubPath);

                byte[] content = GetResource(path);

                if (content == null)
                {
                    //Content not found
                    resp.StatusCode = 404;
                    return;
                }

                //Prevent caching in local browser
                resp.SetHeader("Cache-Control", "no-cache");
                resp.StatusCode = 200;
                resp.ContentType = GetContentType(path);
                resp.Write(content);
            }, true, true);

            _isStarted = true;
        }

        public static void StopWebServer()
        {
            server?.Dispose();
            server = null;

            _isStarted = false;
        }
    }
}