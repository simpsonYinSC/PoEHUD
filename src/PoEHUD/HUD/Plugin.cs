using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PoEHUD.Controllers;
using PoEHUD.HUD.Interfaces;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.UI;
using PoEHUD.Models;
using SharpDX;

namespace PoEHUD.HUD
{
    public abstract class Plugin<TSettings> : IPlugin where TSettings : SettingsBase
    {
        protected readonly GameController GameController;

        protected readonly Graphics Graphics;

        protected Plugin(GameController gameController, Graphics graphics, TSettings settings)
        {
            GameController = gameController;
            Graphics = graphics;
            Settings = settings;
            gameController.EntityListWrapper.EntityAdded += OnEntityAdded;
            gameController.EntityListWrapper.EntityRemoved += OnEntityRemoved;
        }

        protected TSettings Settings { get; private set; }

        public virtual void Dispose()
        {
        }

        public abstract void Render();

        protected static RectangleF GetDirectionsUV(double phi, double distance)
        {
            // could not find a better place yet
            phi += Math.PI * 0.25; // FIX: rotation due to projection
            if (phi > 2 * Math.PI)
            {
                phi -= 2 * Math.PI;
            }

            var xSprite = (float)Math.Round(phi / Math.PI * 4);
            if (xSprite >= 8)
            {
                xSprite = 0;
            }

            float ySprite = distance > 60 ? distance > 120 ? 2 : 1 : 0;
            float x = xSprite / 8;
            float y = ySprite / 3;
            return new RectangleF(x, y, (xSprite + 1) / 8 - x, (ySprite + 1) / 3 - y);
        }

        protected static IEnumerable<string[]> LoadConfigBase(string path, int columnsCount = 2)
        {
            return File.ReadAllLines(path)
                .Where(line => !string.IsNullOrWhiteSpace(line) && line.IndexOf(';') >= 0 && !line.StartsWith("#"))
                .Select(line => line.Split(new[] { ';' }, columnsCount).Select(parts => parts.Trim()).ToArray());
        }

        /// <summary>
        /// Loads a Comma separated file into a list of Strings
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected static Dictionary<string, List<string>> LoadConfigList(string path)
        {
            var result = new Dictionary<string, List<string>>();

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines.Select(a => a.Trim()))
            {
                // Ignore empty lines, those without , and comments
                if (string.IsNullOrWhiteSpace(line) || line.IndexOf(',') < 0 || line.StartsWith("#"))
                {
                    continue;
                }

                List<string> values = line.Split(',').Select(s => s.Trim()).ToList(); // Split comma separated Values into the List of strings
                string name = values[0];  // Key Value for the Dictionary
                values.RemoveAt(0); // remove the key-Value from the List
                result.Add(name, values);
            }

            return result;
        }

        protected virtual void OnEntityAdded(EntityWrapper entityWrapper)
        {
        }

        protected virtual void OnEntityRemoved(EntityWrapper entityWrapper)
        {
        }
    }
}
