using System;
using System.Collections.Generic;

namespace Ink
{
    public class PluginManager
    {
        public PluginManager (List<string> pluginNames)
        {
            _plugins = new List<IPlugin> ();

            // TODO: Make these plugin names DLL filenames, and load up their assemblies
            foreach (string pluginName in pluginNames) {
                //if (pluginName == "ChoiceListPlugin") {
                //    _plugins.Add (new InkPlugin.ChoiceListPlugin ());
                //}else  
                {
                    throw new System.Exception ("Plugin not found");
                }
            }
        }

        public void PostParse(Parsed.Fiction parsedFiction)
        {
            foreach (var plugin in _plugins) {
                plugin.PostParse (parsedFiction);
            }
        }

        public void PostExport(Parsed.Fiction parsedFiction, Runtime.Story runtimeStory)
        {
            foreach (var plugin in _plugins) {
                plugin.PostExport (parsedFiction, runtimeStory);
            }
        }

        List<IPlugin> _plugins;
    }
}

