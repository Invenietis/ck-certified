﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CK.Keyboard.Model;
using CK.WPF.ViewModel;

namespace KeyboardEditor.Wizard.ViewModels
{
    public class ExportKeyboardViewModel : VMBase
    {
        readonly IKeyboardCollection _keyboards;
        readonly ExportKeyboard _owner;

        List<CheckBoxExportKeyboardViewModel> _checkBoxs;
        string _fileName;
        bool _canExecute;

        public ExportKeyboardViewModel( ExportKeyboard owner, IKeyboardCollection keyboards )
        {
            _checkBoxs = new List<CheckBoxExportKeyboardViewModel>();
            _owner = owner;
            _keyboards = keyboards;
            _canExecute = true;
            CreateCheckBox();
            SaveCommand = new VMCommand( ShowSaveWindow, o => CanExecute );
        }

        public bool CanExecute
        {
            get { return _canExecute; }
            set
            {
                if( value != _canExecute )
                {
                    _canExecute = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<CheckBoxExportKeyboardViewModel> CheckBoxs
        {
            get { return _checkBoxs; }
        }

        void CreateCheckBox()
        {
            foreach( var k in _keyboards )
            {
                _checkBoxs.Add( new CheckBoxExportKeyboardViewModel( k ) );
            }
        }

        public ICommand SaveCommand
        {
            get;
            internal set;
        }

        void ShowSaveWindow()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Keyboards"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "CiviKey keyboard (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if( result == true )
            {
                SaveInXml( dlg.FileName );
            }
        }

        void SaveInXml( string fileName )
        {
            CanExecute = false;

            List<IKeyboard> keyboardsToSerialize = new List<IKeyboard>();
            // Save document
            foreach( var cb in _checkBoxs )
            {
                if( cb.IsSelected )
                {
                    keyboardsToSerialize.Add( cb.Keyboard );
                }
            }
            _owner.ExportKeyboards( fileName, keyboardsToSerialize );

            CanExecute = true;
        }
    }
}
