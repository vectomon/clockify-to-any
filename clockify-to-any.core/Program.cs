using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Clockify.Net;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Users;
using Clockify.Net.Models.Workspaces;

namespace clockify_to_any.core
{
    class Program
    {
        static ClockifyClient Client { get; set; }

        static void Main(string[] args)
        {
            InitClient(args);
            var user = GetUser();
            var workspace = GetWorkspace();
            HandleTimeEntries(workspace, user);
        }

        static void InitClient(string[] args)
        {
            Console.WriteLine("Clockify kliens inicializálása...");
            var clockifyApiKeyProvider = new ClockifyApiKeyProvider(args);

            Client = new ClockifyClient(clockifyApiKeyProvider.GetApiKey());
        }

        static CurrentUserDto GetUser()
        {
            Console.WriteLine("Felhasználó adatainak lekérdezése...");
            return Client.GetCurrentUserAsync().Result.Data;
        }

        static WorkspaceDto GetWorkspace()
        {
            WorkspaceDto selectedWorkspace;

            Console.WriteLine("Workspace-ek lekérdezése...");
            var workspaces = Client.GetWorkspacesAsync().Result.Data;

            switch (workspaces.Count)
            {
                case 0: throw new Exception("Nem található workspace.");
                case 1:
                    selectedWorkspace = workspaces[0];
                    Console.WriteLine($"{selectedWorkspace.Name} az egyetlen talált workspace.");
                    break;
                default:
                    Console.WriteLine("Mely workspace bejegyzéseit szeretnéd könyvelni? (írd be a számát)");
                    for (var i = 0; i < workspaces.Count; ++i)
                    {
                        Console.WriteLine($"{i + 1}. {workspaces[i].Name}");
                    }
                    Console.Write("Választás: ");
                    int choice = int.Parse(Console.ReadLine());
                    selectedWorkspace = workspaces[choice - 1];
                    break;
            }

            return selectedWorkspace;
        }

        static void HandleTimeEntries(WorkspaceDto workspace, CurrentUserDto user)
        {
            Console.WriteLine("Időbejegyzések lekérdezése...");
            var entries = Client.FindAllTimeEntriesForUserAsync(workspace.Id, user.Id).Result.Data;

            var tmpFilePath = CreateEntryFile(entries);
            ProcessStartInfo psi = new ProcessStartInfo(tmpFilePath)
            {
                UseShellExecute = true
            };

            var p = System.Diagnostics.Process.Start(psi);
            System.Console.WriteLine(p.Id);
            System.Threading.Thread.Sleep(500000);
        }

        static string CreateEntryFile(List<TimeEntryDtoImpl> entries)
        {
            Console.WriteLine("Kérlek, határozd meg a sorsukat az alapértelmezett szövegszerkesztődben!");

            var tmpFilePath = Path.GetTempFileName();

            File.WriteAllLines(tmpFilePath, new List<string>()
            {
                "Kérlek, határozd meg a bejegyzések sorsát az alábbi táblázat segítségével:",
                "asd"
            });


            File.WriteAllLines(tmpFilePath, entries.Select(entry => entry.Description));

            return tmpFilePath;
        }
    }
}
