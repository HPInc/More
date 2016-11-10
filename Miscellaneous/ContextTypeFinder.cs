// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Reflection;

#if !WindowsCE
namespace More
{
    /// <summary>
    /// Used to find types from fully qualified class names (means it includes the namespace).
    /// It caches types that have already been found, and also first checks assemblies that other types
    /// have already been found in.
    /// </summary>
    public class ContextTypeFinder
    {
        readonly List<Assembly> assembliesInContext;
        readonly Dictionary<String, Type> typesFound;

        public ContextTypeFinder()
        {
            this.assembliesInContext = new List<Assembly>();
            this.typesFound = new Dictionary<String, Type>();
        }
        public Type FindType(String fullTypeName)
        {
            //Console.WriteLine("------ Looking for type '{0}'", fullTypeName);
            Type typeThatWasAlreadyFound;
            if (typesFound.TryGetValue(fullTypeName, out typeThatWasAlreadyFound))
                return typeThatWasAlreadyFound;

            //
            // Check assemblies in context first
            //
            //Console.WriteLine("------ Checking assemblies in context...");
            for (int i = 0; i < assembliesInContext.Count; i++)
            {
                Assembly assembly = assembliesInContext[i];
                Type[] types = assembly.GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];
                    //Console.WriteLine("Checking type '{0}'", type);
                    if (type.FullName.Equals(fullTypeName))
                    {
                        typesFound[fullTypeName] = type;
                        return type;
                    }
                }
            }

            //
            // Check all loaded assemblies
            //
            //Console.WriteLine("------ Checking loaded assemblies in context...");
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < loadedAssemblies.Length; i++)
            {
                Assembly assembly = loadedAssemblies[i];
                
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];
                    //Console.WriteLine("Checking type '{0}'", type);
                    if (type.FullName.Equals(fullTypeName))
                    {
                        if (!assembliesInContext.Contains(assembly))
                        {
                            assembliesInContext.Add(assembly);
                        }
                        typesFound[fullTypeName] = type;
                        return type;
                    }
                }
            }

            //
            // Check all referenced assemblies
            //
            HashSet<String> assembliesAlreadyChecked = new HashSet<String>();
            for (int i = 0; i < loadedAssemblies.Length; i++)
            {
                assembliesAlreadyChecked.Add(loadedAssemblies[i].FullName);
            }

            //Console.WriteLine("------ Checking referenced assemblies in context...");
            for (int i = 0; i < loadedAssemblies.Length; i++)
            {
                Type type = CheckReferencedAssemblies(assembliesAlreadyChecked, loadedAssemblies[i], fullTypeName);
                if (type != null) return type;
            }

            throw new InvalidOperationException(String.Format("Could not find type '{0}'", fullTypeName));
        }

        Type CheckReferencedAssemblies(HashSet<String> alreadyChecked, Assembly assembly, String fullTypeName)
        {
            AssemblyName[] referencedAssemblyNames = assembly.GetReferencedAssemblies();

            if (referencedAssemblyNames != null && referencedAssemblyNames.Length > 0)
            {
                for (int i = 0; i < referencedAssemblyNames.Length; i++)
                {
                    AssemblyName referencedAssemblyName = referencedAssemblyNames[i];

                    if (!alreadyChecked.Contains(referencedAssemblyName.FullName))
                    {
                        alreadyChecked.Add(referencedAssemblyName.FullName);
                        Assembly referencedAssembly = Assembly.Load(referencedAssemblyName);

                        Type[] types = referencedAssembly.GetTypes();
                        for (int j = 0; j < types.Length; j++)
                        {
                            Type type = types[j];
                            //Console.WriteLine("Checking type '{0}'", type.Name);
                            if (type.FullName.Equals(fullTypeName))
                            {
                                if (!assembliesInContext.Contains(referencedAssembly))
                                {
                                    assembliesInContext.Add(referencedAssembly);
                                }
                                typesFound[fullTypeName] = type;
                                return type;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
#endif