using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Mac;

namespace UITests;

public class AppiumSetup : IDisposable
{
	private static AppiumDriver? driver;

	public static AppiumDriver App => driver ?? throw new NullReferenceException("AppiumDriver is null");

	public AppiumSetup()
	{
		var macOptions = new AppiumOptions
		{
			// Specify mac2 as the driver, typically don't need to change this
			AutomationName = "mac2",
			// Always Mac for Mac
			PlatformName = "Mac",
			// The full path to the .app file to test
			App = "/path/to/MauiApp/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/BasicAppiumSample.app",
		};

		// Setting the Bundle ID is required, else the automation will run on Finder
		macOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, "com.companyname.applicationname");

		// Note there are many more options that you can use to influence the app under test according to your needs

		driver = new MacDriver(macOptions);
	}

	public void Dispose()
	{
		driver?.Quit();
	}
}