﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shiny.Infrastructure;

namespace Shiny
{
    public static class ShinyHost
    {
        public static IHostBuilder CreateDefaultBuilder()
        {
            var builder = new ShinyHostBuilder()
                .AddShiny();

            return builder;
        }


        static IServiceProvider? services;
        public static IServiceProvider ServiceProvider
        {
            get
            {
                if (services == null)
                    throw new ArgumentException("ServiceProvider has not been setup - have you initialized the Platform provider using ShinyHost.Init?");

                return services;
            }
            internal set => services = value;
        }


        public static ILoggerFactory LoggerFactory
            => ServiceProvider.Resolve<ILoggerFactory>();


        /// <summary>
        /// Resolve a specified service from the container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>() => ServiceProvider.Resolve<T>();


        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Lazy<T> LazyResolve<T>() => new Lazy<T>(() => ServiceProvider.Resolve<T>());


        /// <summary>
        /// Resolve a list of registered services from the container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> ResolveAll<T>()
            => ServiceProvider.GetServices<T>() ?? Enumerable.Empty<T>();

        public static void Init(IPlatform platform, IShinyStartup? startup = null)
        {
            var services = new ServiceCollection();
            services.AddSingleton(platform);
            services.AddLogging(builder => startup?.ConfigureLogging(builder, platform));
            startup?.ConfigureServices(services, platform);

            ServiceProvider = startup?.CreateServiceProvider(services) ?? services.BuildServiceProvider();
            ServiceProvider.GetRequiredService<StartupModule>().Start();
        }
    }
}