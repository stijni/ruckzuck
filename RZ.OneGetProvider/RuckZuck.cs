﻿using System;
using System.Collections.Generic;
using System.Linq;
using PackageManagement.Sdk;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Security;
using RuckZuck.Base;
using Microsoft.Win32;

namespace PackageManagement
{
    partial class PackageProvider
    {
        // RuckZuck Code Here

        //private static string _AuthenticationToken = "";
        private RZScan oScan;
        private RZUpdate.RZUpdater oUpdate;

        public List<AddSoftware> lSoftware = new List<AddSoftware>();
        //public static string WebServiceURL = "https://ruckzuck.azurewebsites.net/wcf/RZService.svc";

        //private DateTime dLastTokenRefresh = new DateTime();

        /// <summary>
        /// Initialize the RuckZuck Web-Service
        /// </summary>
        /// <param name="request"></param>
        private void _initRZ(Request request)
        {
            //try
            //{
            //    Properties.Settings.Default.Location = "";
            //    Properties.Settings.Default.Save();

            //    if (Properties.Settings.Default.Location.StartsWith("https:"))
            //    {
            //        RZRestAPIv2.sURL = Properties.Settings.Default.Location;
            //    }
            //    else
            //    {
            //        Properties.Settings.Default.Location = RZRestAPIv2.sURL;
            //        Properties.Settings.Default.Save();
            //    }
            //}
            //catch { }

            RZRestAPIv2.sURL.ToString(); //get REST API URL

            request.Debug("RZ Endpoint: " + RZRestAPIv2.sURL);

            oScan = new RZScan(false, false);
            oUpdate = new RZUpdate.RZUpdater();
        }

        private string _providerVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); // "1.0.0.0";
            }
        }

        private void _getDynamicOptions(string category, Request request)
        {
            switch ((category ?? string.Empty).ToLowerInvariant())
            {
                case "install":
                    // todo: put any options required for install/uninstall/getinstalledpackages
                    request.YieldDynamicOption("SkipDependencies", Constants.OptionType.Switch, false);
                    request.YieldDynamicOption("LocalPath", Constants.OptionType.Folder, false);

                    request.YieldDynamicOption("Manufacturer", Constants.OptionType.String, false);
                    request.YieldDynamicOption("ProductVersion", Constants.OptionType.String, false);
                    request.YieldDynamicOption("ProductName", Constants.OptionType.String, false);

                    break;

                case "provider":
                    //request.YieldDynamicOption("Username", Constants.OptionType.String, false);
                    //request.YieldDynamicOption("Password", Constants.OptionType.String, false);
                    //request.YieldDynamicOption("EndPointURL", Constants.OptionType.String, false);
                    request.YieldDynamicOption("CustomerID", Constants.OptionType.String, false);
                    // todo: put any options used with this provider. Not currently used.

                    break;

                case "source":
                    // todo: put any options for package sources

                    break;

                case "package":
                    // todo: put any options used when searching for packages
                    request.YieldDynamicOption("Contains", Constants.OptionType.String, false);
                    request.YieldDynamicOption("Updates", Constants.OptionType.Switch, false);
                    break;
            }
        }

        private void _resolvePackageSources(Request request)
        {
            bool bValidated = true;
            bool bIsTrusted = true;
            //try
            //{
            //    //_reAuthenticate(request); //Check if AuthToken is still valid
            //    bValidated = true;
            //    //if (!string.IsNullOrEmpty(_AuthenticationToken))
            //    //{
            //    //    bValidated = true;
            //    //}

            //    if(string.IsNullOrEmpty(Properties.Settings.Default.Location))
            //    {
            //        Properties.Settings.Default.Location = "";
            //        Properties.Settings.Default.Save();
            //    }

            //    if (string.Equals(Properties.Settings.Default.Location, RZRestAPIv2.sURL , StringComparison.InvariantCultureIgnoreCase))
            //        bIsTrusted = true;
            //}
            //catch (Exception ex)
            //{
            //    request.Debug("RZ112: " + ex.Message);
            //    return;
            //}

            string sLocation = "https://ruckzuck.azurewebsites.net";

            try
            {
                request.Debug("RZ Endpoint: " + RZRestAPIv2.sURL);
                sLocation = RZRestAPIv2.sURL;
            }
            catch(Exception ex)
            {
                request.Debug("E130: " + ex.Message);
            }

            request.YieldPackageSource("RuckZuck", sLocation, bIsTrusted, true, bValidated);
        }

        private void _addPackageSource(string name, string location, bool trusted, Request request)
        {
            //Properties.Settings.Default.Location = location;

            ////Set default URL if no loaction is specified
            //if (!string.IsNullOrEmpty(location))
            //{
            //    Properties.Settings.Default.Location = location;
            //    Properties.Settings.Default.Save();
            //}

            //RZRestAPIv2.sURL = Properties.Settings.Default.Location;

            //string sUser = "FreeRZ";
            //SecureString sPW = ToSecureString(GetTimeToken());


            //if (request.OptionKeys.Contains("Username"))
            //{
            //    Properties.Settings.Default.Username = request.GetOptionValue("Username");
            //    sUser = Properties.Settings.Default.Username;
            //}
            //else
            //    Properties.Settings.Default.Username = "";

            //if (request.OptionKeys.Contains("Password"))
            //{
            //    sPW = ToSecureString(request.GetOptionValue("Password"));
            //    Properties.Settings.Default.Password = EncryptString(sPW);
            //}
            //else
            //    Properties.Settings.Default.Password = "";

            //if (request.OptionKeys.Contains("EndPointURL"))
            //{
            //    Properties.Settings.Default.ContentURL = request.GetOptionValue("EndPointURL");
            //}
            //else
            //    Properties.Settings.Default.ContentURL = "";

            //Properties.Settings.Default.Save();

            //_AuthenticationToken = RZRestAPI.GetAuthToken(sUser, ToInsecureString(sPW));

            //Guid gToken;
            //if (!Guid.TryParse(_AuthenticationToken, out gToken))
            //{
            //    request.Warning(_AuthenticationToken);
            //    dLastTokenRefresh = new DateTime();
            //    return;
            //}

            //Set default URL if no loaction is specified
            if (!string.IsNullOrEmpty(location))
            {
                try
                {
                    if (location.ToLower().StartsWith("http"))
                    {
                        RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\RuckZuck", true);
                        if (key == null)
                            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies", true).CreateSubKey("RuckZuck");

                        key.SetValue("WebService", location);
                    }
                }
                catch(Exception ex)
                {
                    request.Debug("E197:" + ex.Message);
                }
            }

            if (request.OptionKeys.Contains("CustomerID"))
            {
                try
                {
                    string custid = request.GetOptionValue("CustomerID");

                    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\RuckZuck", true);

                    if (key == null)
                        key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies", true).CreateSubKey("RuckZuck");

                    key.SetValue("CustomerID", custid);
                }
                catch (Exception ex)
                {
                    request.Debug("E212:" + ex.Message);
                }
            }
        }

        private void _removePackageSource(string name, Request request)
        {
            //Properties.Settings.Default.Location = RZRestAPIv2.sURL;
            //Properties.Settings.Default.Username = "";
            //Properties.Settings.Default.Password = "";
            //Properties.Settings.Default.Save();

            //dLastTokenRefresh = new DateTime();

            //RZRestAPI.sURL = Properties.Settings.Default.Location;
        }

        /// <summary>
        /// Check if RuckZuck Token is still valid or request a new token if last refresh was mor than an hour
        /// </summary>
        /// <param name="request"></param>
        //private void _reAuthenticate(Request request)
        //{
        //    //Check if there is a token..
        //    //Guid gToken;
        //    //if (!Guid.TryParse(_AuthenticationToken, out gToken))
        //    //{
        //    //    dLastTokenRefresh = new DateTime();
        //    //}

        //    //Re-Authenticate after 30min
        //    if ((DateTime.Now - dLastTokenRefresh).TotalMinutes >= 30)
        //    {


        //        if (string.IsNullOrEmpty(Properties.Settings.Default.Location))
        //        {
        //            //Properties.Settings.Default.Location = WebServiceURL;
        //            //Properties.Settings.Default.Save();

        //        }

        //        //RZRestAPI.sURL = Properties.Settings.Default.Location;

        //        if (!string.IsNullOrEmpty(Properties.Settings.Default.Username))
        //        {
        //            //_AuthenticationToken = RZRestAPI.GetAuthToken(Properties.Settings.Default.Username, ToInsecureString(DecryptString(Properties.Settings.Default.Password)));
        //            //dLastTokenRefresh = DateTime.Now;
        //            request.Debug("RZ Account: " + Properties.Settings.Default.Username);
        //        }
        //        else
        //        {
        //            //_AuthenticationToken = RZRestAPI.GetAuthToken("FreeRZ", GetTimeToken());
        //            dLastTokenRefresh = DateTime.Now;
        //            request.Debug("RZ Account: FreeRZ");
        //        }

        //        //if (!Guid.TryParse(_AuthenticationToken, out gToken))
        //        //{
        //        //    dLastTokenRefresh = new DateTime();
        //        //    request.Warning(_AuthenticationToken);
        //        //    _AuthenticationToken = "";
        //        //    return;
        //        //}

        //        request.Debug("RZ Authentication Token:" + _AuthenticationToken);
        //    }
        //}

        private void _findPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Request request)
        {
            //_reAuthenticate(request); //Check if AuthToken is still valid

            try
            {
                bool exactSearch = true;
                if (request.OptionKeys.Contains("Contains"))
                {
                    name = request.GetOptionValue("Contains");
                    request.Message("exact search disabled.");
                    exactSearch = false;
                }

                //Search all if no name is specified
                if (string.IsNullOrEmpty(name))
                    exactSearch = false;

                bool bUpdate = false;
                if (request.OptionKeys.Contains("Updates"))
                {
                    request.Message("check updates for installed Software.");
                    bUpdate = true;
                }

                List<GetSoftware> lResult = new List<GetSoftware>();

                //Get all installed SW
                if (bUpdate)
                {
                    oScan.GetSWRepository().Wait(6000);
                    oScan.bCheckUpdates = false;
                    oScan.SWScanAsync().Wait();
                    oScan.CheckUpdatesAsync(null).Wait();
                    lSoftware = oScan.InstalledSoftware;

                    List<AddSoftware> RequiredUpdates = oScan.NewSoftwareVersions; // RZApi.CheckForUpdate(lSoftware.ToArray()).ToList().Where(t => t.Architecture != "new").ToList();
                    foreach (var SW in RequiredUpdates)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(name))
                            {
                                lResult.Add(new GetSoftware() { ProductName = SW.ProductName, ProductVersion = SW.ProductVersion, Manufacturer = SW.Manufacturer, ShortName = SW.ShortName, Description = SW.Description, ProductURL = SW.ProductURL });
                            }
                            else
                            {
                                if ((SW.ProductName.ToLowerInvariant() == name.ToLowerInvariant() | SW.ShortName.ToLowerInvariant() == name.ToLowerInvariant()) && exactSearch)
                                {
                                    lResult.Add(new GetSoftware() { ProductName = SW.ProductName, ProductVersion = SW.ProductVersion, Manufacturer = SW.Manufacturer, ShortName = SW.ShortName, Description = SW.Description, ProductURL = SW.ProductURL });
                                }
                                if ((SW.ProductName.ToLowerInvariant().Contains(name.ToLowerInvariant()) | SW.ShortName.ToLowerInvariant().Contains(name.ToLowerInvariant())) && !exactSearch)
                                {
                                    lResult.Add(new GetSoftware() { ProductName = SW.ProductName, ProductVersion = SW.ProductVersion, Manufacturer = SW.Manufacturer, ShortName = SW.ShortName, Description = SW.Description, ProductURL = SW.ProductURL });
                                }
                            }
                        }
                        catch { }
                    }
                    if (lResult.Count == 0)
                        request.Warning("No updates found...");
                }
                else
                {
                    if (string.IsNullOrEmpty(requiredVersion))
                    {
                        //Find by ShortName
                        if (exactSearch)
                        {
                            if (!string.IsNullOrEmpty(name))
                                lResult = RZRestAPIv2.GetCatalog().Where(t=>t.ShortName.ToLower() == name.ToLower()).OrderBy(t => t.ShortName).ToList();

                            if (lResult.Count == 0)
                            {
                                //Find any
                                lResult = RZRestAPIv2.GetCatalog().Where(t=>t.ProductName.ToLower() == name.ToLower()).OrderBy(t => t.ProductName).ToList();
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(name))
                                lResult = RZRestAPIv2.GetCatalog().Where(t=>t.ShortName.ToLower().Contains(name.ToLower())).OrderBy(t => t.ShortName).ToList();

                            if (lResult.Count == 0)
                            {
                                //Find any
                                lResult = RZRestAPIv2.GetCatalog().Where(t=>t.ProductName.ToLower().Contains(name.ToLower())).OrderBy(t => t.ShortName).ToList();
                            }
                            if (lResult.Count == 0)
                            {
                                //Find any
                                lResult = RZRestAPIv2.GetCatalog().Where(t => t.Manufacturer.ToLower().Contains(name.ToLower())).OrderBy(t => t.ShortName).ToList();
                            }
                        }
                    }
                    else
                    {
                        //Find by Shortname
                        if (exactSearch)
                        {
                            lResult = RZRestAPIv2.GetCatalog().Where(t=>t.ShortName.ToLower() == name.ToLower() && t.ProductVersion.ToLower() == requiredVersion.ToLower()).OrderBy(t => t.ShortName).ToList();
                        }
                        else
                        {
                            //Find any
                            lResult = RZRestAPIv2.GetCatalog().Where(t => t.ShortName.ToLower().Contains(name.ToLower()) && t.ProductVersion.ToLower() == requiredVersion.ToLower()).OrderBy(t => t.ShortName).ToList();

                            if(lResult.Count == 0)
                                lResult = RZRestAPIv2.GetCatalog().Where(t => t.ProductName.ToLower().Contains(name.ToLower()) && t.ProductVersion.ToLower() == requiredVersion.ToLower()).OrderBy(t => t.ShortName).ToList();

                            if (lResult.Count == 0)
                                lResult = RZRestAPIv2.GetCatalog().Where(t => t.Manufacturer.ToLower().Contains(name.ToLower()) && t.ProductVersion.ToLower() == requiredVersion.ToLower()).OrderBy(t => t.ShortName).ToList();
                        }
                    }
                }


                if (minimumVersion != null)
                {
                    try
                    {
                        lResult = lResult.Where(p => Version.Parse(p.ProductVersion) >= Version.Parse(minimumVersion)).ToList();
                    }
                    catch
                    {
                        lResult = lResult.Where(p => p.ProductVersion == minimumVersion).ToList();
                    }
                }
                if (maximumVersion != null)
                {
                    try
                    {
                        lResult = lResult.Where(p => Version.Parse(p.ProductVersion) <= Version.Parse(maximumVersion)).ToList();
                    }
                    catch
                    {
                        lResult = lResult.Where(p => p.ProductVersion == maximumVersion).ToList();
                    }
                }


                foreach (var SW in lResult.OrderBy(t => t.ShortName))
                {
                    request.YieldSoftwareIdentity(SW.ProductName + ";" + SW.ProductVersion + ";" + SW.Manufacturer, SW.ProductName, SW.ProductVersion, "", SW.Description, RZRestAPIv2.sURL, name, SW.SWId.ToString(), SW.ShortName);
                    //Trust the original RucKZuck source
                    request.AddMetadata("FromTrustedSource", "True");
                    //if (string.Equals(Properties.Settings.Default.Location, RZRestAPIv2.sURL , StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    request.AddMetadata("FromTrustedSource", "True");
                    //}
                }

            }
            catch (Exception ex)
            {
                request.Debug("E334:" + ex.Message);
            }
        }

        /// <summary>
        /// Install a Software from RuckZuck repo
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request"></param>
        private void _installPackage(string fastPackageReference, Request request)
        {
            //_reAuthenticate(request); //Check if AuthToken is still valid

            bool bSkipDep = false;
            bool bDLPath = false;
            string sDLPath = "";

            if (request.OptionKeys.Contains("SkipDependencies"))
            {
                request.Message("Skip all dependencies.");
                bSkipDep = true;
            }
            if (request.OptionKeys.Contains("LocalPath"))
            {
                sDLPath = request.GetOptionValue("LocalPath");
                request.Message("Download-Path set to:" + sDLPath);
                bDLPath = true;
            }

            string sProd = fastPackageReference;
            string sVer = "";
            string sManu = "";

            if (fastPackageReference.Contains(";"))
            {
                try
                {
                    sProd = fastPackageReference.Split(';')[0].Trim();
                    sVer = fastPackageReference.Split(';')[1].Trim();
                    sManu = fastPackageReference.Split(';')[2].Trim();
                }
                catch { }
            }

            if (request.OptionKeys.Contains("Manufacturer"))
            {
                sManu = request.GetOptionValue("Manufacturer");
            }

            if (request.OptionKeys.Contains("ProductVersion"))
            {
                sVer = request.GetOptionValue("ProductVersion");
            }

            if (request.OptionKeys.Contains("ProductName"))
            {
                sProd = request.GetOptionValue("ProductName");
            }

            oUpdate = new RZUpdate.RZUpdater();
            oUpdate.SoftwareUpdate.SW.ProductName = sProd;
            oUpdate.SoftwareUpdate.SW.ProductVersion = sVer;
            oUpdate.SoftwareUpdate.SW.Manufacturer = sManu;

            oUpdate.SoftwareUpdate.GetInstallType();

            if (!bDLPath)
                oUpdate.SoftwareUpdate.Download(false).Result.ToString();
            else
                oUpdate.SoftwareUpdate.Download(false, sDLPath).Result.ToString();

            if (oUpdate.SoftwareUpdate.Install(false, false).Result)
                request.Verbose(sManu + " " + sProd + " " + sVer + " installed.");
            else
                request.Verbose(sManu + " " + sProd + " " + sVer + " NOT installed.");
        }

        /// <summary>
        /// Uninstall a SW
        /// </summary>
        /// <param name="fastPackageReference"></param>
        /// <param name="request"></param>
        private void _uninstallPackage(string fastPackageReference, Request request)
        {
            //_reAuthenticate(request); //Check if AuthToken is still valid

            string sProd = fastPackageReference;
            string sVer = "";
            string sManu = "";

            if (fastPackageReference.Contains(";"))
            {
                try
                {
                    sProd = fastPackageReference.Split(';')[0].Trim();
                    sVer = fastPackageReference.Split(';')[1].Trim();
                    sManu = fastPackageReference.Split(';')[2].Trim();
                }
                catch { }
            }

            if (string.IsNullOrEmpty(sVer))
            {
                sVer = lSoftware.First(t => t.ProductName == sProd).ProductVersion;
            }

            request.Debug(sProd);
            request.Debug(sVer);

            oUpdate = new RZUpdate.RZUpdater();
            oUpdate.SoftwareUpdate.SW.ProductName = sProd;
            oUpdate.SoftwareUpdate.SW.ProductVersion = sVer;
            oUpdate.SoftwareUpdate.SW.Manufacturer = sManu;

            oUpdate.SoftwareUpdate.GetInstallType();

            if (!string.IsNullOrEmpty(oUpdate.SoftwareUpdate.SW.PSUninstall))
                RunPS(oUpdate.SoftwareUpdate.SW.PSUninstall);
        }

        private void _downloadPackage(string fastPackageReference, string location, Request request)
        {
            //_reAuthenticate(request); //Check if AuthToken is still valid

            bool bSkipDep = false;
            if (request.OptionKeys.Contains("SkipDependencies"))
            {
                request.Message("Skip all dependencies.");
                bSkipDep = true;
            }

            string sProd = fastPackageReference;
            string sVer = "";
            string sManu = "";

            if (fastPackageReference.Contains(";"))
            {
                try
                {
                    sProd = fastPackageReference.Split(';')[0].Trim();
                    sVer = fastPackageReference.Split(';')[1].Trim();
                    sManu = fastPackageReference.Split(';')[2].Trim();
                }
                catch { }
            }

            request.Debug("Calling 'SWGet::DownloadPackage' '{0}', '{1}'", sProd, sVer);

            oUpdate.SoftwareUpdate.SW.ProductName = sProd;
            oUpdate.SoftwareUpdate.SW.ProductVersion = sVer;
            oUpdate.SoftwareUpdate.SW.Manufacturer = sManu;

            oUpdate.SoftwareUpdate.GetInstallType();
            if (oUpdate.SoftwareUpdate.Download(false, location).Result)
                request.Verbose(sManu + " " + sProd + " " + sVer + " downloaded.");
            else
                request.Verbose(sManu + " " + sProd + " " + sVer + " NOT downloaded.");

        }

        private void _getInstalledPackages(string name, string requiredVersion, string minimumVersion, string maximumVersion, Request request)
        {
            //_reAuthenticate(request); //Check if AuthToken is still valid

            try
            {
                List<AddSoftware> lResult = getInstalledSW().ToList();
                List<GetSoftware> lCatalog = RZRestAPIv2.GetCatalog();

                //List<GetSoftware> lServer = RZRestAPI.SWResults(name).OrderBy(t => t.Shortname).ToList();

                List<AddSoftware> lLocal = lResult.Where(t => lCatalog.Count(x => x.ProductName == t.ProductName && x.Manufacturer == t.Manufacturer && x.ProductVersion == t.ProductVersion) != 0).OrderBy(t => t.ProductName).ThenBy(t => t.ProductVersion).ThenBy(t => t.Manufacturer).ToList();
                request.Debug("Items Found: " + lLocal.Count().ToString());

                //List<AddSoftware> lLocal = lResult.Where(t => lServer.Count(x => x.ProductName == t.ProductName & x.Manufacturer == t.Manufacturer & x.ProductVersion == t.ProductVersion) != 0).OrderBy(t => t.ProductName).ThenBy(t => t.ProductVersion).ThenBy(t => t.Manufacturer).ToList();


                if (!string.IsNullOrEmpty(name))
                {
                    string sProdName = "";
                    try
                    {
                        sProdName = lCatalog.FirstOrDefault(p => string.Equals(p.ShortName, name, StringComparison.OrdinalIgnoreCase)).ProductName;
                    }
                    catch { }
                    lLocal = lLocal.Where(p => String.Equals(p.ProductName, name, StringComparison.OrdinalIgnoreCase) | String.Equals(p.ProductName, sProdName, StringComparison.OrdinalIgnoreCase)).OrderBy(t => t.ProductName).ToList();
                }

                if (requiredVersion != null)
                {
                    lLocal = lLocal.Where(p => p.ProductVersion.ToLowerInvariant() == requiredVersion.ToLowerInvariant()).ToList();
                }
                if (minimumVersion != null)
                {
                    try
                    {
                        lLocal = lLocal.Where(p => Version.Parse(p.ProductVersion) >= Version.Parse(minimumVersion)).ToList();
                    }
                    catch
                    {
                        lLocal = lLocal.Where(p => p.ProductVersion == minimumVersion).ToList();
                    }
                }
                if (maximumVersion != null)
                {
                    try
                    {
                        lLocal = lLocal.Where(p => Version.Parse(p.ProductVersion) <= Version.Parse(maximumVersion)).ToList();
                    }
                    catch
                    {
                        lLocal = lLocal.Where(p => p.ProductVersion == maximumVersion).ToList();
                    }
                }

                foreach (var SW in lLocal)
                {
                    var oCAT = lCatalog.FirstOrDefault(t => t.ProductName == SW.ProductName && t.ProductVersion == SW.ProductVersion && t.Manufacturer == SW.Manufacturer);
                    if (oCAT != null)
                        request.YieldSoftwareIdentity(SW.ProductName + ";" + SW.ProductVersion, SW.ProductName, SW.ProductVersion, "", oCAT.Description, RZRestAPIv2.sURL, (name ?? ""), oCAT.SWId.ToString(), oCAT.ShortName);
                    else
                        request.YieldSoftwareIdentity(SW.ProductName + ";" + SW.ProductVersion, SW.ProductName, SW.ProductVersion, "", "", RZRestAPIv2.sURL, (name ?? ""), "", SW.ShortName);
                    request.AddMetadata("FromTrustedSource", "True");
                }
            }
            catch
            {
                //dLastTokenRefresh = new DateTime();
            }
        }

        private static string GetTimeToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            return Convert.ToBase64String(time.Concat(key).ToArray());
        }

        public Collection<PSObject> RunPS(string PSScript)
        {
            PowerShell PowerShellInstance = PowerShell.Create();

            PowerShellInstance.AddScript(PSScript);

            Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

            return PSOutput;

        }

        public string descramble(string sKey)
        {
            int[] code = new int[] { 8, 4, 4, 2, 2, 2, 2, 2, 2, 2, 2 };
            int ipos = 0;
            string sResult = "";
            for (int i = 0; i < code.Length; i++)
            {
                sResult = sResult + new string(sKey.Substring(ipos, code[i]).Reverse().ToArray());
                ipos = ipos + code[i];
            }

            return sResult;
        }

        public List<AddSoftware> getInstalledSW()
        {
            lSoftware = new List<AddSoftware>();

            if (oScan.bInitialScan)
            {
                oScan = new RZScan(false, false);
                oScan.SWScanAsync().Wait();
            }

            return oScan.InstalledSoftware;
        }

        #region PW 
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("RZ" + Environment.UserName);

        public static string EncryptString(System.Security.SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static System.Security.SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        public static System.Security.SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }

        #endregion
    }
}
