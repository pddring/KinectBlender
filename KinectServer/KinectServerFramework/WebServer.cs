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
            logger.Log("Web server loading");
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
                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    Match m = Regex.Match(line, @"GET\s+(.*)\s+HTTP");
                    if(m.Success)
                    {
                        url = m.Groups[1].Value;
                    }
                }
                logger.Log($"Requested: {url}");
                if(url == "/")
                {
                    string html = "Hello!";
                    byte[] bytes = Encoding.UTF8.GetBytes(html);
                    StreamWriter sw = new StreamWriter(s);
                    sw.WriteLine($"HTTP/1.1 200 OK");
                    sw.WriteLine($"Date: {DateTime.Now}");
                    sw.WriteLine($"Server: KinectBlender");
                    sw.WriteLine($"Last-Modified: {DateTime.Now}");
                    sw.WriteLine($"Content-Length: {bytes.Length}");
                    sw.WriteLine("Content-Type: text/html");
                    sw.WriteLine("Connection: Closed");
                    sw.Write(bytes);
                    sw.WriteLine("\r\n");
                }
                
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
