This folder is for project-specific extensions, which are unfortunately hard-coded into inklecate at the moment.

Eventually, we could potentially have some kind of basic plugin architecture that loads up DLLs rather than having them hard-baked into inklecate, and the PluginManager would be reponsible for loading up the assemblies based on DLL filenames.

The main problems I found were:

 * The access levels for all the classes are mostly "internal", meaning that the object hierarchies wouldn't be accessible to any external plugins. So we'd have to audit that. It seems a shame since I was hoping that the runtime API could be kept quite small.
 * I've been hitting a bug in Xamarin where internal assembly references aren't working correctly

Code looks fairly straightforward though:

https://code.msdn.microsoft.com/windowsdesktop/Creating-a-simple-plugin-b6174b62

