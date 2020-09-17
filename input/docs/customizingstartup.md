Title: Source Generation Demystify
Order: 3
---

## Info

Shiny generates all of the necessary boilerplate on Android and iOS.  It will even generate your entire Startup class if you configure it to do so.  However, there are times where source generation may break a level of customization you need for your application.  This article contains info on how to work with or work around the source generator.

## 3rd Party Support
A few cases where Shiny source generators work on behalf of other libraries, so you don't need to customize.  Shiny will wire-in the following 3rd party libraries:

* **Xamarin Forms** - Adds Xamarin.Forms.Forms.Init to your Android main activity and to the iOS AppDelegate
* **Xamarin Essentials** - Adds Xamarin.Essentials.Platform.Init to the Android application class
* **ACR User Dialogs** - Add UserDialogs.Init to the Android application class

## Going Further
There are occasions however, when you need to customize things further, as such, the Shiny source generator will do the following

* The Shiny generator will not create any methods if you already have an existing one in place, however, you are responsible for wiring in Shiny when you do this.  Each section below explains what you need to add to wire-in Shiny
* Shiny will look for special  "Shiny" methods and generate calls to them from the method it is "hijacking".  Example, if you allow an Android Activity to have OnCreate generated, the Shiny source generator will look for ShinyOnCreate and pass the same arguments.  

## iOS AppDelegate

The following methods will be generated by Shiny.  All 

|Method|Shiny Wire-In|Purpose|
|------|-------------|-------|
ReceivedRemoteNotification|ShinyDidReceiveRemoteNotification|Used by Shiny.Push and some app services
DidReceiveRemoteNotification|ShinyDidReceiveRemoteNotification|Used by Shiny.Push and some app services
RegisteredForRemoteNotifications|ShinyRegisteredForRemoteNotifications|Used by Shiny.Push and some app services
FailedToRegisterForRemoteNotifications|ShinyFailedToRegisterForRemoteNotifications|Used by Shiny.Push and some app services
PerformFetch|ShinyPerformFetch|Used by Shiny Core mostly for jobs, but there are lots of places in Shiny where Jobs are used
HandleEventsForBackgroundUrl|ShinyHandleEventsForBackgroundUrl|Used by Shiny.Net.Http for background transfers

Example of what Shiny Source Generator creates:
```cs
using System;
using Shiny;
using Foundation;
using UIKit;


namespace YourNamespace
{
	public partial class MyAppDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			this.OnFinishedLaunching(app, options);
			this.ShinyFinishedLaunching(new YourDetectedAndGeneratedStartup());
			global::Xamarin.Forms.Forms.Init();
			return base.FinishedLaunching(app, options);
		}
		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler) => this.ShinyDidReceiveRemoteNotification(userInfo, completionHandler);
		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken) => this.ShinyRegisteredForRemoteNotifications(deviceToken);
		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error) => this.ShinyFailedToRegisterForRemoteNotifications(error);
		public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler) => this.ShinyPerformFetch(completionHandler);
		public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler) => this.ShinyHandleEventsForBackgroundUrl(sessionIdentifier, completionHandler);
	}
}
```

## Android Application
The Android application is generated in full.  Shiny also auto-wires in Xamarin Essentials and ACR User Dialogs.  If you want source gen to happen on your custom application class, you will also need to make sure that you class is marked as partial.

|Method|Shiny Wire-In|Purpose|
OnCreate|Shiny.AndroidShinyHost.Init|This spins up all of the Shiny infrastructure on Android
OnTrimMemory|Shiny.AndroidShinyHost.OnBackground|This runs appstate which is used internally by jobs

Here is an example of what Shiny generates
```cs
using System;
using Shiny;
using Android.App;
using Android.Content;
using Android.Runtime;


namespace YourNamespace
{
	[ApplicationAttribute]
	public partial class AppShinyApplication : Application
	{
		public AppShinyApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) {}
		public override void OnCreate()
		{
			AndroidShinyHost.Init(this, new YourDetectedAndGeneratedStartup());
			base.OnCreate();
		}
		public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
		{
			AndroidShinyHost.OnBackground(level);
			base.OnTrimMemory(level);
		}
	}
}
```

## Android Activity
As with the Android application, your activity classes must be marked as partial if you want Shiny to code-gen its wire-ins.

|Method|Shiny Wire-In|Purpose|
|------|-------------|-------|
OnCreate|ShinyOnCreate|Used by Shiny.Notifications for running delegates on activity entry
OnNewIntent|ShinyOnNewIntent|Used by Shiny.Notifications for running delegates on activity entry
OnRequestPermissionsResult|ShinyOnRequestPermissionsResult|This is for permission requests that are made by pretty much all of the Shiny modules

Here is an example of what Shiny generates:
```cs
using System;
using Shiny;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;


namespace YourNamespace
{
	public partial class MainActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			this.ShinyOnCreate();
		}
		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);
			this.ShinyOnNewIntent(intent);
		}
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			this.ShinyOnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}
```

## Shiny Startup
When you add [assembly:Shiny.GenerateStartupClass] in your assembly, the Shiny source generator will scan the project for all references to all Shiny services as well as scan all of your code for all of you created delegates.

An example of what Shiny generates:
```cs
using System;
using Shiny;
using Microsoft.Extensions.DependencyInjection;


namespace YourNamespace
{
	public partial class AppShinyStartup : Shiny.ShinyStartup
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			services.UseBleClient();
		}
		public override void ConfigureApp(IServiceProvider provider)
		{
			global::Xamarin.Forms.Internals.DependencyResolver.ResolveUsing(t => provider.GetService(t));
		}
	}
}
```