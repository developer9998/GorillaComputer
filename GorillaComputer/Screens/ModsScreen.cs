using BepInEx;
using BepInEx.Bootstrap;
using GorillaComputer.Behaviours;
using GorillaComputer.Models;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Screens
{
    internal class ModsScreen : ComputerScreen
    {
        public override string Title => "Mods";
        public override string Summary => "Use [W/S] to navigate mods\nPress [ENTER] to toggle mod";

        private List<PluginInfo> _plugins;

        private const int pageCapacity = 10;

        private int pageIndex = 0;

        public void Awake()
        {
            var plugins = Chainloader.PluginInfos.Values.Where(pluginInfo =>
            {
                var methods = AccessTools.GetMethodNames(pluginInfo.Instance);
                return methods.Contains("OnEnable") && methods.Contains("OnDisable");
            });

            _plugins = plugins != null && plugins.Any() ? plugins.ToList() : null;
        }

        public override string GetContent()
        {
            StringBuilder str = new();

            if (!_plugins.Any())
            {
                str.AppendLine("Please install at least one mod that uses toggleable functionality.");

                return str.ToString();
            }

            var plugins = _plugins.Skip(Mathf.FloorToInt(pageIndex / (float)pageCapacity) * pageCapacity).Take(pageCapacity);

            int selectedPluginIndex = pageIndex % pageCapacity;

            for (int i = 0; i < pageCapacity; i++)
            {
                var plugin = plugins.ElementAtOrDefault(i);

                if (plugin == null) continue;

                bool isSelected = i == selectedPluginIndex;

                str.AppendLine($"{plugin.Metadata.Name}: {(plugin.Instance.enabled ? "<color=lime>E</color>" : "<color=red>D</color>")} {(isSelected ? "<" : "")}");
            }

            return str.ToString();
        }

        public override void ProcessScreen(KeyBinding key)
        {
            switch (key)
            {
                case KeyBinding.W:
                    if (_plugins.Any() && pageIndex > 0)
                    {
                        pageIndex--;
                        UpdateScreen();
                    }

                    break;
                case KeyBinding.S:
                    if (_plugins.Any() && pageIndex < _plugins.Count - 1)
                    {
                        pageIndex++;
                        UpdateScreen();
                    }

                    break;
                case KeyBinding.enter:
                    if (_plugins.Any())
                    {
                        _plugins.ElementAt(pageIndex).Instance.enabled ^= true;
                        UpdateScreen();
                    }

                    break;
            }
        }
    }
}
