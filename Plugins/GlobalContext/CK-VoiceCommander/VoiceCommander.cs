using System;
using System.Collections.Generic;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using System.Windows;
using CK_WindowManager;
using System.Media;
using System.Xml;
using System.IO;
using System.Speech.Recognition;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.ComponentModel;
namespace CK_VoiceCommander
{
    /// <summary>
    /// Class that represent a CiviKey plugin
    /// </summary>
    [Plugin(PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion)]
    public class CK_VoiceCommander : IPlugin
    {
        //This GUID has been generated when you created the project, you may use it as is
        const string PluginGuidString = "{c35e6495-89e6-45ac-9898-2380c5a4a206}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "CK-VoiceCommander";
        [DynamicService(Requires = RunningRequirement.MustExistAndRun)]
        public static IWindowManager windowManager { get; set; }
        //Reference to the storage object that enables one to save data.
        //This object is injected after all plugins' Setup method has been called
        public IPluginConfigAccessor Config { get; set; }
        private XmlTextWriter myXmlTextWriter;
        SpeechRecognitionEngine asr;
        private string path;
        /// <summary>
        /// First called method on the class, at this point, all services are null.
        /// Used to set up the service exposed by this plugin (if any).
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Setup(IPluginSetupInfo info)
        {
            return true;
        }

        /// <summary>
        /// Called after the Setup method.
        /// All launched services are now set, you may now set up the link to the service used by this class
        /// </summary>
        public void Start()
        {
            grammar();
            for (int i = 0; i < windowManager.WindowList.Count; i++)
                windowManager.WindowList[i].PropertyChanged += new PropertyChangedEventHandler(parametersListener);
            asr = new SpeechRecognitionEngine();
            asr.SetInputToDefaultAudioDevice();
            asr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(asr_SpeechRecognized);
            asr.LoadGrammarCompleted += new EventHandler<LoadGrammarCompletedEventArgs>(asr_LoadGrammarCompleted);
            string path = Path.Combine(Environment.CurrentDirectory, @"..\..\..\CK-VoiceCommander\grammar.xml");
            asr.LoadGrammarAsync(new Grammar(path));
            asr.RecognizeAsync(RecognizeMode.Multiple);
            INotifyCollectionChanged z = windowManager.WindowList;
            z.CollectionChanged+=new NotifyCollectionChangedEventHandler(windowListener);
        }

        /// <summary>
        /// First method called when the plugin is stopping
        /// 
        /// You should remove all references to any service here.
        /// </summary>
        public void Stop()
        {

        }

        /// <summary>
        /// Called after Stop()
        /// All services are null
        /// </summary>
        public void Teardown()
        { }

        public void windowListener(Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                grammar();
            }
        }
        void asr_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                System.Windows.MessageBox.Show("incorrect grammar"); 
            }
        }

        void asr_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            bool found=false;
            int key = Convert.ToInt32(e.Result.Semantics.Value.ToString());      
            for (int i = 0; i < windowManager.WindowList.Count && !found; i++)
            {
                if (windowManager.WindowList[i].Z == key)
                {
                    if (!windowManager.WindowList[i].ScreenInfo.IsPrimaryScreen)
                    {
                        windowManager.WindowList[i].Move(0, 0);
                    }
                    else
                    {
                        for (int j = 0; j < windowManager.ScreenList.Count; j++)
                        {
                            if (!windowManager.ScreenList[j].IsPrimaryScreen)
                            {
                                int top=windowManager.ScreenList[j].Bounds.Y;
                                int left = windowManager.ScreenList[j].Bounds.X;
                                windowManager.WindowList[i].Move(left, top);
                            }
                        }
                    }
                    found = true;
                }
            }
          
            
        }

        public void grammar()
        {

            path = Path.Combine(Environment.CurrentDirectory, @"..\..\..\CK-VoiceCommander\grammar.xml");
            myXmlTextWriter = new XmlTextWriter(path, null);
            myXmlTextWriter.Formatting = Formatting.Indented;
            myXmlTextWriter.WriteStartDocument(false);
            myXmlTextWriter.WriteStartElement("grammar");
            myXmlTextWriter.WriteAttributeString("xml:lang", "fr-FR");
            myXmlTextWriter.WriteAttributeString("root", "root");
            myXmlTextWriter.WriteAttributeString("tag-format", "semantics/1.0");
            myXmlTextWriter.WriteAttributeString("version", "1.0");
            myXmlTextWriter.WriteAttributeString("xmlns", "http://www.w3.org/2001/06/grammar");
            myXmlTextWriter.WriteStartElement("rule");
            myXmlTextWriter.WriteAttributeString("id", "root");
            myXmlTextWriter.WriteAttributeString("scope", "private");
            myXmlTextWriter.WriteStartElement("one-of", null);
            for (int key = 0; key < windowManager.WindowList.Count; key++)
            {
                if (windowManager.WindowList[key].X > -10000 && windowManager.WindowList[key].Title != "" && windowManager.WindowList[key].Title != "Program Manager" && windowManager.WindowList[key].Title != "Démarrer" && windowManager.WindowList[key].Title != "MainWindow")
                {
                    myXmlTextWriter.WriteStartElement("item", null);
                    myXmlTextWriter.WriteStartElement("ruleref", null);
                    myXmlTextWriter.WriteAttributeString("uri", "#ID_DEPLACER" + windowManager.WindowList[key].Z);
                    myXmlTextWriter.WriteEndElement();
                    myXmlTextWriter.WriteElementString("tag", "out=" + windowManager.WindowList[key].Z + ";");
                    myXmlTextWriter.WriteEndElement();
                }
            }
            myXmlTextWriter.WriteEndElement();

            myXmlTextWriter.WriteEndElement();

            for (int key = 0; key < windowManager.WindowList.Count; key++)
            {
                if (windowManager.WindowList[key].X > -10000 && windowManager.WindowList[key].Title != "" && windowManager.WindowList[key].Title != "Program Manager" && windowManager.WindowList[key].Title != "Démarrer" && windowManager.WindowList[key].Title != "MainWindow")
                {
                    myXmlTextWriter.WriteStartElement("rule");
                    myXmlTextWriter.WriteAttributeString("id", "ID_DEPLACER" + windowManager.WindowList[key].Z);
                    myXmlTextWriter.WriteStartElement("one-of");
                    myXmlTextWriter.WriteElementString("item",windowManager.WindowList[key].Z.ToString());
                    myXmlTextWriter.WriteEndElement();
                    myXmlTextWriter.WriteEndElement();
                }
            }
            myXmlTextWriter.WriteEndElement();
            myXmlTextWriter.Flush();
            myXmlTextWriter.Close();
        }

        public void parametersListener(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Z")
            {
                grammar();
            }
        }

    }
}
