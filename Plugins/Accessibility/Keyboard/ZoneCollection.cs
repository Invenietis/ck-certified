#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\Keyboard\ZoneCollection.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Xml;
using System.Collections.Generic;
using CK.Core;
using CK.Keyboard.Model;
using CK.Context;
using CK.Storage;
using System.Linq;

namespace CK.Keyboard
{
    class ZoneCollection : IZoneCollection, IEnumerable<Zone>, IStructuredSerializable
    {
        Keyboard _kb;
        //Dictionary<string, Zone> _zones;
        IList<Zone> _zonesl;
        Zone _defaultZone;

        public event EventHandler<ZoneEventArgs> ZoneCreated;
        public event EventHandler<ZoneEventArgs> ZoneDestroyed;
        public event EventHandler<ZoneEventArgs> ZoneRenamed;
        public event EventHandler<ZoneEventArgs> ZoneMoved;

        internal ZoneCollection( Keyboard kb )
        {
            _kb = kb;
            _defaultZone = new Zone( this, String.Empty, 0 );
            //_zones = new Dictionary<string, Zone>();
            _zonesl = new List<Zone>();

            //_zones.Add( _defaultZone.Name, _defaultZone );
            _zonesl.Add( _defaultZone );
        }

        IKeyboardContext IZoneCollection.Context
        {
            get { return _kb.Context; }
        }

        internal KeyboardContext Context
        {
            get { return _kb.Context; }
        }

        IKeyboard IZoneCollection.Keyboard
        {
            get { return _kb; }
        }

        internal Keyboard Keyboard
        {
            get { return _kb; }
        }

        public bool Contains( object item )
        {
            IZone z = item as IZone;
            return z != null && z.Keyboard == Keyboard;
        }

        public int Count
        {
            get { return _zonesl == null ? 1 : _zonesl.Count; }
        }

        IEnumerator<IZone> IEnumerable<IZone>.GetEnumerator()
        {
            Converter<Zone, IZone> conv = delegate( Zone l ) { return (IZone)l; };
            //return Wrapper<IZone>.CreateEnumerator( _zones.Values, conv );
            return Wrapper<IZone>.CreateEnumerator( _zonesl, conv );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _zonesl.GetEnumerator();
            //return _zones.Values.GetEnumerator();
        }

        public IEnumerator<Zone> GetEnumerator()
        {
            return _zonesl.GetEnumerator();
            //return _zones.Values.GetEnumerator();
        }

        IZone IZoneCollection.this[string name]
        {
            get { return _zonesl.Where( z => z.Name == name ).FirstOrDefault(); }
            //get { return this[name]; }
        }

        internal Zone this[string name]
        {
            get
            {
                return _zonesl.Where( z => z.Name == name ).FirstOrDefault();

                //Zone l;
                //_zones.TryGetValue( name, out l );
                //return l;
            }
        }

        IZone IZoneCollection.this[int index]
        {
            get { return _zonesl[index]; }
            //get { return _zones.Values.ToReadOnlyList<IZone>()[index]; }
        }

        internal Zone this[int index]
        {
            get { return _zonesl[index]; }
            //get { return _zones.Values.ToReadOnlyList<Zone>()[index]; }
        }

        IZone IZoneCollection.Default
        {
            get { return _defaultZone; }
        }

        internal Zone Default
        {
            get { return _defaultZone; }
        }

        IZone IZoneCollection.Create( string name )
        {
            return Create( name, -1 );
        }

        IZone IZoneCollection.Create( string name, int index )
        {
            return Create( name, index );
        }

        internal Zone Create( string name, int index )
        {
            name = name.Trim();

            //If the zone's index has not been specified, it is added with the last index
            //if( index == -1 ) index = _zones.Count;
            if( index == -1 ) index = _zonesl.Count;

            //Zone zone = new Zone( this, KeyboardContext.EnsureUnique( name, null, _zones.ContainsKey ), index );
            Predicate<string> exists = new Predicate<string>( ( s ) => _zonesl.Any<Zone>( z => z.Name == s ) );
            Zone zone = new Zone( this, KeyboardContext.EnsureUnique( name, null, exists ), index );

            //_zones.Add( zone.Name, zone );
            _zonesl.Add( zone );

            if( ZoneCreated != null ) ZoneCreated( this, new ZoneEventArgs( zone ) );
            Context.SetKeyboardContextDirty();
            return zone;
        }

        internal void OnDestroy( Zone z )
        {
            Debug.Assert( z.Keyboard == Keyboard && z != Default, "It is not the default." );
            //_zones.Remove( z.Name );
            _zonesl.Remove( z );

            foreach( Layout l in _kb.Layouts ) l.DestroyConfig( z, true );
            if( ZoneDestroyed != null ) ZoneDestroyed( this, new ZoneEventArgs( z ) );
            Context.SetKeyboardContextDirty();
        }

        internal void RenameZone( Zone z, ref string zoneName, string newName )
        {
            Debug.Assert( z.Keyboard == Keyboard && z.Name != newName, "It is not the default" );
            string previous = zoneName;

            Predicate<string> exists = new Predicate<string>( ( s ) => _zonesl.Any<Zone>( zone => zone.Name == s ) );
            newName = KeyboardContext.EnsureUnique( newName, previous, exists ); // _zones.ContainsKey 

            if( newName != previous )
            {
                //_zones.Remove( z.Name );
                _zonesl.Remove( z );

                //_zones.Add( newName, z );
                _zonesl.Add( z );

                zoneName = newName;
                if( ZoneRenamed != null ) ZoneRenamed( this, new ZoneRenamedEventArgs( z, previous ) );
                Context.SetKeyboardContextDirty();
            }
        }

        internal void OnMove( Zone zone, int i )
        {
            Debug.Assert( zone != null && zone.Keyboard == Keyboard, "This is one of our zones." );
            if( i < 0 ) i = 0;
            else if( i > _zonesl.Count ) i = _zonesl.Count;
            int prevIndex = zone.Index;
            if( i == prevIndex ) return;

            Debug.Assert( _zonesl[prevIndex] == zone, "Indices are always synchronized." );
            _zonesl.Insert( i, zone );
            int min, max;
            if( i > prevIndex )
            {
                min = prevIndex;
                max = i;
                Debug.Assert( _zonesl[prevIndex] == zone, "Remove the key" );
                _zonesl.RemoveAt( prevIndex );
            }
            else
            {
                min = i;
                max = prevIndex + 1;
                Debug.Assert( _zonesl[max] == zone, "Remove the key" );
                _zonesl.RemoveAt( max );
            }
            for( int iNew = min; iNew < max; ++iNew )
            {
                _zonesl[iNew].SetIndex( iNew );
            }

            ZoneEventArgs e = new ZoneEventArgs( zone );
            if( ZoneMoved != null ) ZoneMoved( this, e );

            Context.SetKeyboardContextDirty();
        }

        void IStructuredSerializable.ReadContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;

            // We are on the <Zones> tag, we move on to the content
            r.Read();

            int idx = 1; //index 0 is the default zone's index
            while( r.IsStartElement( "Zone" ) )
            {
                // Gets normalized zone name.
                string n = r.GetAttribute( "Name" );
                if( n == null ) n = String.Empty;
                else n = n.Trim();

                Zone z;
                // If empty name, it is the default zone.
                if( n.Length == 0 ) z = _defaultZone;
                else z = Create( n, idx++ );

                sr.ReadInlineObjectStructured( z );
            }
        }

        void IStructuredSerializable.WriteContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            foreach( Zone z in _zonesl )
            {
                sw.WriteInlineObjectStructuredElement( "Zone", z );
            }
        }
    }
}