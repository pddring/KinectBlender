using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
                TcpListener server = new TcpListener(80);
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
                    logger.Log($"Requested: {url}");
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

        public void HandleURL(string url, Dictionary<string, string> headers, Stream s)
        {
            foreach(string key in headers.Keys)
            {
                logger.Log($"Header: {key} is {headers[key]}");
            }
            string html = "Hello!";
            byte[] bytes = Encoding.UTF8.GetBytes(html);
            StreamWriter sw = new StreamWriter(s);
            if(url == "/")
            {
                logger.Log($"Responding with {html}");
                sw.WriteLine($"HTTP/1.1 200 OK");
                sw.WriteLine($"Date: {DateTime.Now}");
                sw.WriteLine($"Server: KinectBlender");
                sw.WriteLine($"Last-Modified: {DateTime.Now}");
                sw.WriteLine($"Content-Length: {bytes.Length}");
                sw.WriteLine("Content-Type: text/html");
                sw.WriteLine("Connection: Closed");
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
