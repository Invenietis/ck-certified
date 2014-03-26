﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Animation;
using CK.WPF.ViewModel;
using CommonServices;
using ProtocolManagerModel;
using CK.Plugin.Config;
using System.Windows.Input;
using CK.Plugins.SendInputDriver;

namespace BasicCommandHandlers
{
    public class TextTemplateCommandParameterManager : IProtocolParameterManager
    {
        string _template;
        string _placeholder;
        int _caretIndex;
        bool _isFocused;

        VMCommand _insertPlacholder;
        ITextTemplateService _textTemplate;

        public TextTemplateCommandParameterManager(ITextTemplateService textTemplate, ISendStringService sendString)
        {
            _textTemplate = textTemplate;
            _placeholder = "";

            _insertPlacholder = new VMCommand( () => {
                if( string.IsNullOrEmpty( _placeholder ) ) return;
                if( !IsFocused ) return;

                sendString.SendString( _textTemplate.OpentTag + _placeholder + _textTemplate.CloseTag );
                
                NotifyPropertyChanged( "Template" );
                NotifyPropertyChanged( "IsValid" );
            } );
        }

        public int TemplateCaretIndex
        {
            get { return _caretIndex; }
            set
            {
                _caretIndex = value;
                NotifyPropertyChanged( "TemplateCaretIndex" );
            }
        }

        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                NotifyPropertyChanged( "IsFocused" );
            }
        }
        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value;
                NotifyPropertyChanged( "Placeholder" );
            }
        }

        public string Template
        {
            get { return _template; }
            set
            {
                _template = value;
                NotifyPropertyChanged( "Template" );
                NotifyPropertyChanged( "TemplateCaretIndex" );
                NotifyPropertyChanged( "IsValid" );
            }
        }

        internal void NotifyPropertyChanged( string peropertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( peropertyName ) );
        }

        #region IProtocolParameterManager Members

        public void FillFromString( string parameter )
        {
            Template = parameter;
        }

        public string GetParameterString()
        {
            return Template;
        }
        
        public bool IsValid
        {
            get 
            {
                return !string.IsNullOrEmpty(Template);
            }
        }

        public ICommand InsertPlaceholder 
        {
            get { return _insertPlacholder; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IProtocolParameterManager Members

        public IProtocolEditorRoot Root { get; set; }

        #endregion
    }

}
