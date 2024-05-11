using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Policy;
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
        ILogger logger;
        IRequestFrame frameGrabber;
        public WebServer(ILogger logger, IRequestFrame frameGrabber)
        {
            this.logger = logger;
            this.frameGrabber = frameGrabber;
            logger.Log($"Web server loading on {Dns.GetHostName()}");

            
        }
        public void Start()
        {
            if(running) return;
            running = true;
            serverThread = new Thread(async () =>
            {
                try
                {
                    TcpListener server = new TcpListener(IPAddress.Any, 80);
                    logger.Log("Starting webserver");

                    server.Start();

                    while (running)
                    {
                        await HandleClient(await server.AcceptTcpClientAsync());
                    }
                } catch (Exception ex)
                {
                    logger.Log(
                        
                        ex.Message);
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
            if (url == "/")
            {
                url = "/index.html";
            }

            string filename = System.AppContext.BaseDirectory + "static\\" + url.Replace("/","\\");

            // autodetect which armature to send if not specified
            if(filename.EndsWith("armature.json"))
            {
                filename = System.AppContext.BaseDirectory + "static\\armature0.json";
                for(int i = 0; i < 6; i++) 
                {
                    if(frameGrabber.IsLive(i))
                    {
                        filename = System.AppContext.BaseDirectory + $"static\\armature{i}.json";
                        break;
                    }
                }
            }

            // check if requesting an armature
            Match m = Regex.Match(filename, @"armature([0-5])\.json");

            
            if(m.Success)
            {
                int id = int.Parse(m.Groups[1].Value);
                string json = frameGrabber.GetArmature(id);
                logger.Log($"Sending armature {id}");
                Respond(s, HTTPResponseCode.OK, Encoding.UTF8.GetBytes(json), "text/json");

            } else

            // check if requesting a file
            if (File.Exists(filename)) {
                byte [] fileContents = File.ReadAllBytes(filename);
                logger.Log($"Responding with file {url}");
                Respond(s, HTTPResponseCode.OK, fileContents, GetMimeType(filename));
            } else
            {
                Respond(s, HTTPResponseCode.NOT_FOUND, Encoding.UTF8.GetBytes("File not found"), "text/html");
                logger.Log($"Responding with 404");
            }
        }

        enum HTTPResponseCode
        {
            OK = 200,
            NOT_FOUND = 404
        }

        void Respond(Stream s, HTTPResponseCode responseCode, byte[] bytes, string mimeType)
        {
            StreamWriter sw = new StreamWriter(s);
            
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>() {
                    { "Date", DateTime.Now.ToString() } ,
                    { "Server", "KinectBlender" },
                    { "Last-Modified", DateTime.Now.ToString() },
                    { "Content-Length", bytes.Length.ToString() },
                    { "Content-Type", mimeType },
                    { "Connection", "Closed" },
                };
            sw.WriteLine($"HTTP/1.1 {((int)responseCode)} {responseCode.ToString()}");
            foreach (string key in responseHeaders.Keys)
            {
                sw.WriteLine($"{key}: {responseHeaders[key]}");
            }

            sw.Write("\r\n");
            sw.Flush();
            s.Write(bytes, 0, bytes.Length);

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
