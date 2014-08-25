#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Prediction\CK.WordPredictor\TextualContext\PredictionTextAreaBus.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using CK.Plugin;
using CK.Plugins.SendInputDriver;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{55C2A080-30EB-4CC6-B602-FCBBF97C8BA5}", PublicName = "WordPrediction - TextArea Bus", Categories = new string[] { "Prediction", "Advcanced" } )]
    public class PredictionTextAreaBus : BasicCommandHandler, IPredictionTextAreaService
    {
        string _text;
        int _caretIndex;

        public const string CMDSendPredictionAreaContent = "sendPredictionAreaContent";

        public event EventHandler<PredictionAreaContentEventArgs> PredictionAreaContentChanged;
        
        public event EventHandler<PredictionAreaContentEventArgs> PredictionAreaTextSent;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ISendStringService> SendStringService { get; set; }

        public override void Start()
        {
            base.Start();
            PredictionAreaContentChanged += OnPredictionAreaContentChanged;

        }

        public override void Stop()
        {
            PredictionAreaContentChanged -= OnPredictionAreaContentChanged;
            base.Stop();
        }

        protected virtual void OnPredictionAreaContentChanged( object sender, PredictionAreaContentEventArgs e )
        {
            _text = e.Text;
            _caretIndex = e.CaretIndex;
        }

        void IPredictionTextAreaService.ChangePredictionAreaContent( string text, int caretIndex )
        {
            if( _text != text || _caretIndex != caretIndex )
            {
                if( PredictionAreaContentChanged != null )
                    PredictionAreaContentChanged( this, new PredictionAreaContentEventArgs( text, caretIndex ) );
            }
        }

        void IPredictionTextAreaService.SendText()
        {
            if( PredictionAreaTextSent != null )
                PredictionAreaTextSent( this, new PredictionAreaContentEventArgs( _text, _caretIndex ) );
        }

        protected override void OnCommandSent( object sender, CommandSentEventArgs e )
        {
            if( e.Command != null && e.Command.Contains( CMDSendPredictionAreaContent ) )
                ((IPredictionTextAreaService)this).SendText();
        }

        public event EventHandler<IsDrivenChangedEventArgs> IsDrivenChanged;
        bool _isDriven;
        public bool IsDriven
        {
            get
            {
                return _isDriven;
            }
            set
            {
                _isDriven = value;
                if( IsDrivenChanged != null ) IsDrivenChanged( this, new IsDrivenChangedEventArgs( value ) );
            }
        }

    }
}
