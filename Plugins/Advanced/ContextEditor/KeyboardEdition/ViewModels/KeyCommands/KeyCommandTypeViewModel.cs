﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KeyboardEditor.KeyboardEdition
{
    public class KeyCommandTypeViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Valid Constructor for a KeyCommandType, this object must be capable of creating on the fly its KeyCommandParameterManager.
        /// Here by using Activator.CreateInstance(Type).
        /// </summary>
        /// <param name="protocol">The protocol that is handled.</param>
        /// <param name="name">The name displayed in the list of available editors. Must be multilingual.</param>
        /// <param name="description">A description of the protocol that is handled by this object. Must be multilingual.</param>
        /// <param name="keyCommandParameterManagerType">The type of the IKeyCommandParameterMaanger that handles the parameters of this protocol. Must implement IKeyCommandParameter.</param>
        public KeyCommandTypeViewModel( string protocol, string name, string description, Type keyCommandParameterManagerType )
        {
            if( !typeof( IKeyCommandParameterManager ).IsAssignableFrom( keyCommandParameterManagerType ) ) throw new ArgumentException( String.Format( "The keyCommandParameterType ({0}) for the KeyCommandType {1} doesn't implement IKeyCommandParameter", keyCommandParameterManagerType.ToString(), protocol ) );

            Protocol = protocol;
            Name = name;
            Description = description;
            CreateParameterManager = new Func<IKeyCommandParameterManager>( () => { return (IKeyCommandParameterManager)Activator.CreateInstance( keyCommandParameterManagerType ); } );
        }

        internal Func<IKeyCommandParameterManager> CreateParameterManager { get; private set; }

        /// <summary>
        /// Valid Constructor for a KeyCommandType, this object must be capable of creating on the fly its KeyCommandParameterManager.
        /// Here by using the Func set as parameter.
        /// </summary>
        /// <param name="protocol">The protocol that is handled.</param>
        /// <param name="name">The name displayed in the list of available editors. Must be multilingual.</param>
        /// <param name="description">A description of the protocol that is handled by this object. Must be multilingual.</param>
        /// <param name="keyCommandParameterManagerFunc">A Func that returns an instance of an implementation of IKeyCommandParameterManager that handles a protocol. Must not return null.</param>
        public KeyCommandTypeViewModel( string protocol, string name, string description, Func<IKeyCommandParameterManager> keyCommandParameterManagerFunc )
        {
            
            Protocol = protocol;
            Name = name;
            Description = description;
            CreateParameterManager = keyCommandParameterManagerFunc;
        }

        /// <summary>
        /// Internal ctor used to create an invalid KeyCommandTypeViewModel
        /// </summary>
        internal KeyCommandTypeViewModel( string protocol, string name )
        {
            Protocol = protocol;
            Name = name;
        }

        /// <summary>
        /// Gets whether the current KeyCommandType Protocol is recognized by a registered command handler
        /// </summary>
        public bool IsValid { get { return !String.IsNullOrWhiteSpace( Protocol ); } }

        /// <summary>
        /// Gets a user-friendly name (displayed to the user : must be multilingual)
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A description of the command (displayed to the user : must be multilingual)
        /// </summary>
        public string Description { get; internal set; }

        public string Protocol { get; private set; }

        ///// <summary>
        ///// The object that will handle the parameter of the KeyCommand handled by this object.
        ///// This is where the actual logic of a KeyCommand protocol handler lies.
        ///// </summary>
        //public IKeyCommandParameterManager KeyCommandParameter { get; private set; }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
