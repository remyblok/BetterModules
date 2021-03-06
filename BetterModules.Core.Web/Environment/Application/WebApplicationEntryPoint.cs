﻿using System;

using BetterModules.Core.Dependencies;
using BetterModules.Core.Exceptions;
using BetterModules.Core.Web.Dependencies;
using BetterModules.Core.Web.Environment.Application;

using Common.Logging;

[assembly: WebApplicationPreStartAttribute(typeof(WebApplicationEntryPoint), "PreApplicationStart", Order = 100)]
[assembly: WebApplicationPreStartAttribute(typeof(WebApplicationEntryPoint), "PreWebApplicationStart", Order = 200)]

namespace BetterModules.Core.Web.Environment.Application
{
    /// <summary>
    /// Entry point to run web application preload  logic. 
    /// </summary>
    public class WebApplicationEntryPoint
    {
        private static bool isApplicationStarted;
        private static bool isWebApplicationStarted;

        /// <summary>
        /// Method to run logic before application start (as PreApplicationStartMethod). Do not run this method from your code.
        /// </summary>        
        public static void PreApplicationStart()
        {
            if (isApplicationStarted)
            {
                return;
            }

            ILog logger;

            try
            {
                logger = LogManager.GetCurrentClassLogger();
                logger.Info("Starting Web Application...");
            }
            catch (Exception ex)
            {
                throw new CoreException("Logging is not working. A reason may be that Common.Logging section is not configured in web.config.", ex);
            } 

            try
            {
                logger.Info("Creating Web Application context dependencies container...");
                ContextScopeProvider.RegisterTypes(WebApplicationContext.InitializeContainer());
            }
            catch (Exception ex)
            {
                string message = "Failed to create Web Application context dependencies container.";
                logger.Fatal(message, ex);

                throw new CoreException(message, ex);
            }

            PreStartWebApplication();

            isApplicationStarted = true;
        }

        public static void PreStartWebApplication()
        {
            if (isWebApplicationStarted)
            {
                return;
            }

            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                logger.Info("Registering per web request lifetime manager module...");
                PerWebRequestLifetimeModule.DynamicModuleRegistration();
            }
            catch (Exception ex)
            {
                string message = "Failed to register per web request lifetime manager module.";
                logger.Fatal(message, ex);

                throw new CoreException(message, ex);
            }

            try
            {
                logger.Info("Load assemblies...");
                WebApplicationContext.LoadAssemblies();
            }
            catch (Exception ex)
            {
                string message = "Failed to load assemblies.";
                logger.Fatal(message, ex);

                throw new CoreException(message, ex);
            }

            try
            {
                logger.Info("Migrating database...");
                ApplicationContext.RunDatabaseMigrations();
            }
            catch (Exception ex)
            {
                string message = "Failed to run database migrations.";
                logger.Fatal(message, ex);

                throw new CoreException(message, ex);
            }

            isWebApplicationStarted = true;
        }
    }
}
