using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Clockify.Net;
using Clockify.Net.Models.Projects;
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
            var projects = GetProjects(workspace);
            HandleTimeEntries(workspace, user, projects);
        }

        static Dictionary<string, ProjectDtoImpl> GetProjects(WorkspaceDto workspace)
        {
            return Client.FindAllProjectsOnWorkspaceAsync(workspace.Id).Result.Data.ToDictionary(p => p.Id, p => p);
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

        static void HandleTimeEntries(WorkspaceDto workspace, CurrentUserDto user, Dictionary<string, ProjectDtoImpl> projects)
        {
            Console.WriteLine("Időbejegyzések lekérdezése...");
            var entries = Client.FindAllTimeEntriesForUserAsync(workspace.Id, user.Id).Result.Data;

            var tmpFilePath = CreateEntryFile(entries, projects);
            ProcessStartInfo editorPsi = new ProcessStartInfo("code", tmpFilePath)
            {
                UseShellExecute = true
            };

            var editorProcess = Process.Start(editorPsi);
            editorProcess.WaitForExit();
        }

        static string CreateEntryFile(List<TimeEntryDtoImpl> entries, Dictionary<string, ProjectDtoImpl> projects)
        {
            Console.WriteLine("Kérlek, határozd meg a sorsukat az alapértelmezett szövegszerkesztődben!");

            var tmpFilePath = Path.GetTempFileName();

            File.WriteAllLines(tmpFilePath, new List<string>()
            {
                "Kérlek, határozd meg a bejegyzések sorsát az alábbi táblázat segítségével:",
                "(s)kip - feldolgozás során nem lesz figyelembe véve",
                "(r)edmine - redmine-ba könyvelhető",
                "(d)elete - csak törlés",
                "A time-tracking platformok közül többet is meg lehet adni!",
                "PL: ",
                ""
            });


            File.AppendAllLines(tmpFilePath, entries.Select(entry =>
            {
                return $"r - {entry.Description} - {projects[entry.ProjectId].Name}";
            }));

            return tmpFilePath;
        }
    }
}
