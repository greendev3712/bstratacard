using System;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace Lib {
    public class SSLTcpClient 
    {   
        private static Hashtable certificateErrors = new Hashtable();
        private TcpClient m_client;
        private SslStream m_sslStream;
        private string m_serverName;
        private string m_backlog = "";
        private int m_timeout_milliseconds = 10000;
        
        public SSLTcpClient(string host, int port, string serverName)
        {
            m_serverName = serverName;

            m_client = new TcpClient(host, port);
            Console.WriteLine("SSLTcpCLient() SSL Client connected.");

            // Create an SSL stream that will close the client's stream.
            m_sslStream = new SslStream(
                m_client.GetStream(),
                true,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
            );

            try {
                m_sslStream.AuthenticateAsClient(serverName);
            }
            catch (AuthenticationException e) {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null) {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }

                Console.WriteLine("Authentication failed - closing the connection.");
                m_client.Close();
                return;
            }

            m_sslStream.ReadTimeout = 10;
        }

        public void WriteLine(string line)
        {
            QD.p("To ToolBarServer -> " + line, 1);
            byte[] messsage = Encoding.ASCII.GetBytes(line + "\n"); //  .UTF8
            m_sslStream.Write(messsage);
            m_sslStream.Flush();
        }
                   
        public string ReadLine() {
            Stopwatch stop_watch = new Stopwatch();
            string return_line;
            byte [] buffer = new byte[1024];
            int bytes = -1;
            int newline_pos = -1;
            
            do
            {
                stop_watch.Start();

                do {
                    try {
                        bytes = m_sslStream.Read(buffer, 0, buffer.Length);
                    }
                    catch (IOException e) {
                        // Timeout
                        Exception ex = e; // Get rid of warning
                        bytes = -1;
                        break;
                    }
                }
                while((bytes <= 0) && (stop_watch.Elapsed.Milliseconds < m_timeout_milliseconds));

                if (bytes <= 0) {
                    break;
                }

                // Use Decoder class to convert from bytes
                Decoder decoder = Encoding.ASCII.GetDecoder(); // Could use UTF8 here if we need to.. multi language stuff?
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);

                // ETB - End Of Transmission?
                if (chars[0] == 23) {
                    // try again
                    continue;
                }

                m_backlog += new string(chars);

                if ((newline_pos = m_backlog.IndexOf("\n")) != -1) {
                    return_line = m_backlog.Substring(0, newline_pos);
                    m_backlog = m_backlog.Substring(newline_pos + 1);

                    QD.p("From ToolBarServer -> " + return_line, 2);
                    return return_line;
                }
            }
            while (bytes != 0); 

            return null;
        }
    
        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Okay For now
            SslPolicyErrors errors_dont_care = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors;
            SslPolicyErrors errors_remainder = sslPolicyErrors ^ errors_dont_care;

            if (errors_remainder == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

    }
}