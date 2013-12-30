using BasicCommandHandlers.Resources;
using CK.Plugins.SendInputDriver;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicCommandHandlers
{
    /// <summary>
    /// Most simple implementation of the IKeyCommandParameter interface.
    /// The parameter is set as string value.
    /// Getting the Command string returns the value.
    /// This implementation is all the sendString command needs.
    /// </summary>
    public class SendKeyCommandParameterManager : IProtocolParameterManager
    {
        public SendKeyCommandParameterManager()
        {
            //TODO : use resx
            _values = new Dictionary<string, CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys>();

            _values.Add( "+ (addition)", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Add );
            _values.Add( "- (soustraction)", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Subtract );
            _values.Add( "* (multiplication)", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Multiply );
            _values.Add( "/ (division)", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Divide );

            _values.Add( "Entrée", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Return );
            _values.Add( "Retour", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Back );
            _values.Add( "Suppr", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Delete );


            _values.Add( "Flèche vers le haut", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Up );
            _values.Add( "Flèche vers le bas", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Down );
            _values.Add( "Fleche vers la gauche", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Left );
            _values.Add( "Flèche vers la droite", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Right );

            //TODO : Where is the "beginning of the line" key ?
            _values.Add( "Echap", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Escape );
            _values.Add( "Fin", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.End );
            _values.Add( "Insert", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Insert );
            _values.Add( "Tabulation", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Tab );

            _values.Add( "Page bas", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.PageDown );
            _values.Add( "Page haut", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.PageUp );

            _values.Add( "Impr écran", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.PrintScreen );
            _values.Add( "Verrouillage pavé numérique", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.NumLock );

            _values.Add( "F1", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F1 );
            _values.Add( "F2", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F2 );
            _values.Add( "F3", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F3 );
            _values.Add( "F4", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F4 );
            _values.Add( "F5", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F5 );
            _values.Add( "F6", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F6 );
            _values.Add( "F7", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F7 );
            _values.Add( "F8", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F8 );
            _values.Add( "F9", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F9 );
            _values.Add( "F10", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F10 );
            _values.Add( "F11", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F11 );
            _values.Add( "F12", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.F12 );

            _values.Add( "Pause", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Pause );
            _values.Add( "Lecture", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.Play );

            _values.Add( "Baisser son", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.VolumeDown );
            _values.Add( "Augmenter son", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.VolumeUp );
            _values.Add( "Stopper son", CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys.VolumeMute );
        }

        public IProtocolEditorRoot Root { get; set; }
        Dictionary<string, CK.Plugins.SendInputDriver.NativeMethods.KeyboardKeys> _values;
        public IEnumerable<string> AvailableValues { get { return _values.Keys; } }

        string _selectedValue;
        public string SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                _selectedValue = value;
                OnPropertyChanged( "SelectedValue" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public string Title { get { return R.SendKeyProtocolSubtitle; } }

        public void FillFromString( string parameter )
        {
            NativeMethods.KeyboardKeys enumValue = NativeMethods.KeyboardKeys.A;

            if( Enum.TryParse<NativeMethods.KeyboardKeys>( parameter, out enumValue ) )
                SelectedValue = _values.Where( kvp => kvp.Value == enumValue ).Single().Key;
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid
        {
            get
            {
                NativeMethods.KeyboardKeys enumValue = NativeMethods.KeyboardKeys.A;
                return SelectedValue != null && _values.TryGetValue(SelectedValue, out enumValue);
            }
        }

        public string GetParameterString()
        {
            if( String.IsNullOrWhiteSpace( SelectedValue ) ) return String.Empty;
            return _values[SelectedValue].ToString();
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
