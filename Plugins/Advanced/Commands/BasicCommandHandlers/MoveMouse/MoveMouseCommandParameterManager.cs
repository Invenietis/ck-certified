using BasicCommandHandlers.Resources;
using CK.Keyboard.Model;
using CK.Plugins.SendInputDriver;
using ProtocolManagerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicCommandHandlers
{
    /// <summary>
    /// Implementation of the IKeyCommandParameter interface that defines a direction (up, right, bottom, left etc..)
    /// </summary>
    public class MoveMouseCommandParameterManager : IProtocolParameterManager
    {
        public MoveMouseCommandParameterManager()
        {
            _values = new Dictionary<string, string>() 
            { 
                { "U", R.Up }, 
                { "R", R.Right }, 
                { "B", R.Bottom }, 
                { "L", R.Left }, 
                { "UL", R.UpLeft },
                { "UR", R.UpRight }, 
                { "DR", R.BottomRight }, 
                { "DL", R.BottomLeft  } 

                //{ R.Up, "U"  }, 
                //{ R.Right, "R" }, 
                //{ R.Bottom, "B" }, 
                //{ R.Left, "L"  }, 
                //{ R.UpLeft, "UL" },
                //{ R.UpRight, "UR" }, 
                //{ R.BottomRight, "DR" }, 
                //{ R.BottomLeft, "DL" } 
            };
        }

        IDictionary<string, string> _values;
        public IEnumerable<string> AvailableValues { get { return _values.Values; } }

        string _selectedDirection;
        public string SelectedDirection
        {
            get { return _selectedDirection; }
            set
            {
                _selectedDirection = value;
                OnPropertyChanged( "SelectedDirection" );
                OnPropertyChanged( "IsValid" );
            }
        }

        int _selectedSpeed;
        public int SelectedSpeed
        {
            get { return _selectedSpeed; }
            set
            {
                _selectedSpeed = value;
                OnPropertyChanged( "SelectedSpeed" );
                OnPropertyChanged( "IsValid" );
            }
        }

        public void FillFromString( string parameter )
        {
            string selectedDirection = null;
            int selectedSpeed = 0;

            string[] splittedParameter = parameter.Split( ',' );
            if( splittedParameter.Length == 2 )
            {
                if( _values.TryGetValue( splittedParameter[0].Trim(), out selectedDirection ) )
                {
                    SelectedDirection = selectedDirection;
                }
                else SelectedDirection = null;

                if( Int32.TryParse( splittedParameter[1].Trim(), out selectedSpeed ) )
                {
                    SelectedSpeed = selectedSpeed;
                }
                else SelectedSpeed = 0;
            }
        }

        private void OnPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( propertyName ) );
        }

        public bool IsValid { get { return !String.IsNullOrWhiteSpace( SelectedDirection ) && SelectedSpeed > 0; } }

        public string GetParameterString()
        {
            if( SelectedDirection == null || SelectedSpeed <= 0 ) return String.Empty;
            return String.Format( "{0}, {1}", _values.Where( x => x.Value == SelectedDirection ).Single().Key, SelectedSpeed );
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
