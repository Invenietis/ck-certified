using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    public class KeyCommandTypeViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Valid Constructor for a KeyCommandType
        /// </summary>
        /// <param name="innerName"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public KeyCommandTypeViewModel( string innerName, string name, string description, Type keyCommandParameterType )
        {
            if( !typeof( IKeyCommandParameterManager ).IsAssignableFrom( keyCommandParameterType ) ) throw new ArgumentException( String.Format( "The keyCommandParameterType ({0}) for the KeyCommandType {1} doesn't implement IKeyCommandParameter", keyCommandParameterType.ToString(), innerName ) );

            InnerName = innerName;
            Name = name;
            Description = description;
            KeyCommandParameterType = keyCommandParameterType;
        }

        public KeyCommandTypeViewModel()
        {
        }

        /// <summary>
        /// Gets whether the current KeyCommandType InnerName is recognized by a registered command handler
        /// </summary>
        public bool IsValid { get { return !String.IsNullOrWhiteSpace( InnerName ); } }

        /// <summary>
        /// Gets a user-friendly name (displayed to the user : must be multilingual)
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// A description of the command (displayed to the user : must be multilingual)
        /// </summary>
        public string Description { get; internal set; }

        public string InnerName { get; set; }

        public Type KeyCommandParameterType { get; internal set; }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
