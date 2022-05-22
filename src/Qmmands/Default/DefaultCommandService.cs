using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qommon;
using Qommon.Collections;
using Qommon.Collections.ReadOnly;

namespace Qmmands.Default;

/// <inheritdoc/>
public class DefaultCommandService : ICommandService
{
    public ICommandMapProvider MapProvider { get; }

    /// <inheritdoc/>
    public IServiceProvider Services { get; }

    protected Dictionary<Type, ISet<IModule>> ModuleSets { get; }

    public DefaultCommandService(
        IOptions<DefaultCommandServiceConfiguration> options,
        ILogger<DefaultCommandService> logger,
        ICommandMapProvider mapProvider,
        IServiceProvider services)
    {
        MapProvider = mapProvider;
        Services = services;

        ModuleSets = new();
    }

    public IEnumerable<KeyValuePair<Type, IEnumerable<IModule>>> EnumerateModules()
    {
        return ModuleSets.Select(x => KeyValuePair.Create(x.Key, x.Value.AsEnumerable()));
    }

    public IReadOnlyList<IModule> AddModules(Assembly assembly, Action<IModuleBuilder>? builderAction = null, Predicate<TypeInfo>? predicate = null)
    {
        var modules = new FastList<IModule>();
        var moduleBaseType = typeof(IModuleBase);
        foreach (var type in assembly.GetExportedTypes())
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsNested || typeInfo.IsAbstract || typeInfo.ContainsGenericParameters || !moduleBaseType.IsAssignableFrom(typeInfo))
                continue;

            if (predicate != null && !predicate(typeInfo))
                continue;

            try
            {
                var module = AddModule(typeInfo, builderAction);
                modules.Add(module);
            }
            catch
            {
                for (var i = 0; i < modules.Count; i++)
                    RemoveModule(modules[i]);

                throw;
            }
        }

        return modules.ReadOnly();
    }

    public IModule AddModule(TypeInfo typeInfo, Action<IModuleBuilder>? builderAction = null)
    {
        var reflectorProvider = Services.GetRequiredService<ICommandReflectorProvider>();
        var reflector = reflectorProvider.GetReflector(typeInfo);
        if (reflector == null)
            throw new InvalidOperationException($"No command reflector found for module type {typeInfo}.");

        var module = reflector.CreateModule(typeInfo, builderAction);
        try
        {
            AddModule(module);
        }
        catch
        {
            RemoveModule(module);
            throw;
        }

        return module;
    }

    public virtual void AddModule(IModule module)
    {
        Guard.IsNotNull(module);

        var moduleType = module.GetType();
        var map = MapProvider.GetRequiredMapForModuleType(moduleType);
        if (!ModuleSets.TryGetValue(map.GetType(), out var moduleSet))
            moduleSet = new HashSet<IModule>();

        if (!moduleSet.Add(module))
            throw new ArgumentException("This module has already been added.", nameof(module));

        map.MapModule(module);
    }

    public virtual bool RemoveModule(IModule module)
    {
        Guard.IsNotNull(module);

        var moduleType = module.GetType();
        var map = MapProvider.GetRequiredMapForModuleType(moduleType);

        if (!ModuleSets.TryGetValue(map.GetType(), out var moduleSet))
            return false;

        if (!moduleSet.Remove(module))
            return false;

        map.UnmapModule(module);
        return true;
    }

    public virtual void ClearModules(Type? mapType = null)
    {
        if (mapType != null)
        {
            var map = MapProvider.GetMap(mapType);
            Guard.IsNotNull(map);

            if (ModuleSets.Remove(map.GetType(), out var moduleSet))
            {
                foreach (var module in moduleSet)
                {
                    map.UnmapModule(module);
                }
            }

            return;
        }

        foreach (var kvp in ModuleSets)
        {
            mapType = kvp.Key;
            var map = MapProvider.GetMap(mapType);
            Guard.IsNotNull(map);

            foreach (var module in kvp.Value)
            {
                map.UnmapModule(module);
            }
        }

        ModuleSets.Clear();
    }

    public virtual async ValueTask<IResult> ExecuteAsync(ICommandContext context)
    {
        ICommandPipeline? pipeline;
        try
        {
            pipeline = Services.GetRequiredService<ICommandPipelineProvider>().GetPipeline(context);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An exception occurred while attempting to get the pipeline for {context.GetType()}.", ex);
        }

        if (pipeline == null)
            throw new InvalidOperationException($"No suitable execution pipeline exists for {context.GetType()}.");

        try
        {
            return await pipeline.ExecuteAsync(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An exception occurred while executing the command pipeline {pipeline.GetType()}", ex);
        }
    }
}
