using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ntp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (UdpClient client = new UdpClient())
            {
                //Variable ntpServer qui contient l'adresse IP ou le nom de domaine d'un serveur NTP public, tel que 0.ch.pool.ntp.org.
                //string ntpServer = "0.ch.pool.ntp.org";

                // NTP Server Pool and Reliability
                string[] ntpServers = {
                    "0.pool.ntp.org",
                    "1.pool.ntp.org",
                    "time.google.com",
                    "time.cloudflare.com"
                };

                // TODO : Try multiple servers for reliability

                // Initatialisation Variable timeMessage de type byte[] avec une taille de 48 octets
                byte[] timeMessage = new byte[48];
                timeMessage[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

                IPEndPoint ntpReference = null;
                bool connected = false;


                //Variable ntpReference de type IPEndPoint en utilisant le port 123 (port standard du protocole NTP)
                /*IPEndPoint ntpReference = new IPEndPoint(Dns.GetHostAddresses(ntpServer)[0], 123);
                client.Connect(ntpReference);
                client.Send(timeMessage, timeMessage.Length);
                timeMessage = client.Receive(ref ntpReference);*/

                foreach (string server in ntpServers)
                {
                    try
                    {
                        var addresses = Dns.GetHostAddresses(server);
                        ntpReference = new IPEndPoint(addresses[0], 123);

                        // Send the request to the current server
                        client.Send(timeMessage, timeMessage.Length, ntpReference);

                        // Receive response
                        timeMessage = client.Receive(ref ntpReference);

                        // If we got a response, this server is valid
                        Console.WriteLine($"Using NTP server: {server}");
                        connected = true;
                        break;
                    }
                    catch
                    {
                        Console.WriteLine($"Failed: {server}, trying next...");
                    }
                }

                if (!connected)
                {
                    Console.WriteLine("Could not contact any NTP server.");
                }



                DateTime ntpTime = NtpPacket.ToDateTime(timeMessage);

                Console.WriteLine($"Heure actuelle : {ntpTime}");
                Console.WriteLine($"Heure actuelle (format personnalisé) : {ntpTime.ToString("dd/MM/yyyy HH:mm:ss")}");
                Console.WriteLine($"Heure actuelle (ISO 8601) : {ntpTime.ToString("yyyy-MM-ddTHH:mm:ssZ")}");
                Console.WriteLine($"Heure actuelle (ISO 8601) : {ntpTime.ToString("dddd, dd/MM/yyyy")}");

                // 1. Calculate time difference (both in UTC)
                DateTime ntpTimeUtc = ntpTime; // Already UTC from NTP
                DateTime systemTimeUtc = DateTime.UtcNow;
                TimeSpan timeDiff = systemTimeUtc - ntpTimeUtc;
                Console.WriteLine($"Différence de temps : {timeDiff.TotalSeconds:F2} secondes");

                // 2. Convert to local time zone properly
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(ntpTimeUtc, TimeZoneInfo.Local);
                Console.WriteLine($"Heure locale : {localTime}");

                // 3. Convert to specific time zones
                TimeZoneInfo swissTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                DateTime swissTime = TimeZoneInfo.ConvertTimeFromUtc(ntpTimeUtc, swissTimeZone);
                Console.WriteLine($"Heure suisse : {swissTime}");

                TimeZoneInfo utcTimeZone = TimeZoneInfo.Utc;
                DateTime backToUtc = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, utcTimeZone);
                Console.WriteLine($"Retour vers UTC : {backToUtc}");

                Console.ReadLine();
                client.Close();
            }
        }
        // Exercise F: World Clock Display
        public static void DisplayWorldClocks(DateTime utcTime)
        {
            var timeZones = new[]
            {
                ("UTC", TimeZoneInfo.Utc),
                ("New York", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                ("London", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")),
                ("Tokyo", TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")),
                ("Sydney", TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"))
            };

            foreach (var (name, tz) in timeZones)
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
                Console.WriteLine($"{name}: {localTime:yyyy-MM-dd HH:mm:ss}");
            }
        }
    }
}