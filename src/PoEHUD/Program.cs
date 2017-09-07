using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.HUD;
using PoEHUD.PoE;
using PoEHUD.Tools;

namespace PoEHUD
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, exceptionArgs) =>
            {
                string errorText = "Program exited with message:\n " + exceptionArgs.ExceptionObject;
                File.AppendAllText("Error.log", $"{DateTime.Now:g} {errorText}\r\n{new string('-', 30)}\r\n");
                MessageBox.Show(errorText);
                Environment.Exit(1);
            };

#if !DEBUG
            MemoryControl.Start();
            if (Scrambler.Scramble(args.Length > 0 ? args[0] : null))
            {
                return;
            }
#endif

            Offset offset;
            int pid = FindPoEProcess(out offset);

            if (pid == 0)
            {
                MessageBox.Show("Path of Exile is not running or you started x32 PoE (DirectX9).\nThis version only working with x64 version of PoE.");
                return;
            }

            Sounds.LoadSounds();

            using (var memory = new Memory(offset, pid))
            {
                offset.DoPatternScans(memory);
                var gameController = new GameController(memory);

#if DEBUG
                StringBuilder sb = new StringBuilder();

                sb.Append($"AddressOfProcess: {memory.AddressOfProcess:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"GameController full: {offset.Base + memory.AddressOfProcess:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"GameController: {offset.Base + memory.AddressOfProcess:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"TheGame: {gameController.Game.Address:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"IngameState: {gameController.Game.IngameState.Address:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"IngameData: {gameController.Game.IngameState.Data.Address:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"IngameUI: {gameController.Game.IngameState.IngameUI.Address:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"UIRoot: {gameController.Game.IngameState.UIRoot.Address:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"ServerData: {gameController.Game.IngameState.ServerData.Address:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"GetInventoryZone: {memory.ReadLong(gameController.Game.IngameState.IngameUI.InventoryPanel.Address + Element.OffsetBuffers + 0x42c):X}");
                sb.Append(Environment.NewLine);

                sb.Append($"Area Addr: {gameController.Game.IngameState.Data.CurrentArea.Address:X}");
                sb.Append(Environment.NewLine);

                sb.Append($"Area Name: {gameController.Game.IngameState.Data.CurrentArea.Name}");
                sb.Append(Environment.NewLine);

                sb.Append($"Area change: {memory.ReadLong(offset.AreaChangeCount + memory.AddressOfProcess)}");
                sb.Append(Environment.NewLine);

                sb.Append(memory.DebugString);

                File.WriteAllText("__BaseOffsets.txt", sb.ToString());
#endif

                var overlay = new ExternalOverlay(gameController, memory.IsInvalid);
                Application.Run(overlay);
            }
        }

        private static int FindPoEProcess(out Offset offs)
        {
            List<Tuple<Process, Offset>> clients = Process.GetProcessesByName(Offset.Regular.ExecutableName).Select(p => Tuple.Create(p, Offset.Regular)).ToList();
            clients.AddRange(Process.GetProcessesByName(Offset.Steam.ExecutableName).Select(p => Tuple.Create(p, Offset.Steam)));
            int chosen = clients.Count > 1 ? ChooseSingleProcess(clients) : 0;
            if (clients.Count > 0 && chosen >= 0)
            {
                offs = clients[chosen].Item2;
                return clients[chosen].Item1.Id;
            }

            offs = null;
            return 0;
        }

        private static int ChooseSingleProcess(IReadOnlyList<Tuple<Process, Offset>> clients)
        {
            string o1 = $"Yes - process #{clients[0].Item1.Id}, started at {clients[0].Item1.StartTime.ToLongTimeString()}";
            string o2 = $"No - process #{clients[1].Item1.Id}, started at {clients[1].Item1.StartTime.ToLongTimeString()}";
            const string o3 = "Cancel - quit this application";
            var answer = MessageBox.Show(null, string.Join(Environment.NewLine, o1, o2, o3), "Choose a PoE instance to attach to", MessageBoxButtons.YesNoCancel);
            return answer == DialogResult.Cancel ? -1 : answer == DialogResult.Yes ? 0 : 1;
        }
    }
}
