using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace CK_WindowManager
{
    public class ScreenInformations:IScreenInformations, INotifyPropertyChanged
    {
        private Rectangle _bounds;
        private Rectangle _workingArea;
        private bool _isPrimaryScreen;
        private string _deviceName;

        public ScreenInformations(Rectangle bounds,Rectangle workingArea,bool isPrimaryScreen,string deviceName)
        {
            this._bounds=bounds;
            this._workingArea=workingArea;
            this._deviceName = deviceName;
            this._isPrimaryScreen = isPrimaryScreen;
        }

        public Rectangle Bounds
        { 
            get 
            { 
                return _bounds; 
            } 
            set 
            {
                if (_bounds!= value)
                {
                    _bounds = value;
                    NotifyPropertyChanged("Bounds");
                }
            } 
        }

        public Rectangle WorkingArea
        {
            get
            {
                return _workingArea;
            }
            set
            {
                if (_workingArea != value)
                {
                    _workingArea = value;
                    NotifyPropertyChanged("WorkingArea");
                }
            } 
        }

        public string DeviceName
        {
            get
            {
                return _deviceName;
            }
            set
            {
                if (_deviceName != value)
                {
                    _deviceName= value;
                    NotifyPropertyChanged("DeviceName");
                }
            }
        }
        public override string ToString()
        {
            return _deviceName;
        }

        public bool IsPrimaryScreen
        {
            get
            {
                return _isPrimaryScreen;
            }
            set
            {
                if (_isPrimaryScreen!=value)
                {
                    _isPrimaryScreen = value;
                    NotifyPropertyChanged("IsPrimaryScreen");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        
    }
}
