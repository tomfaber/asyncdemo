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

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace AsyncDemo.UnitTests
{
    [TestClass]
    public class TestTheMock
    {
        int plentyOfTime = 100000; // default wait time for async operations.
        MockAsyncThing mat;
        ManualResetEvent aCallbackCalled = new ManualResetEvent(false);
        ManualResetEvent bCallbackCalled = new ManualResetEvent(false);
        ManualResetEvent cCallbackCalled = new ManualResetEvent(false);
        ManualResetEvent dCallbackCalled = new ManualResetEvent(false);
        private int aResult;
        private int bResult;
        private int cResult;
        private int aCallbackThreadId;

        [TestInitialize]
        public void Init()
        {
            mat = new MockAsyncThing();
        }

        [TestCleanup]
        public void Cleanup()
        {
            mat.Dispose();
        }

        [TestMethod]
        public void ValidCase()
        {
            mat.BeginA(ACallback, null);
            aCallbackCalled.WaitOne(plentyOfTime);

            mat.BeginB(aResult, BCallback, null);
            mat.BeginC(aResult, CCallback, null);

            bCallbackCalled.WaitOne(plentyOfTime);
            cCallbackCalled.WaitOne(plentyOfTime);

            if (bResult > cResult)
            {
                mat.BeginD(bResult, cResult, DCallback, null);
                dCallbackCalled.WaitOne(plentyOfTime);
            }
            mat.Validate();
        }

        [TestMethod]
        public void ValidCaseSynchronous()
        {
            var aAsyncResult = mat.BeginA(null, null);
            aAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            Assert.IsTrue(aAsyncResult.IsCompleted, "operation A never completed");
            aResult = mat.EndA(aAsyncResult);

            var bAsyncResult = mat.BeginB(aResult, null, null);
            bAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            Assert.IsTrue(bAsyncResult.IsCompleted, "operation B never completed");
            bResult = mat.EndB(bAsyncResult);

            var cAsyncResult = mat.BeginC(aResult, null, null);
            cAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            Assert.IsTrue(cAsyncResult.IsCompleted, "operation C never completed");
            cResult = mat.EndC(cAsyncResult);

            if (bResult > cResult)
            {
                var dAsyncResult = mat.BeginD(bResult, cResult, null, null);
                dAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
                Assert.IsTrue(dAsyncResult.IsCompleted, "operation D never completed");
                mat.EndD(dAsyncResult);
            }
            mat.Validate();
        }

        [TestMethod]
        public void CallbackOnDifferentThread()
        {
            aCallbackThreadId = -1;
            IAsyncResult aAsyncResult = mat.BeginA(ACallback, null);
            aCallbackCalled.WaitOne(plentyOfTime);
            Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, aCallbackThreadId);
            Assert.AreNotEqual(-1, aCallbackThreadId, "aCallbackThreadId not set");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PassInWrongAsyncResultToEnd()
        {
            IAsyncResult aAsyncResult = mat.BeginA(null, null);
            aAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            int aResult = mat.EndA(aAsyncResult);

            IAsyncResult bAsyncResult = mat.BeginB(aResult, null, null);
            bAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            mat.EndB(aAsyncResult);
        }

        [TestMethod]
        [ExpectedException(typeof(AsyncDemoValidationException))]
        public void BeginBCalledWithWrongInput()
        {
            int aResult = CallASynchronously();
            mat.BeginB(aResult + 1, (ar) => { mat.EndB(ar); }, null);
            mat.Validate(false);
        }

        [TestMethod]
        public void CallCBeforeB()
        {
            int aResult = CallASynchronously();
            CallCSynchronously(aResult);
            CallBSynchronously(aResult);
            mat.Validate(false);
        }

        [TestMethod]
        public void CallBBeforeC()
        {
            int aResult = CallASynchronously();
            CallBSynchronously(aResult);
            CallCSynchronously(aResult);
            mat.Validate(false);
        }

        [TestMethod]
        public void CallBAndCInParallel()
        {
            int aResult = CallASynchronously();
            mat.BeginB(aResult, (ar) => { mat.EndB(ar); }, null);
            mat.BeginC(aResult, (ar) => { mat.EndC(ar); }, null);
            mat.Validate(false);
        }

        private int CallASynchronously()
        {
            var aAsyncResult = mat.BeginA(null, null);
            aAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            return mat.EndA(aAsyncResult);
        }

        private int CallBSynchronously(int aResult)
        {
            var bAsyncResult = mat.BeginB(aResult, null, null);
            bAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            return mat.EndB(bAsyncResult);
        }

        private int CallCSynchronously(int aResult)
        {
            var cAsyncResult = mat.BeginC(aResult, null, null);
            cAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            return mat.EndC(cAsyncResult);
        }

        private int CallDSynchronously(int bResult, int cResult)
        {
            var dAsyncResult = mat.BeginD(bResult, cResult, null, null);
            dAsyncResult.AsyncWaitHandle.WaitOne(plentyOfTime);
            return mat.EndD(dAsyncResult);
        }

        [TestMethod]
        [ExpectedException(typeof(AsyncDemoValidationException))]
        public void ABeginCalledTwice()
        {
            IAsyncResult foo = mat.BeginA(null, null);
            try
            {
                mat.BeginA(null, null);
            }
            finally
            {
                foo.AsyncWaitHandle.WaitOne(plentyOfTime);
                mat.EndA(foo);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(AsyncDemoValidationException))]
        public void FailsIfCNotCalled()
        {
            int aResult = CallASynchronously();
            CallBSynchronously(aResult);
            mat.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(AsyncDemoValidationException))]
        public void FailsIfCCalledWithWrongInput()
        {
            int aResult = CallASynchronously();
            int bResult = CallBSynchronously(aResult);
            CallCSynchronously(bResult);
            mat.Validate(false);
        }

        #region D Tests

        [TestMethod]
        public void ValidCaseWhereDShouldBeCalled()
        {
            mat.DShouldBeCalled = true;
            int aResult = CallASynchronously();
            int bResult = CallBSynchronously(aResult);
            int cResult = CallCSynchronously(aResult);
            CallDSynchronously(bResult, cResult);

            mat.Validate();
        }

        [TestMethod]
        public void ValidCaseWhereDShouldNotBeCalled()
        {
            mat.DShouldBeCalled = false;
            int aResult = CallASynchronously();
            CallBSynchronously(aResult);
            CallCSynchronously(aResult);
            mat.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(AsyncDemoValidationException))]
        public void DCalledWithBAndCInputSwitched()
        {
            mat.DShouldBeCalled = true;
            int aResult = CallASynchronously();
            int bResult = CallBSynchronously(aResult);
            int cResult = CallCSynchronously(aResult);
            CallDSynchronously(cResult, bResult);

            mat.Validate(false);
        }

        [TestMethod]
        public void SettingDShouldBeCalledTrue()
        {
            mat.DShouldBeCalled = true;
            int aResult = CallASynchronously();
            int bResult = CallBSynchronously(aResult);
            int cResult = CallCSynchronously(aResult);
            Assert.IsTrue(mat.DShouldBeCalled);
            Assert.IsTrue(bResult > cResult);
        }

        [TestMethod]
        public void SettingDShouldBeCalledFalse()
        {
            mat.DShouldBeCalled = false;
            int aResult = CallASynchronously();
            int bResult = CallBSynchronously(aResult);
            int cResult = CallCSynchronously(aResult);
            Assert.IsFalse(mat.DShouldBeCalled);
            Assert.IsFalse(bResult > cResult);
        }

        [TestMethod]
        [ExpectedException(typeof(AsyncDemoValidationException))]
        public void DCalledWhenItShouldnt()
        {
            mat.DShouldBeCalled = false;
            int aResult = CallASynchronously();
            int bResult = CallBSynchronously(aResult);
            int cResult = CallCSynchronously(aResult);
            CallDSynchronously(bResult, cResult);

            mat.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(AsyncDemoValidationException))]
        public void DNotCalledWhenItShould()
        {
            mat.DShouldBeCalled = true;
            int aResult = CallASynchronously();
            CallBSynchronously(aResult);
            CallCSynchronously(aResult);
            mat.Validate();
        }

        #endregion D Tests

        public void ACallback(IAsyncResult ar)
        {
            aCallbackThreadId = Thread.CurrentThread.ManagedThreadId;
            aResult = mat.EndA(ar);
            aCallbackCalled.Set();
        }

        public void BCallback(IAsyncResult ar)
        {
            bResult = mat.EndB(ar);
            bCallbackCalled.Set();
        }

        public void CCallback(IAsyncResult ar)
        {
            cResult = mat.EndC(ar);
            cCallbackCalled.Set();
        }

        public void DCallback(IAsyncResult ar)
        {
            mat.EndD(ar);
            dCallbackCalled.Set();
        }
    }
}