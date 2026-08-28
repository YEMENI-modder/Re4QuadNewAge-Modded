using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Re4QuadExtremeEditor.src.Class.Enums;
using System.IO;

namespace Re4QuadExtremeEditor.src.JSON
{
    /// <summary>
    /// Representa o arquivo de configurações json, nas quais são replicadas na classe Globals;
    /// </summary>
    public class Configs
    {
        public string DirectoryXFILE { get; set; }
        public string Directory2007RE4 { get; set; }
        public string DirectoryPS2RE4 { get; set; }
        public string DirectoryUHDRE4 { get; set; }
        public string DirectoryPS4NSRE4 { get; set; }
        public string DirectoryCustom1 { get; set; }
        public string DirectoryCustom2 { get; set; }
        public string DirectoryCustom3 { get; set; }

        // external tool paths (manual fallback when the bundled copy is missing)
        public string ToolPathUDAS { get; set; }
        public string ToolPathLFS { get; set; }
        public string ToolPathPACK { get; set; }
        public string ToolPathGCA { get; set; }

        // files included in the room complete load
        public bool IncludeAEV { get; set; }
        public bool IncludeDSE { get; set; }
        public bool IncludeEAR { get; set; }
        public bool IncludeEMI { get; set; }
        public bool IncludeESE { get; set; }
        public bool IncludeETS { get; set; }
        public bool IncludeFSE { get; set; }
        public bool IncludeITA { get; set; }
        public bool IncludeLIT { get; set; }
        public bool IncludeSAR { get; set; }

        //file source mode for Load With Objects
        public bool UseDataQingshengSource { get; set; }

        //listagens json
        public string FileDiretoryItemsList { get; set; }
        public string FileDiretoryEtcModelsList { get; set; }
        public string FileDiretoryEnemiesList { get; set; }
        public string FileDiretoryQuadCustomList { get; set; }

        public Color SkyColor { get; set; }

        // floats
        public ConfigFrationalSymbol FrationalSymbol { get; set; }
        public int FrationalAmount { get; set; }

        //items rotations
        public bool ItemDisableRotationAll { get; set; }
        public bool ItemDisableRotationIfXorYorZequalZero { get; set; }
        public bool ItemDisableRotationIfZisNotGreaterThanZero { get; set; }
        public ObjRotationOrder ItemRotationOrder { get; set; }
        public float ItemRotationCalculationMultiplier { get; set; }
        public float ItemRotationCalculationDivider { get; set; }

        //theme
        public bool UseDarkerGrayTheme { get; set; }

        // light mode mirror of the dark theme (same layout, light palette)
        public bool UseLightTheme { get; set; }

        // botao do mouse invertido
        public bool UseInvertedMouseButtons { get; set; }

        // first-run welcome/setup wizard already completed
        public bool SetupDone { get; set; }

        // lang
        public bool LoadLangTranslation { get; set; }
        public string LangJsonFile { get; set; }

        // recently opened data files, "Kind|Path" entries (most recent first)
        public List<string> RecentFiles { get; set; }

        // viewport frame-rate limit (0 = unlimited)
        public int FpsLimit { get; set; }


        /// <summary>
        /// define as configs padrões 
        /// </summary>
        /// <returns></returns>
        public static Configs GetDefaultConfigs()
        {
            Configs configs = new Configs();
            configs.DirectoryXFILE = @"";
            configs.Directory2007RE4 = @"";
            configs.DirectoryPS2RE4 = @"";
            configs.DirectoryUHDRE4 = @"";
            configs.DirectoryPS4NSRE4 = @"";
            configs.DirectoryCustom1 = @"";
            configs.DirectoryCustom2 = @"";
            configs.DirectoryCustom3 = @"";

            configs.ToolPathUDAS = @"";
            configs.ToolPathLFS = @"";
            configs.ToolPathPACK = @"";
            configs.ToolPathGCA = @"";

            configs.IncludeAEV = true;
            configs.IncludeDSE = true;
            configs.IncludeEAR = true;
            configs.IncludeEMI = true;
            configs.IncludeESE = true;
            configs.IncludeETS = true;
            configs.IncludeFSE = true;
            configs.IncludeITA = true;
            configs.IncludeLIT = true;
            configs.IncludeSAR = true;

            configs.UseDataQingshengSource = false;

            configs.FileDiretoryEnemiesList = Consts.DefaultEnemiesListFileDirectory;
            configs.FileDiretoryEtcModelsList = Consts.DefaultEtcModelsListFileDirectory;
            configs.FileDiretoryItemsList = Consts.DefaultItemsListFileDirectory;
            configs.FileDiretoryQuadCustomList = Consts.DefaultQuadCustomModelsListFileDirectory;

            configs.SkyColor = Color.FromArgb(0xFF, 0x94, 0xD2, 0xFF);
            // colocar novas configurões aqui;
            configs.FrationalAmount = 9;
            configs.FrationalSymbol = ConfigFrationalSymbol.AcceptsCommaAndPeriod_OutputPeriod;

            configs.ItemDisableRotationAll = false;
            configs.ItemDisableRotationIfXorYorZequalZero = false;
            configs.ItemDisableRotationIfZisNotGreaterThanZero = true;
            configs.ItemRotationOrder = ObjRotationOrder.RotationXY;
            configs.ItemRotationCalculationMultiplier = 1;
            configs.ItemRotationCalculationDivider = 1;

            configs.UseDarkerGrayTheme = false;
            configs.UseLightTheme = false;
            configs.UseInvertedMouseButtons = false;
            configs.SetupDone = false;
            configs.LoadLangTranslation = false;
            configs.LangJsonFile = "";

            configs.RecentFiles = new List<string>();
            configs.FpsLimit = 120;
            return configs;
        }

        /// <summary>
        /// metodo que tem como função carregar as cofigurações ao carregar;
        /// </summary>
        public static void StartLoadConfigs()
        {
            if (File.Exists(Consts.ConfigsFileDirectory))
            {
                Configs configs = GetDefaultConfigs();
                // para caso o arquivo não consiga ser lido
                try { configs = ConfigsFile.parseConfigs(Consts.ConfigsFileDirectory); } catch (Exception) { }

                Globals.BackupConfigs = configs;
                Globals.DirectoryXFILE = configs.DirectoryXFILE;
                Globals.Directory2007RE4 = configs.Directory2007RE4;
                Globals.DirectoryPS2RE4 = configs.DirectoryPS2RE4;
                Globals.DirectoryUHDRE4 = configs.DirectoryUHDRE4;
                Globals.DirectoryPS4NSRE4 = configs.DirectoryPS4NSRE4;
                Globals.DirectoryCustom1 = configs.DirectoryCustom1;
                Globals.DirectoryCustom2 = configs.DirectoryCustom2;
                Globals.DirectoryCustom3 = configs.DirectoryCustom3;

                // newer optional entries (older Configs.json files may lack them)
                if (configs.ToolPathUDAS != null) Globals.ToolPathUDAS = configs.ToolPathUDAS;
                if (configs.ToolPathLFS != null) Globals.ToolPathLFS = configs.ToolPathLFS;
                if (configs.ToolPathPACK != null) Globals.ToolPathPACK = configs.ToolPathPACK;
                if (configs.ToolPathGCA != null) Globals.ToolPathGCA = configs.ToolPathGCA;
                Globals.includeAEV = configs.IncludeAEV;
                Globals.includeDSE = configs.IncludeDSE;
                Globals.includeEAR = configs.IncludeEAR;
                Globals.includeEMI = configs.IncludeEMI;
                Globals.includeESE = configs.IncludeESE;
                Globals.includeETS = configs.IncludeETS;
                Globals.includeFSE = configs.IncludeFSE;
                Globals.includeITA = configs.IncludeITA;
            Globals.useDataQingshengSource = configs.UseDataQingshengSource;
                Globals.includeLIT = configs.IncludeLIT;
                Globals.includeSAR = configs.IncludeSAR;

                Globals.FileDiretoryEnemiesList = configs.FileDiretoryEnemiesList;
                Globals.FileDiretoryEtcModelsList = configs.FileDiretoryEtcModelsList;
                Globals.FileDiretoryItemsList = configs.FileDiretoryItemsList;
                Globals.FileDiretoryQuadCustomList = configs.FileDiretoryQuadCustomList;

                Globals.SkyColor = configs.SkyColor;

                // colocar novas configurões aqui;
                Globals.FrationalAmount = configs.FrationalAmount;
                Globals.FrationalSymbol = configs.FrationalSymbol;

                Globals.ItemDisableRotationAll = configs.ItemDisableRotationAll;
                Globals.ItemDisableRotationIfXorYorZequalZero = configs.ItemDisableRotationIfXorYorZequalZero;
                Globals.ItemDisableRotationIfZisNotGreaterThanZero = configs.ItemDisableRotationIfZisNotGreaterThanZero;
                Globals.ItemRotationOrder = configs.ItemRotationOrder;
                Globals.ItemRotationCalculationMultiplier = configs.ItemRotationCalculationMultiplier;
                Globals.ItemRotationCalculationDivider = configs.ItemRotationCalculationDivider;

                // newer optional entries (older Configs.json files may lack them)
                if (configs.RecentFiles != null)
                {
                    Re4QuadExtremeEditor.src.Class.RecentFiles.Restore(configs.RecentFiles);
                }
                if (configs.FpsLimit >= 0)
                {
                    Globals.FpsLimit = configs.FpsLimit;
                }
            }
            else
            {
                // para caso o arquivo não consiga ser gravado
                try { ConfigsFile.writeConfigsFile(Consts.ConfigsFileDirectory, GetDefaultConfigs()); } catch (Exception) { }

                Globals.BackupConfigs = GetDefaultConfigs();
            }

        }
    }
}
