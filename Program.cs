using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace TotalUninstaller
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{

				var command = Args.Configuration.Configure<CommandObject>().CreateAndBind(args);

				// Get the installed MSI Products
				IEnumerable<ProductInstallation> installations = ProductInstallation.GetProducts(null, "s-1-1-0", UserContexts.All);


				if (!command.ListAll && !command.Filter && !command.Uninstall)
				{
					printCommands();
				}

				if (command.ListAll)
				{
					// Loop through the installed MSI Products and output information
					foreach (ProductInstallation installation in installations)
					{
						Console.WriteLine("Name: " + installation.ProductName);
						Console.WriteLine("Product Code: " + installation.ProductCode);
						Console.WriteLine();
					}
				}
				else if (command.Filter)
				{
					IEnumerable<string> productNameSubstrings = GetListOfProductNameSubstringsToUninstallFromConfig();
					foreach (ProductInstallation installation in installations)
					{
						if (string.IsNullOrEmpty(installation.ProductName)) continue;
						if (productNameSubstrings.Any(installation.ProductName.Contains))
						{
							Console.WriteLine("Name: " + installation.ProductName);
							Console.WriteLine("Product Code: " + installation.ProductCode);
							Console.WriteLine();
						}
					}
				}
				else if (command.Uninstall)
				{
					IEnumerable<string> productNameSubstrings = GetListOfProductNameSubstringsToUninstallFromConfig();
					List<ProductInstallation> productsToUninstall = new List<ProductInstallation>();
					foreach (ProductInstallation installation in installations)
					{
						if (string.IsNullOrEmpty(installation.ProductName)) continue;
						if (productNameSubstrings.Any(installation.ProductName.Contains))
						{
							productsToUninstall.Add(installation);
							Console.WriteLine("Name: " + installation.ProductName);
							Console.WriteLine("Product Code: " + installation.ProductCode);
							Console.WriteLine();
						}
					}

					foreach (ProductInstallation productInstallation in productsToUninstall)
					{
						Console.WriteLine("Uninstalling " + productInstallation.ProductName);
						Console.WriteLine();
						Uninstall(productInstallation.ProductCode);
					}

				}
			}
			catch (Exception)
			{
				printCommands();
			}

		}

		private static void printCommands()
		{
			Console.WriteLine("USAGE:");
			Console.WriteLine("\t/ListAll - show all installed products");
			Console.WriteLine("\t/Filter  - show all products matching list of filter strings in configuration file");
			Console.WriteLine("\t/Uninstall  - UNINSTALL all products matching list of filter strings in configuration file");
			return;

		}


		private static IEnumerable<string> GetListOfProductNameSubstringsToUninstallFromConfig()
		{
			Hashtable section = (Hashtable)ConfigurationManager.GetSection("ProductsToUninstall");
			return section.Values.Cast<string>();
		}
		private static void Uninstall(string productCode)
		{
			Installer.SetInternalUI(InstallUIOptions.ProgressOnly | InstallUIOptions.SourceResolutionOnly | InstallUIOptions.UacOnly);
			Installer.ConfigureProduct(productCode, 0, InstallState.Absent, "IGNOREDEPENDENCIES=\"ALL\"");
		}
	}

	public class CommandObject
	{
		public bool ListAll;
		public bool Filter;
		public bool Uninstall;
	}
}

/*"REBOOT=\"R\" /l*v uninstall.log"*/