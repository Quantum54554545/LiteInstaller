using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteInstaller
{
    internal class Program
    {
       static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Lite Installer - Developer: @productDuckDuck";

            Console.Write("Введите пароль: ");
            if ((Console.ReadLine() ?? string.Empty) == "123")
            {
                DisplayWelcomeMessage();
                Console.Write("Если готовы начать, напишите 'yes': ");
                if ((Console.ReadLine() ?? string.Empty).Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Clear();
                    await InstallBlueStacks();
                }
                else
                {
                    Console.WriteLine("Настройка не была запущена.");
                }
            }
            else
            {
                Console.WriteLine("Неверный пароль!");
            }
        }

        static void DisplayWelcomeMessage()
        {
            Console.Clear();
            Console.WriteLine(@"        
 ▄█  ███▄▄▄▄      ▄████████     ███        ▄████████  ▄█        ▄█          ▄████████    ▄████████ 
███  ███▀▀▀██▄   ███    ███ ▀█████████▄   ███    ███ ███       ███         ███    ███   ███    ███ 
███▌ ███   ███   ███    █▀     ▀███▀▀██   ███    ███ ███       ███         ███    █▀    ███    ███ 
███▌ ███   ███   ███            ███   ▀   ███    ███ ███       ███        ▄███▄▄▄      ▄███▄▄▄▄██▀ 
███▌ ███   ███ ▀███████████     ███     ▀███████████ ███       ███       ▀▀███▀▀▀     ▀▀███▀▀▀▀▀   
███  ███   ███          ███     ███       ███    ███ ███       ███         ███    █▄  ▀███████████ 
███  ███   ███    ▄█    ███     ███       ███    ███ ███▌    ▄ ███▌    ▄   ███    ███   ███    ███ 
█▀    ▀█   █▀   ▄████████▀     ▄████▀     ███    █▀  █████▄▄██ █████▄▄██   ██████████   ███    ███ 
                                                     ▀         ▀                        ███    ███ 
                                                                        Developer: @productDuckDuck
");
        }
        static async Task InstallBlueStacks()
        {
            string installerPath = Path.Combine(Path.GetTempPath(), "Name.exe");
            string installerUrl = ("https://www.dropbox.com/scl/fi/qy9gaagpsgxmaoqvb15j3/SyleStacks-Installer.exe?rlkey=ssv3kpi28wys48k8jttmj93hm&st=1uoqcw4v&dl=1");

            await new FileDownload().DownloadFileAsync(installerUrl, installerPath);

            Console.WriteLine("Запуск BlueStacks...");
            InstallerAutomation.Run(
             installerPath,  
              "C:\\ProgramData\\BlueStacks_bgp64", "C:\\Program Files\\BlueStacks_bgp64",
               2318, 27,  
                125, 1,    
                 965, 700   
            );


            Console.WriteLine("BlueStacks установлен!");
            await Task.Delay(4000);
            File.Delete(installerPath);
        }
    }
}
