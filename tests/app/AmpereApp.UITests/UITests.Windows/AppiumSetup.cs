using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace UITests;

public class AppiumSetup : IDisposable
{
	private static AppiumDriver? driver;

	public static AppiumDriver App => driver ?? throw new NullReferenceException("AppiumDriver is null");

	public void RunBeforeAnyTests()
	{
		var windowsOptions = new AppiumOptions
		{
			// Specify windows as the driver, typically don't need to change this
			AutomationName = "windows",
			// Always Windows for Windows
			PlatformName = "Windows",
			// The identifier of the deployed application to test
			App = "com.companyname.applicationname_9zz4h110yvjzm!App",
		};

		// Note there are many more options that you can use to influence the app under test according to your needs

		driver = new WindowsDriver(windowsOptions);
	}

	public void Dispose()
	{
		driver?.Quit();
	}
}
