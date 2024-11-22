using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text.Json;

class NetworkScanner
{
    private string[] liveIps;
    
    public async Task StartScan()
    {
        liveIps = new string[254];
        string baseIp = "192.168.1";
        int startIp = 1;
        int endIP = 254;
        string searchIP;
        int foundIps = 0;
        for (int i = startIp; i <= endIP; i++){
            searchIP = baseIp + "." + i.ToString();
        if(await PingSweepAsync(searchIP)){
            liveIps[foundIps] = searchIP;
            foundIps++;
        }
        }
        await ArrayTidy(liveIps);
    }
       private async Task<bool> PingSweepAsync(string ipAddress)
    {
        Ping ping = new Ping();
        try
        {
            PingReply reply = await ping.SendPingAsync(ipAddress, 1000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private async Task PingReport(String[] tidyIpArray){
            foreach (string ip in tidyIpArray)
        {
            string networkLocation = ip;
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ip);
                if (entry != null){
                    networkLocation = ip + "// " + entry.HostName;
                }
            }
            catch (SocketException ex)
            {

            }
            Console.WriteLine("Found live address:" + networkLocation);
            
           
        }
         Data pref = new Data
            {
                TargetIp = UserInput("Which IP address belongs to your NAS drive?"),
                SourceBkpLocations = UserInput("Enter the location you want to backup (highest lvl location)"),
                DestinationBkpLocations = UserInput("Enter where you want the backup to be save to (within IP address)"),
                Preferences = new Dictionary<string, string>{
                    {"TargetIP", "TargetIP"},
                    {"SourceLocation", "SourceBkpLocations"},
                    {"DestinationLocation", "DestinationBkpLocation"}
                }
            };
            string json = JsonSerializer.Serialize(pref, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("pref.json", json);
            Console.WriteLine("Preferences Saved");
    }

   private async Task ArrayTidy(string[] array){
        int length = array.Length;
        string[] tidyLiveIps = liveIps.Where(ip => ip != null).ToArray();
        await PingReport(tidyLiveIps);
    }

    private static String UserInput(string message)
    {
        Console.WriteLine(message);
        string userEntry = Console.ReadLine();
        return userEntry;
    }
}

class Data{
    public string TargetIp { get; set; }
    public string SourceBkpLocations { get; set; }
    public string DestinationBkpLocations { get; set;}
    public Dictionary<string, string> Preferences { get; set; } = new Dictionary<string, string>();
}

class Program{

    public static async Task Main(string[] args)
    {
        NetworkScanner scanner = new NetworkScanner();

        await scanner.StartScan();

      
    }
}
