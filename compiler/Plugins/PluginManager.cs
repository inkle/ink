using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Ink
{
    public class PluginManager
    {
        public PluginManager (List<string> pluginDirectories)
        {
            _plugins = new List<IPlugin> ();

            foreach (string pluginName in pluginDirectories) 
            {
                foreach (string file in Directory.GetFiles(pluginName, "*.dll"))
                {
                    foreach (Type type in Assembly.LoadFile(Path.GetFullPath(file)).GetExportedTypes())
                    {
                        if (typeof(IPlugin).IsAssignableFrom(type))
                        {
                            _plugins.Add((IPlugin)Activator.CreateInstance(type));
                        }
                    }
                }
            }
        }

		public string PreParse(string storyContent)
		{
			object[] args = new object[] { storyContent };

            foreach (var plugin in _plugins) 
            {
                typeof(IPlugin).InvokeMember("PreParse", BindingFlags.InvokeMethod, null, plugin, args);
            }

			return (string)args[0];
		}

        public Parsed.Story PostParse(Parsed.Story parsedStory)
        {
            object[] args = new object[] { parsedStory };

            foreach (var plugin in _plugins) 
            {
                typeof(IPlugin).InvokeMember("PostParse", BindingFlags.InvokeMethod, null, plugin, args);
            }

			return (Parsed.Story)args[0];
        }

        public Runtime.Story PostExport(Parsed.Story parsedStory, Runtime.Story runtimeStory)
        {
            object[] args = new object[] { parsedStory, runtimeStory };

            foreach (var plugin in _plugins) 
            {
                typeof(IPlugin).InvokeMember("PostExport", BindingFlags.InvokeMethod, null, plugin, args);
            }

			return (Runtime.Story)args[1];
        }

        List<IPlugin> _plugins;
    }
}

