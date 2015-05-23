[![Build status](https://ci.appveyor.com/api/projects/status/ej34y5wtjfxr8w7m/branch/master?svg=true)](https://ci.appveyor.com/project/Workshop2/iisconfiguration/branch/master)
[![Version](https://img.shields.io/nuget/v/IisConfiguration.svg?style=flat)](https://www.nuget.org/packages/IisConfiguration)

# IisConfiguration
IisConfiguration is a .NET library for automating site and app pool creation in IIS. It adds a fluent interface on top of the existing Microsoft.Web.Administration namespace to simplify the creation of things like:

- Adding sites with bindings and SSL certificates
- Adding app pools
- Automatic tear up and tear down of sites (which is done when you add a site).

The library is intended to be used inside a console application, which you can then call as part of a "setup.ps1" file that lives in the root of your web application.

## Example usage

This example creates two sites: a web site, and a service site (for your website to call a web api or WCF service). It also adds an SSL binding for the website, using the SSL certificate path and private key password you provide in the app.config file.

    using System;
    using System.IO;
    using IisConfiguration;
    using Microsoft.Web.Administration;
    
    namespace MyApp.IisConfigTool
    {
    	class Program
        {
            private const string SiteName = "MyApp";
            private const string ServiceName = "MyAppService";
    
    		static void Main(string[] args)
    		{
    			var logger = new ConsoleLogger();
    			var serverConfig = new WebServerConfig(logger);
    
    			if (!serverConfig.IsIis7OrAbove)
    			{
    				logger.LogHeading("IIS7 is not installed on this machine. IIS configuration setup terminated.");
    				return;
    			}
    
    			var envConfig = new EnvironmentalConfig();
    
    			try
    			{
                    SetupWeb(serverConfig, envConfig);
                    SetupService(serverConfig, envConfig);
    			}
    			catch (Exception e)
    			{
    				logger.LogError(e);
    				throw;
    			}
    		}
    
            private static void SetupWeb(WebServerConfig serverConfig, EnvironmentalConfig envConfig)
    	    {
                const int webPort = 8001;
                const int webPortSsl = 4001;
    
    	        serverConfig
                    .AddAppPool(SiteName, "v4.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalService)
    	            .WithProcessModel(envConfig.IdleTimeout, envConfig.PingingEnabled)
    	            .Commit();
    
    	        serverConfig
                    .AddSite(SiteName, webPort, webPort)
                    .WithSecurePort(webPortSsl, envConfig.SslPfxPath, envConfig.SslPassword)
                    .AddApplication("/", envConfig.WebRoot, SiteName)
    	            .WithLogging(false)
    	            .Commit();
    	    }
    
            private static void SetupService(WebServerConfig serverConfig, EnvironmentalConfig envConfig)
            {
                const int servicePort = 8110;
    
                serverConfig
                    .AddAppPool(ServiceName, "v4.0", ManagedPipelineMode.Integrated, envConfig.AppPoolIdentityType)
                    .WithCredentials(envConfig.AppPoolUser, envConfig.AppPoolPassword)
                    .WithProcessModel(envConfig.IdleTimeout, envConfig.PingingEnabled)
                    .Commit();
    
                serverConfig
                    .AddSite(ServiceName, servicePort, servicePort)
                    .AddApplication("/", envConfig.ServiceRoot, ServiceName)
                    .Commit();
            }
    	}
    }


IISConfiguration also expects an app.config file, to read its settings from. The keys it expects are mapped directly to the property names you find in the `EnvironmentalConfig` class. Here is a basic example:

    <?xml version="1.0"?>
    <configuration>
        <appSettings>
            <add key="WebRoot" value="C:\Code\MySite" />
            <add key="ServiceRoot" value="C:\Code\MyService" />
    
            <add key="PingingEnabled" value="false"/>
            <add key="IdleTimeout" value="00:20:00"/>
        </appSettings>
    </configuration>

