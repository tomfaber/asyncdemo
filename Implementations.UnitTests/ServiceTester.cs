/* Copyright 2013 Tom Faber

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.  */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace AsyncDemo.Implementations.UnitTests
{
    class ServiceTester
    {
        const int plentyOfTime = 100000;

        private MockAsyncThing mat = new MockAsyncThing();
        private IWebService serviceUnderTest;

        public ServiceTester(Func<MockAsyncThing, IWebService> serviceFactory)
        {
            mat = new MockAsyncThing();
            serviceUnderTest = serviceFactory(mat);
        }

        public void WhenDCalled()
        {
            mat.DShouldBeCalled = true;
            IAsyncResult iar = serviceUnderTest.BeginPublicApi(null, null);
            iar.AsyncWaitHandle.WaitOne(plentyOfTime);
            int result = serviceUnderTest.EndPublicApi(iar);
            mat.Validate();
            Assert.AreEqual(mat.ExpectedResult, result);
        }

        public void WhenDNotCalled()
        {
            mat.DShouldBeCalled = false;
            IAsyncResult iar = serviceUnderTest.BeginPublicApi(null, null);
            iar.AsyncWaitHandle.WaitOne(plentyOfTime);
            int result = serviceUnderTest.EndPublicApi(iar);
            mat.Validate();
            Assert.AreEqual(mat.ExpectedResult, result);
        }

        public void CallbackShouldBeCalled()
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            IAsyncResult iar = serviceUnderTest.BeginPublicApi((unused) => mre.Set(), null);
            iar.AsyncWaitHandle.WaitOne(plentyOfTime);
            int result = serviceUnderTest.EndPublicApi(iar);
            Assert.IsTrue(mre.WaitOne(plentyOfTime));
        }

        public void PreserveAsyncState()
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            object expectedState = new object();
            object statePassedIntoCallback = null;
            IAsyncResult iar = serviceUnderTest.BeginPublicApi((iar2) =>
            {
                statePassedIntoCallback = iar2.AsyncState;
                mre.Set();
            }, expectedState);
            Assert.IsTrue(iar.AsyncWaitHandle.WaitOne(plentyOfTime));
            int result = serviceUnderTest.EndPublicApi(iar);
            Assert.IsTrue(mre.WaitOne(plentyOfTime));
            Assert.AreSame(expectedState, statePassedIntoCallback);
        }
    }
}