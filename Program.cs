using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text.Json;
using System.Data;
using System.Runtime.InteropServices;

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

        MapDrives();

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

    private async Task MapDrives(){
        List<DriveInfo> driveList = new List<DriveInfo>{};
       
                DriveInfo[] drives = DriveInfo.GetDrives();

                foreach (DriveInfo drive in drives){
                    driveList.Add(drive);
                }

                foreach(DriveInfo drive in driveList){
                    Console.WriteLine("Found drive: "+drive.Name+"("+drive.VolumeLabel+" "+drive.RootDirectory+") with a " + drive.DriveFormat + " filesystem. Total space "+drive.TotalSize+" with "+drive.TotalFreeSpace+" of free space ("+drive.AvailableFreeSpace+" available) Drive is ready "+drive.IsReady);
                }
                Console.WriteLine("Get all directories in drives? ('N' - No, 'Y'-Yes for all, '[Drive name]- Yes for specific drive')");
                switch(Console.ReadLine()){
                    case "Y":
                    List<string> dirList = new List<string>();
                  
                    var queryRootDir =
                        from drive in driveList
                        select new {drive.RootDirectory};
                    foreach (var rootDir in queryRootDir){
                        dirList.Add(rootDir.ToString());
                    };
                    foreach(string dir in dirList){
                        List<string> subDirList = new List<string>(Directory.EnumerateDirectories(dir));
                        foreach(string subDir in subDirList){
                            Console.WriteLine(subDir);
                        };
                    };
                    break;
                    case "N":
                    break;
                    default:
                    break;
                }
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
