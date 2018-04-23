using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace M1.Utilities
{
    public class Config : SingletonBehaviour<Config>
    {
        private Dictionary<string, string> configData = new Dictionary<string, string>();
        private Container containerXML;

        public static string Read(CONFIG_KEYS _key)
        {
            return Instance.configData[_key.ToString()];
        }

        public static bool HasKey(CONFIG_KEYS _key)
        {
            if (Instance.configData.ContainsKey(_key.ToString()))
                return true;
            else
                return false;
        }

        void Awake()
        {
            DontDestroy();
            configData = GetConfigData();

        }

        IEnumerator Start()
        {
            yield return null;
            containerXML = Container.Load("Container.xml");
            yield return null;
            DebugSystemInformation();
            ApplyQualitySettings();
            DebugXML();
        }

        private Dictionary<string, string> GetConfigData()
        {
            Dictionary<string, string> configData = new Dictionary<string, string>();
            
            string line;
            StreamReader inStream;

#if UNITY_EDITOR
            if (!File.Exists(Application.dataPath + "/Config/config.txt"))
                File.Create(Application.dataPath + "/Config/config.txt");

            inStream = new StreamReader(Application.dataPath + "/Config/config.txt");
#else
        if (!File.Exists(Application.dataPath + "/../Config/" + "config.txt"))
            File.Create(Application.dataPath + "/../Config/" + "config.txt");

        inStream = new StreamReader(Application.dataPath + "/../Config/" + "config.txt");
#endif
            while ((line = inStream.ReadLine()) != null)
            {
                line = line.Replace(" ", "");

                if (line.Length > 0 && string.Compare(line[0].ToString(), "/") != 0)
                {
                    string[] words = line.Split('=', '/');
                    if (words.Length > 1)
                    {
                        string key = words[0];
                        string value = words[1];
                        configData.Add(key.ToLower(), value);
                    }
                }
            }
            inStream.Close();

            return configData;
        }

        private Dictionary<string, int> GetIndexgData()
        {
            Dictionary<string, int> indexgData = new Dictionary<string, int>();
            
            string line;
            StreamReader inStream;

#if UNITY_EDITOR
            if (!File.Exists(Application.dataPath + "/Config/config.txt"))
                File.Create(Application.dataPath + "/Config/config.txt");

            inStream = new StreamReader(Application.dataPath + "/Config/config.txt");
#else
        if (!File.Exists(Application.dataPath + "/../Config/" + "config.txt"))
            File.Create(Application.dataPath + "/../Config/" + "config.txt");

        inStream = new StreamReader(Application.dataPath + "/../Config/" + "config.txt");
#endif
            var idx = 0;
            
            while ((line = inStream.ReadLine()) != null)
            {
                line = line.Replace(" ", "");

                if (line.Length > 0 && string.Compare(line[0].ToString(), "/") != 0)
                {
                    string[] words = line.Split('=', '/');
                    if (words.Length > 1)
                    {
                        string key = words[0]; 
                        int value = idx;
                        indexgData.Add(key.ToLower(), value);
                        idx++;
                    }
                }
            }
            inStream.Close();

            return indexgData;
        }
        
        
        private void DebugSystemInformation()
        {
            string s = "";
            s += "////////////////////////////////////////////////////////////////////////////////\n";
            s += "// System Info: //\n";
            s += "////////////////////////////////////////////////////////////////////////////////\n";

            s += "\nTime: " + DateTime.Now.ToString("MM-dd-yy_HHmm");
            s += "\nMachineName: " + System.Environment.MachineName;
            s += "\nUserName: " + System.Environment.UserName;
            s += "\nUserDomainName: " + System.Environment.UserDomainName;
            s += "\nOSVersion: " + System.Environment.OSVersion;

            s += "\n////////////////////////////////////////////////////////////////////////////////\n";
            Debug.Log(s);
        }

        private void ApplyQualitySettings()
        {
            // vsync
            if (HasKey(CONFIG_KEYS.vsync))
                QualitySettings.vSyncCount = int.Parse(Read(CONFIG_KEYS.vsync));// Config.configData[CONFIG_KEYS.vsync.ToString()]);

            // pixel light count
            if (HasKey(CONFIG_KEYS.pixellightcount))
                QualitySettings.pixelLightCount = int.Parse(Read(CONFIG_KEYS.pixellightcount));

            // aa
            if (HasKey(CONFIG_KEYS.aa))
                QualitySettings.antiAliasing = int.Parse(Read(CONFIG_KEYS.aa));

            // af
            if (HasKey(CONFIG_KEYS.af))
            {
                int tmp = int.Parse(Read(CONFIG_KEYS.af));
                if (Enum.IsDefined(typeof(AnisotropicFiltering), tmp))
                {
                    QualitySettings.anisotropicFiltering = (AnisotropicFiltering)tmp;
                }
            }
            
            // blend Weights
            if (HasKey(CONFIG_KEYS.blendweights))
            {
                int tmp = int.Parse(Read(CONFIG_KEYS.blendweights));
                if (Enum.IsDefined(typeof(BlendWeights), tmp))
                {
                    QualitySettings.blendWeights = (BlendWeights)tmp;
                }
            }
            
            
            
            // set resolution
            int width = Screen.width;
            int height = Screen.height;
            if (HasKey(CONFIG_KEYS.screenwidth) && HasKey(CONFIG_KEYS.screenheight))
            {
                width = int.Parse(Read(CONFIG_KEYS.screenwidth));
                height = int.Parse(Read(CONFIG_KEYS.screenheight));
            }

            bool fullScreen = true;
            if (HasKey(CONFIG_KEYS.fullscreen))
                fullScreen = bool.Parse(Read(CONFIG_KEYS.fullscreen));

            int refreshRate = Screen.currentResolution.refreshRate;
            if (HasKey(CONFIG_KEYS.refreshrate))
                refreshRate = int.Parse(Read(CONFIG_KEYS.refreshrate));

            Screen.SetResolution(width, height, fullScreen, refreshRate);


            // debug settings
            string s = "";
            s += "////////////////////////////////////////////////////////////////////////////////\n";
            s += "// QualitySettings: //\n";
            s += "////////////////////////////////////////////////////////////////////////////////\n";

            s += "\nComPort: " + Read(CONFIG_KEYS.comport);
            s += "\nvSyncCount: " + QualitySettings.vSyncCount;
            s += "\npixelLightCount: " + QualitySettings.pixelLightCount;
            s += "\nantiAliasing: " + QualitySettings.antiAliasing;
            s += "\nanisotropicFiltering: " + QualitySettings.anisotropicFiltering;
            s += "\nblendWeights: " + QualitySettings.blendWeights;
            s += "\nresolution : " + width + "x" + height +
                           "\nfullScreen: " + fullScreen +
                           "\nrefreshRate: " + refreshRate;
            s += "\nDebug: " + Read(CONFIG_KEYS.debug);

            s += "\n////////////////////////////////////////////////////////////////////////////////\n";
            Debug.Log(s);

        }

        private void DebugXML()
        {
            string s = "";
            s += "////////////////////////////////////////////////////////////////////////////////\n";
            s += "// XML: " + containerXML.data.Count + " //\n";
            s += "////////////////////////////////////////////////////////////////////////////////\n";

            for (int i = 0; i < containerXML.data.Count; i++)
            {
                s += "\nContainer: " + i;
                s += "\nTestEnum: " + containerXML.data[i].testEnum;
                s += "\nInt Value: " + containerXML.data[i].intValue;
                s += "\nFloat Value: " + containerXML.data[i].floatValue;
                s += "\nString Value: " + containerXML.data[i].stringValue;

                foreach (string t in containerXML.data[i].Text)
                {
                    s += "\nText: " + t;
                }

                s += "\n";
            }

            s += "////////////////////////////////////////////////////////////////////////////////\n";
            Debug.Log(s);
        }

    }

    public enum CONFIG_KEYS
    {
        comport,
        vsync,
        pixellightcount,
        aa,
        af,
        blendweights,
        screenwidth,
        screenheight,
        fullscreen,
        refreshrate,
        debug,
        openlane4,
        callibrationmode,
        autotest,
        plcip,
        distance,
        showplot,
        manualinput,
        manualteamselect,
        targetthreshold,
        dmxdisable,
        audiodevice,
        audiodisable,
        arizona_cardinals,
        atlanta_falcons,
        baltimore_ravens,
        buffalo_bills,
        carolina_panthers,
        chicago_bears,
        cincinnati_bengals,
        cleveland_browns,
        dallas_cowboys,
        denver_broncos,
        detroit_lions,
        green_bay_packers,
        houston_texans,
        indianapolis_colts,
        jacksonville_jaguars,
        kansas_city_chiefs,
        los_angeles_chargers,
        los_angeles_rams,
        miami_dolphins,
        minnesota_vikings,
        new_england_patriots,
        new_orleans_saints,
        new_york_giants,
        new_york_jets,
        oakland_raiders,
        philadelphia_eagles,
        pittsburgh_steelers,
        san_francisco_49ers,
        seattle_seahawks,
        tampa_bay_buccaneers,
        tennessee_titans,
        washington_redskins,
        ambientvolume,
        lanevolume,
        refvolumeoffset,
        crowdvolumeoffset
       
    }
}