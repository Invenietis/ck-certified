#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\WordPredictorTest\WordPredictorEngineFactoryTest.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using CK.WordPredictor;
using CK.WordPredictor.Model;
using CK.WordPredictor.Engines;
using NUnit.Framework;
using CK.Core;

namespace WordPredictorTest
{
    [TestFixture]
    public class WordPredictorEngineFactoryTest
    {
        [Test]
        public void Create_Should_Throw_Exception_If_No_Engine_With_The_Given_Name_Is_Available()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath, TestHelper.MockFeature( 8 ).Object );
            Assert.Throws<ArgumentException>( () => f.Create( "lucene" ) );
        }

        [Test]
        public void Sybillye_And_Semantic_Sybille_Are_Available()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath, TestHelper.MockFeature( 8 ).Object );
            Assert.That( f.Create( "sybille" ), Is.Not.Null );
            Assert.That( f.Create( "sem-sybille" ), Is.Not.Null );
        }

        class DisposableEngine : IWordPredictorEngine, IDisposable
        {
            public bool DisposedCalled { get; private set; }
            public void Dispose()
            {
                DisposedCalled = true;
            }

            public bool IsWeightedPrediction
            {
                get { throw new NotImplementedException(); }
            }

            public ICKReadOnlyList<IWordPredicted> Predict( string rawContext, int maxSuggestedWord )
            {
                throw new NotImplementedException();
            }

            public System.Threading.Tasks.Task<ICKReadOnlyList<IWordPredicted>> PredictAsync( string rawContext, int maxSuggestedWords )
            {
                throw new NotImplementedException();
            }

            public string ObtainRawContext( ITextualContextService textualContextService )
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Release_Should_Call_Dispose()
        {
            SybilleWordPredictorEngineFactory f = new SybilleWordPredictorEngineFactory( TestHelper.SybilleResourceFullPath, TestHelper.MockFeature( 8 ).Object );
            var engine = new DisposableEngine();
            f.Release( engine );
            Assert.That( engine.DisposedCalled );
        }
    }
}
