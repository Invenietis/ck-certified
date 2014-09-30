#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\WordPredictorTest\WordPredictorTest.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Plugin.Config;
using CK.WordPredictor;
using Moq;
using NUnit.Framework;

namespace WordPredictorTest
{
    [TestFixture]
    public class WordPredictorTest
    {
        [Test]
        public void WordPredictorServiceTest()
        {
            TextualContextService t = new TextualContextService();

            WordPredictorServiceBase.PluginDirectoryPath = TestHelper.SybilleResourceFullPath;
            WordPredictorServiceBase w = new SybilleWordPredictorService()
            {
                Feature = TestHelper.MockFeature( 10 ).Object,
                TextualContextService = t
            };

            w.Setup( null );
            w.Start();

            Task.WaitAll( w.AsyncEngineContinuation );

            t.SetRawText( "Je" );
            //Task.WaitAll( w.AsyncEngineContinuation );
            Assert.That( w.Words.Count > 0 );
            Console.WriteLine( String.Join( " ", w.Words.Select( o => o.Word ).ToArray() ) );
            t.SetRawText( "Je " );
            t.SetRawText( "Je Bon" );
            Assert.That( w.Words.Count > 0 );
            Console.WriteLine( String.Join( " ", w.Words.Select( o => o.Word ).ToArray() ) );

            w.Stop();
        }
    }
}
