#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\WordPredictorTest\SybilleEngineTest.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CK.WordPredictor;
using CK.WordPredictor.Model;
using CK.WordPredictor.Engines;
using NUnit.Framework;

namespace WordPredictorTest
{
    [TestFixture]
    public class SybilleEngineTest
    {

        [Test]
        public void Sybille_Dictionnary_Should_Load_And_Provide_Results()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath, TestHelper.MockFeature( 8 ).Object );
            IWordPredictorEngine engine = f.Create( "sybille" );
            TestEngine( engine );
            f.Release( engine );
        }

        [Test]
        public void Sem_Sybille_Should_Load_And_Provide_Results()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath, TestHelper.MockFeature( 8 ).Object );
            IWordPredictorEngine engine = f.Create( "sem-sybille" );
            TestEngine( engine );
            f.Release( engine );
        }

        private static void TestEngine( IWordPredictorEngine engine )
        {
            TextualContextService textualContextService = new TextualContextService();
            textualContextService.SetRawText( "Je" );
            string context= textualContextService.GetTextualContext();
            var predicted = engine.Predict( context, 20 );
            Assert.That( predicted, Is.Not.Null );
            Assert.That( predicted.Count() > 0 );
        }

    }
}
