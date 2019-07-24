using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> AreaBlack = new Dictionary<string, string>();
            Dictionary<string, string> AreaWhite = new Dictionary<string, string>();
            AreaBlack.Add("1814991","CN");
            //AreaWhite.Add("1668284", "TW");
            //AreaWhite.Add("1819730", "HK");
            //AreaWhite.Add("1835841", "KR");
            //AreaWhite.Add("1861060", "JP");
            //AreaWhite.Add("1880251", "SG");
            //AreaWhite.Add("2017370", "RU");
            //AreaWhite.Add("2077456", "AU");
            //AreaWhite.Add("2635167", "GB");
            //AreaWhite.Add("2921044", "DE");
            //AreaWhite.Add("3017382", "FR");
            //AreaWhite.Add("6251999", "CA");
            //AreaWhite.Add("6252001", "US");
            //AreaWhite.Add("6255147", "Asia");
            //AreaWhite.Add("6255148", "Euro");
            List<IPNetwork> Routes = new List<IPNetwork>();
            WebClient client = new WebClient();

            var URLAddress = "https://geolite.maxmind.com/download/geoip/database/GeoLite2-Country-CSV.zip";
            Stream DatabaseFile = client.OpenRead(URLAddress);
            ZipInputStream DatabaseStream = new ZipInputStream(DatabaseFile);
            ZipEntry zipEntry = DatabaseStream.GetNextEntry();
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;
                if (entryFileName.Length == 0)
                {
                    zipEntry = DatabaseStream.GetNextEntry();
                    continue;
                }
                if (!entryFileName.Contains(".csv"))
                {
                    zipEntry = DatabaseStream.GetNextEntry();
                    continue;
                }
                if (!entryFileName.Contains("Blocks"))
                {
                    zipEntry = DatabaseStream.GetNextEntry();
                    continue;
                }
                MemoryStream memStream = new MemoryStream();
                DatabaseStream.CopyTo(memStream);
                var result = System.Text.Encoding.UTF8.GetString(memStream.ToArray()).Split('\n');
                foreach (var line in result)
                {
                    if (line == "")
                    {
                        continue;
                    }
                    var thisline = line.Split(',');
                    if (AreaBlack.ContainsKey(thisline[1]))
                    {
                        continue;
                    }
                    if (AreaBlack.ContainsKey(thisline[2]))
                    {
                        continue;
                    }
                    //if (!AreaWhite.ContainsKey(thisline[1]))
                    //{
                    //    continue;
                    //}
                    //if (!AreaWhite.ContainsKey(thisline[2]))
                    //{
                    //    continue;
                    //}
                    if (!thisline[0].Contains("/"))
                    {
                        continue;
                    }
                    IPNetwork thisNetwork = IPNetwork.Parse(thisline[0]);
                    Routes.Add(thisNetwork);
                }
                zipEntry = DatabaseStream.GetNextEntry();
            }


            String APNICURLAddress = "http://ftp.apnic.net/apnic/stats/apnic/delegated-apnic-latest";
            Stream APNICDatabaseFile = client.OpenRead(APNICURLAddress);
            MemoryStream APNICFileStream = new MemoryStream();
            APNICDatabaseFile.CopyTo(APNICFileStream);
            var APNICData = System.Text.Encoding.UTF8.GetString(APNICFileStream.ToArray()).Split('\n');
            foreach (var line in APNICData)
            {
                if (line == "")
                {
                    continue;
                }
                if (!line.Contains("|"))
                {
                    continue;
                }
                if (line.Contains("asn"))
                {
                    continue;
                }
                var thisline = line.Split('|');
                if (thisline[1].Contains("CN"))
                {
                    continue;
                }
                if (thisline[2].Contains("v4"))
                {
                    IPNetwork thisNetwork = IPNetwork.Parse(thisline[3] + "/" + (32-Math.Round(Math.Log(2, System.Convert.ToDouble(thisline[4])))).ToString());
                    Routes.Add(thisNetwork);
                }
                else
                {
                    IPNetwork thisNetwork = IPNetwork.Parse(thisline[3]+"/"+ thisline[4]);
                    Routes.Add(thisNetwork);
                }
                
            }

            IPNetwork[] Source=Routes.ToArray();
            IPNetwork[] SuperNetResult = IPNetwork.Supernet(Source);
            while (true)
            {
                Source = SuperNetResult;
                SuperNetResult = IPNetwork.Supernet(Source);
                if(Source.Length == SuperNetResult.Length)
                {
                    if(Enumerable.SequenceEqual(Source, SuperNetResult))break;
                }
            }
            foreach (var net in SuperNetResult)
            {
                Console.WriteLine(net.ToString());
            }
            Console.WriteLine(Routes.Count);
            Console.WriteLine(SuperNetResult.Length);
            Console.WriteLine();
        }
    }
}
