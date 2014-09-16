//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CK.Plugin;
//using CK.WindowManager.Model;
//using CK.Windows;

//namespace CK.WindowFactory
//{
//    public class WindowFactory : IPlugin 
//    {

//        #region IPlugin Members

//        public bool Setup( IPluginSetupInfo info )
//        {
//            throw new NotImplementedException();
//        }

//        public void Start()
//        {
//            throw new NotImplementedException();
//        }

//        public void Stop()
//        {
//            throw new NotImplementedException();
//        }

//        public void Teardown()
//        {
//            throw new NotImplementedException();
//        }

//        #endregion
//        class SkinInfo
//        {
//            public SkinInfo( SkinWindow window, VMContextActiveKeyboard vm, Dispatcher d, WindowManagerSubscriber sub )
//            {
//                Skin = window;
//                ViewModel = vm;
//                Dispatcher = d;
//                Subscriber = sub;
//                NameKeyboard = vm.KeyboardVM.Keyboard.Name;
//            }

//            public readonly WindowManagerSubscriber Subscriber;

//            public readonly CKWindow Skin;

//            public readonly VMContextActiveKeyboard ViewModel;

//            public readonly Dispatcher Dispatcher;

//            /// <summary>
//            /// must be updated manual
//            /// </summary>
//            public string NameKeyboard;

//            public bool IsClosing;
//        }
//    }
//}
