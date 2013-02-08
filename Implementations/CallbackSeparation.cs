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
using System.Threading;

namespace AsyncDemo.Implementations
{
    /// <summary>
    /// Compare this with AnonymousDelegates: each individual method is much shorter and clearer, but
    /// how they all fit together is harder to understand.
    /// </summary>
    public class CallbackSeparation : IWebService
    {
        private MockAsyncThing _mat;
        private CustomAsyncResult _asyncResult;
        private int _finalResultValue;
        private AsyncCallback _userCallback;
        private int _bResultValue;
        private int _cResultValue;
        private int _bAndCCalled = 0;

        public CallbackSeparation(MockAsyncThing mat)
        {
            _mat = mat;
        }

        public IAsyncResult BeginPublicApi(AsyncCallback callback, object asyncState)
        {
            _asyncResult = new CustomAsyncResult(asyncState);
            _userCallback = callback;
            _mat.BeginA(CallbackFromA, null);

            return _asyncResult;
        }

        private void CallbackFromA(IAsyncResult aAsyncResult)
        {
            int aResultValue = _mat.EndA(aAsyncResult);
            _mat.BeginB(aResultValue, CallbackFromB, null);
            _mat.BeginC(aResultValue, CallbackFromC, null);
        }

        private void CallbackFromB(IAsyncResult ar)
        {
            _bResultValue = _mat.EndB(ar);
            ProcessResultsFromBAndC();
        }

        private void CallbackFromC(IAsyncResult ar)
        {
            _cResultValue = _mat.EndC(ar);
            ProcessResultsFromBAndC();
        }

        private void ProcessResultsFromBAndC()
        {
            if (Interlocked.Increment(ref _bAndCCalled) == 2)
            {
                if (_bResultValue > _cResultValue)
                {
                    _mat.BeginD(_bResultValue, _cResultValue, CallbackFromD, null);
                }
                else
                {
                    int finalResult = _cResultValue;
                    SetComplete(finalResult);
                }
            }
        }

        public void CallbackFromD(IAsyncResult iarD)
        {
            SetComplete(_mat.EndD(iarD));
        }

        private void SetComplete(int finalResult)
        {
            _finalResultValue = finalResult;
            _asyncResult.Complete();
            if (_userCallback != null) _userCallback(_asyncResult);
        }

        public int EndPublicApi(IAsyncResult asyncResult)
        {
            return _finalResultValue;
        }
    }
}