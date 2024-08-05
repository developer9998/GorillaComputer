using BepInEx;
using BepInEx.Bootstrap;
using GorillaComputer.Model;
using GorillaNetworking;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaComputer.Function
{
    internal class ModsFunction : ComputerFunction
    {
        public override string Name => "Mods";
        public override string Description => "Use 'W' & 'S' to navigate mods - Press 'ENTER' to toggle selected mod";

        private List<PluginInfo> _plugins;

        private int pageCapacity = 10;

        private int pageIndex = 0;

        public ModsFunction()
        {
            var plugins = Chainloader.PluginInfos.Values.Where(pi =>
            {
                var methods = AccessTools.GetMethodNames(pi.Instance);
                return methods.Contains("OnEnable") && methods.Contains("OnDisable");
            });
            _plugins = plugins != null && plugins.Any() ? plugins.ToList() : null;
        }

        public override string GetFunctionContent()
        {
            StringBuilder str = new();

            if (!_plugins.Any())
            {
                str.AppendLine("Please install at least one mod that uses toggleable functionality.");

                return str.ToString();
            }

            var plugins = _plugins.Skip(Mathf.FloorToInt(pageIndex / (float)pageCapacity) * pageCapacity).Take(pageCapacity);

            int selectedPluginIndex = pageIndex % pageCapacity;

            for(int i = 0; i < pageCapacity; i++)
            {
                var plugin = plugins.ElementAtOrDefault(i);

                if (plugin == null) continue;

                bool isSelected = i == selectedPluginIndex;

                str.AppendLine($"{plugin.Metadata.Name}: {(plugin.Instance.enabled ? "<color=lime>E</color>" : "<color=red>D</color>")} {(isSelected ? "<" : "")}");
            }

            return str.ToString();
        }

        public override void OnKeyPressed(GorillaKeyboardBindings key)
        {
            switch (key)
            {
                case GorillaKeyboardBindings.W:
                    if (_plugins.Any() && pageIndex > 0)
                    {
                        pageIndex--;
                        SetFunctionContent();
                    }
                    break;
                case GorillaKeyboardBindings.S:
                    if (_plugins.Any() && pageIndex < _plugins.Count - 1)
                    {
                        pageIndex++;
                        SetFunctionContent();
                    }
                    break;
                case GorillaKeyboardBindings.enter:
                    if (_plugins.Any())
                    {
                        _plugins.ElementAt(pageIndex).Instance.enabled ^= true;
                        SetFunctionContent();
                    }
                    break;
            }
        }
    }
}
