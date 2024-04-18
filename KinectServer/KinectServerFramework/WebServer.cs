using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace KinectServerFramework
{
    internal class WebServer
    {
        private bool running = false;
        Thread serverThread;
        Logger logger;
        public WebServer(Logger logger)
        {
            this.logger = logger;
            logger.Log($"Web server loading on {Dns.GetHostName()}");

            
        }
        public void Start()
        {
            if(running) return;
            running = true;
            serverThread = new Thread(async () =>
            {
                TcpListener server = new TcpListener(IPAddress.Any, 80);
                logger.Log("Starting webserver");

                server.Start();
                
                while(running)
                {                    
                    await HandleClient(await server.AcceptTcpClientAsync());
                }
            });
            serverThread.Start();
        }

        async Task HandleClient(TcpClient client)
        {            
            string url = "";
            using (NetworkStream s = client.GetStream())
            {
                
                StreamReader sr = new StreamReader(s);
                
                string line = sr.ReadLine();
                if(line == null)
                {
                    return;
                }

                Match m = Regex.Match(line, @"GET\s+(.*)\s+HTTP");
                if (m.Success)
                {
                    url = m.Groups[1].Value;
                    logger.Log($"{client.Client.RemoteEndPoint} requested: {url}");
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    while (line.Length > 0)
                    {
                        line = sr.ReadLine();
                        m = Regex.Match(line, @"^(.*?):\s?(.*)");
                        if (m.Success)
                        {
                            headers.Add(m.Groups[1].Value, m.Groups[2].Value);
                        }


                    }
                    HandleURL(url, headers, s);
                }
                
                                
            }
        }

        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public void HandleURL(string url, Dictionary<string, string> headers, Stream s)
        {
            string html = "Not found!";
            
            StreamWriter sw = new StreamWriter(s);


            if (url == "/")
            {
                url = "/index.html";
            }

            string filename = System.AppContext.BaseDirectory + "static\\" + url.Replace("/","\\");

            if (File.Exists(filename)) {
                byte[] bytes = File.ReadAllBytes(filename);

                logger.Log($"Responding with file {url}");
                Dictionary<string, string> responseHeaders = new Dictionary<string, string>() {
                    { "Date", DateTime.Now.ToString() } ,
                    { "Server", "KinectBlender" },
                    { "Last-Modified", DateTime.Now.ToString() },
                    { "Content-Length", bytes.Length.ToString() },
                    { "Content-Type", GetMimeType(filename) },
                    { "Connection", "Closed" },
                };
                sw.WriteLine($"HTTP/1.1 200 OK");
                foreach (string key in responseHeaders.Keys)
                {
                    sw.WriteLine($"{key}: {responseHeaders[key]}");
                }
                
                sw.Write("\r\n");
                sw.Flush();
                s.Write(bytes, 0, bytes.Length);
            } else
            {
                logger.Log($"Responding with 404");
                sw.WriteLine($"HTTP/1.1 404 Not found");
                sw.Write("\r\n");
                sw.Flush();
            }
        }

        public void Stop()
        {
            running = false;
        }

        public bool IsRunning()
        {
            return running;
        }
    }
}
